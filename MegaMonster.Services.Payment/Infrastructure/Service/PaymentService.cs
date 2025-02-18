using System.Security.Cryptography;
using System.Text;
using MegaMonster.Services.Payment.Core.Models;
using MegaMonster.Services.Payment.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace MegaMonster.Services.Payment.Infrastructure.Service
{
    public class PaymentService(string publicKey, string privateKey, AppDbContext db)
    {
        private string CreatePayment(int orderId, string userName, double amount, int count, string action)
        {
            var data = new Dictionary<string, string>
            {
                {"version", PaymentSettings.ApiVersion.ToString()},
                {"public_key", publicKey},
                {"userName", userName },
                {"action", action.ToLower() },
                {"amount", amount.ToString()},
                {"currency", "UAH"},
                {"description", $"{count}"},
                {"order_id", orderId.ToString()},
                {"result_url", ""}
            };

            var json = JsonConvert.SerializeObject(data);
            var base64Data = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

            var signature = GenerateSignature(base64Data);
            Console.WriteLine($"Отправленные данные: {JsonConvert.SerializeObject(data)}");

            return $"{PaymentSettings.LiqpayApiCheckoutUrl}?data={base64Data}&signature={signature}";
        }

        private string GenerateSignature(string base64Data)
        {
            using var sha1 = SHA1.Create();
            var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(privateKey + base64Data + privateKey));
            return Convert.ToBase64String(hash);
        }

        public async Task<string> CreatePaymentAsync(int orderId, string userName, double amount, int count, string action)
        {
            if (count <= 0 || amount <= 0)
            {
                return "Count and sum must be > 0";
            }
            
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

            await db.Payments.AddAsync(payment);
            await db.SaveChangesAsync();

            return paymentUrl;
        }
        
        public async Task<bool> HandlePaymentResultAsync(Dictionary<string, string> requestDictionary)
        {
            Console.WriteLine($"Request data: {requestDictionary}");
            if (requestDictionary.TryGetValue("data", out var base64Data) &&
                requestDictionary.TryGetValue("signature", out var signature)) 
            {
                var decodedData = Encoding.UTF8.GetString(Convert.FromBase64String(base64Data));
                Console.WriteLine($"Decoded data: {decodedData}");
                var requestData = JsonConvert.DeserializeObject<Dictionary<string, string>>(decodedData);
                Console.WriteLine($"Request data: {requestData}");
                Console.WriteLine($"Decoded request data: {JsonConvert.SerializeObject(requestData)}");

                if (!requestData.TryGetValue("order_id", out var orderId))
                {
                    return false;
                }

                var payment = await GetPaymentAsync(orderId);
                if (payment == null)
                {
                    return false; 
                }
                
                if (signature != GenerateSignature(base64Data))
                {
                    return false; 
                }

                if (requestData.TryGetValue("status", out var status) && status == "success")
                {
                    payment.Status = PaymentSettings.IsSuccess;
                }
                else
                {
                    payment.Status = PaymentSettings.IsCanceled;
                }

                await SavePaymentAsync(payment);
                return true;
            }

            return false; 
        }
        
        public async Task<bool> CancelPaymentAsync(int orderId)
        {
            var payment = await GetPaymentAsync(orderId.ToString());
            if (payment == null || payment.Status != PaymentSettings.IsCreated)
            {
                return false;
            }

            payment.Status = PaymentSettings.IsCanceled;
            await SavePaymentAsync(payment);
            return true;
        }
        
        private async Task<Payments> GetPaymentAsync(string orderId)
        {
            return await db.Payments.SingleOrDefaultAsync(p => p.OrderId.ToString() == orderId) ?? throw new InvalidOperationException();
        }

        private async Task SavePaymentAsync(Payments payment)
        {
            db.Payments.Update(payment);
            await db.SaveChangesAsync();
        }
    }
}