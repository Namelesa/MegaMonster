using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using MegaMonster.Services.Payment.Application.Payment;
using MegaMonster.Services.Payment.Core.Payment;
using Newtonsoft.Json;

namespace MegaMonster.Services.Payment.Infrastructure.Paymenet;

public class PaymentService(string publicKey, string privateKey, PaymentServiceRepository paymentServiceRepository)
{
    private string CreatePayment(Guid orderId, string userName, double amount, int count, string action)
    {
        var description = $"Register name: {userName}, ticket count: {count}, Sum: {amount} UAH";
        var data = new Dictionary<string, string>
        {
            {"version", PaymentSettings.ApiVersion.ToString()},
            {"public_key", publicKey},
            {"userName", userName },
            {"action", action.ToLower() },
            {"amount", amount.ToString("F2", CultureInfo.InvariantCulture)},
            {"currency", "UAH"},
            {"description", description},
            {"order_id", orderId.ToString()},
            {"result_url", "https://localhost/payment/result"}
        };

        var json = JsonConvert.SerializeObject(data);
        var base64Data = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        var signature = GenerateSignature(base64Data);
        Console.WriteLine($"Signature check: received={signature}, generated={GenerateSignature(base64Data)}");

        
        return $"{PaymentSettings.LiqpayApiCheckoutUrl}?data={base64Data}&signature={signature}";
    }

    private string GenerateSignature(string base64Data)
    {
        using var sha1 = SHA1.Create();
        var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(privateKey + base64Data + privateKey));
        return Convert.ToBase64String(hash);
    }

    public async Task<string> CreatePaymentAsync(Guid orderId, string userName, double amount, int count, string action)
    {
        if (count <= 0 || amount <= 0) return "Count and sum must be > 0";

        var paymentUrl = CreatePayment(orderId, userName, amount, count, action);
        var payment = new Payments
        {
            OrderId = orderId,
            Status = PaymentSettings.IsCreated,
            UserName = userName,
            Count = count,
            Sum = amount,
            CreatedAt = DateTime.UtcNow
        };

        var result = await paymentServiceRepository.AddPayment(payment);
        if (result.Success)
        {
            return paymentUrl;
        }

        return result.Message;
    }

    public async Task<(bool isSuccess, string orderId, string transactionId)> HandlePaymentResultAsync(Dictionary<string, string> requestDictionary)
    {
        if (!requestDictionary.TryGetValue("data", out var base64Data) ||
            !requestDictionary.TryGetValue("signature", out var signature))
        {
            Console.WriteLine("Invalid request data.");
            return (false, null, null);
        }

        var decodedData = Encoding.UTF8.GetString(Convert.FromBase64String(base64Data));
        var requestData = JsonConvert.DeserializeObject<Dictionary<string, string>>(decodedData);
        Console.WriteLine($"Received keys: {string.Join(", ", requestData.Keys)}");

        if (!requestData.TryGetValue("order_id", out var orderId))
        {
            Console.WriteLine("Order ID not found.");
            return (false, null, null);
        }

        if (!requestData.TryGetValue("transaction_id", out var transactionId) &&
            requestData.TryGetValue("liqpay_order_id", out transactionId))
        {
            Console.WriteLine("Using liqpay_order_id as transaction_id");
        }

        if (transactionId == null)
        {
            Console.WriteLine("Transaction ID not found.");
            return (false, orderId, null);
        }

        var payment = await paymentServiceRepository.GetPaymentByOrderId(Guid.Parse(orderId));

        if (signature != GenerateSignature(base64Data))
        {
            Console.WriteLine("Signature mismatch.");
            return (false, orderId, transactionId);
        }

        if (requestData.TryGetValue("status", out var status))
        {
            if (payment.Data.Status == PaymentSettings.IsCreated)
            {
                payment.Data.Status = status switch
                {
                    "success" => PaymentSettings.IsSuccess,
                    "failure" or "error" or "reversed" => PaymentSettings.IsCanceled,
                    _ => PaymentSettings.IsUnknown
                };
                await paymentServiceRepository.UpdatePayment(payment.Data);
            }
            else
            {
                Console.WriteLine("Payment already processed. Skipping update.");
                return (false, orderId, null);
            }

            string successUrl = $"https://www.liqpay.ua/en/checkout/success/{transactionId}";
            return (true, orderId, successUrl);
        }

        Console.WriteLine("Status not found in response.");
        return (false, orderId, transactionId);
    }

    public async Task<bool> CancelPaymentAsync(Guid orderId)
    {
        var payment = await paymentServiceRepository.GetPaymentByOrderId(orderId);
        if (payment.Data == null || payment.Data.Status != PaymentSettings.IsSuccess)
        {
            Console.WriteLine("Payment not found or cannot be refunded.");
            return false;
        }

        var data = new Dictionary<string, string>
        {
            { "version", PaymentSettings.ApiVersion.ToString() },
            { "public_key", publicKey },
            { "action", "refund" },
            { "order_id", orderId.ToString() },
            { "amount", payment.Data.Sum.ToString("F2", CultureInfo.InvariantCulture) }
        };

        var json = JsonConvert.SerializeObject(data);
        var base64Data = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        var signature = GenerateSignature(base64Data);

        using var httpClient = new HttpClient();
        var formData = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "data", base64Data },
            { "signature", signature }
        });

        var response = await httpClient.PostAsync(PaymentSettings.LiqpayApiRequestUrl, formData);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (responseBody.StartsWith("<"))
        {
            throw new Exception($"Unexpected response from LiqPay: {responseBody}");
        }

        var responseData = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseBody);
    
        if (responseData.TryGetValue("status", out var status) && status == "reversed")
        {
            payment.Data.Status = PaymentSettings.IsCanceled;
            var result = await paymentServiceRepository.UpdatePayment(payment.Data);
            return result.Success;
        }

        Console.WriteLine($"Refund failed. LiqPay response: {responseBody}");
        return false;
    }

    public async Task<bool> AddCardPaymentsAsync(Guid orderId, string userName, double sum, int count)
    {
        var payment = new Payments
        {
            OrderId = orderId,
            UserName = userName,
            Sum = sum,
            Count = count,
            Status = PaymentSettings.IsCash,
            CreatedAt = DateTime.UtcNow
        };
        var result = await paymentServiceRepository.AddPayment(payment);
        return result.Success;
    }
}