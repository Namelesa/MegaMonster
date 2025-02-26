namespace MegaMonster.Services.Payment.Infrastructure
{
    public static class PaymentSettings
    {
        public const int ApiVersion = 3;
        public const string LiqpayApiCheckoutUrl = "https://www.liqpay.ua/api/3/checkout";
        public const string LiqpayApiRequestUrl = "https://www.liqpay.ua/api/request";
        public const string IsCreated = "created";
        public const string IsSuccess = "success";
        public const string IsCanceled = "canceled";
        public const string IsUnknown = "unknow";
    }
}