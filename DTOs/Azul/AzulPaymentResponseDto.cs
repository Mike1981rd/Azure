namespace WebsiteBuilderAPI.DTOs.Azul
{
    public class AzulPaymentResponseDto
    {
        public string ResponseCode { get; set; } = string.Empty;
        public string ResponseMessage { get; set; } = string.Empty;
        public string ReasonCode { get; set; } = string.Empty;
        public string ReasonMessage { get; set; } = string.Empty;
        public string IsoCode { get; set; } = string.Empty;
        public string AuthorizationCode { get; set; } = string.Empty;
        public string RRN { get; set; } = string.Empty;
        public string CardNumber { get; set; } = string.Empty;
        public string DataVaultToken { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public string Ticket { get; set; } = string.Empty;
    }
}