namespace MegaMonster.Services.Payment.Infrastructure
{
    public static class PaymentSettings
    {
        public const int ApiVersion = 3;
        public const string LiqpayApiCheckoutUrl = "https://www.liqpay.ua/api/3/checkout";
        public const string IsCreated = "Created";
        public const string IsSuccess = "Success";
        public const string IsCanceled = "Canceled";
    }
}