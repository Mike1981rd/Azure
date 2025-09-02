namespace WebsiteBuilderAPI.DTOs.Azul
{
    public class AzulPaymentRequestDto
    {
        public string Channel { get; set; } = "EC"; // E-commerce
        public string Store { get; set; } = string.Empty;
        public string CardNumber { get; set; } = string.Empty;
        public string Expiration { get; set; } = string.Empty; // YYYYMM
        public string CVC { get; set; } = string.Empty;
        public string PosInputMode { get; set; } = "E-Commerce";
        public string TrxType { get; set; } = "Sale";
        public string Amount { get; set; } = string.Empty; // Sin decimales
        public string Itbis { get; set; } = string.Empty; // Sin decimales
        public string CurrencyPosCode { get; set; } = "$";
        public string Payments { get; set; } = "1";
        public string Plan { get; set; } = "0";
        public string AcquirerRefData { get; set; } = "1";
        public string RRN { get; set; } = string.Empty; // Referencia única
        public string CustomerServicePhone { get; set; } = string.Empty;
        public string OrderNumber { get; set; } = string.Empty;
        public string ECommerceUrl { get; set; } = string.Empty;
        public string CustomOrderId { get; set; } = string.Empty;
        public string DataVaultToken { get; set; } = string.Empty;
        public string SaveToDataVault { get; set; } = "0";
        public string OrderReferenceId { get; set; } = string.Empty;
    }
}