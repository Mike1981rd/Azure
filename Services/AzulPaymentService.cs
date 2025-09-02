using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using WebsiteBuilderAPI.DTOs.Azul;
using WebsiteBuilderAPI.Models;
using WebsiteBuilderAPI.Repositories;

namespace WebsiteBuilderAPI.Services
{
    public interface IAzulPaymentService
    {
        Task<AzulPaymentResponseDto> ProcessPaymentAsync(AzulPaymentRequestDto request, int companyId);
        Task<bool> ValidateCredentialsAsync(int companyId);
        Task<string> GetPaymentStatusAsync(string transactionId, int companyId);
        bool VerifyWebhookSignature(string payload, string signature);
        Task<object> GetConfiguration(int companyId);
    }

    public class AzulPaymentService : IAzulPaymentService
    {
        private readonly HttpClient _httpClient;
        private readonly IPaymentProviderRepository _providerRepo;
        private readonly IEncryptionService _encryptionService;
        private readonly ILogger<AzulPaymentService> _logger;
        private readonly IConfiguration _configuration;

        public AzulPaymentService(
            HttpClient httpClient,
            IPaymentProviderRepository providerRepo,
            IEncryptionService encryptionService,
            ILogger<AzulPaymentService> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _providerRepo = providerRepo;
            _encryptionService = encryptionService;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<AzulPaymentResponseDto> ProcessPaymentAsync(AzulPaymentRequestDto request, int companyId)
        {
            try
            {
                // 1. Obtener configuración de Azul
                var provider = await _providerRepo.GetActiveProviderAsync(companyId, "azul");
                if (provider == null)
                {
                    _logger.LogError($"Azul payment provider not configured for company {companyId}");
                    throw new Exception("Azul payment provider not configured");
                }

                // 2. Validar que existan las credenciales necesarias
                if (string.IsNullOrEmpty(provider.StoreId) || 
                    string.IsNullOrEmpty(provider.Auth1) || 
                    string.IsNullOrEmpty(provider.Auth2))
                {
                    _logger.LogError("Azul credentials are incomplete");
                    throw new Exception("Azul credentials are not properly configured");
                }

                // 3. Configurar certificados SSL si están disponibles
                if (!string.IsNullOrEmpty(provider.CertificatePath) && 
                    !string.IsNullOrEmpty(provider.PrivateKeyPath))
                {
                    ConfigureSSLCertificates(provider);
                }

                // 4. Preparar request
                request.Store = provider.StoreId;
                
                // 5. Configurar headers de autenticación
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Auth1", _encryptionService.Decrypt(provider.Auth1));
                _httpClient.DefaultRequestHeaders.Add("Auth2", _encryptionService.Decrypt(provider.Auth2));
                _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                // 6. Determinar URL según modo
                var baseUrl = provider.IsTestMode 
                    ? _configuration["PaymentProviders:Azul:TestUrl"] ?? "https://pruebas.azul.com.do/PaymentPage/"
                    : _configuration["PaymentProviders:Azul:ProductionUrl"] ?? "https://pagos.azul.com.do/PaymentPage/";

                // 7. Serializar y enviar request
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                
                var json = JsonSerializer.Serialize(request, jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation($"Sending payment request to Azul for Company {companyId}, Order {request.OrderNumber}");
                
                var response = await _httpClient.PostAsync($"{baseUrl}api/payment", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Azul API error: {response.StatusCode} - {responseContent}");
                    throw new Exception($"Payment processing failed: {response.StatusCode}");
                }

                // 8. Deserializar respuesta
                var result = JsonSerializer.Deserialize<AzulPaymentResponseDto>(responseContent, jsonOptions);
                
                if (result == null)
                {
                    _logger.LogError("Invalid response from Azul - null result");
                    throw new Exception("Invalid response from Azul");
                }

                _logger.LogInformation($"Payment processed. Response Code: {result.ResponseCode}, Message: {result.ResponseMessage}");
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Azul payment");
                throw;
            }
        }

        public async Task<bool> ValidateCredentialsAsync(int companyId)
        {
            try
            {
                var provider = await _providerRepo.GetActiveProviderAsync(companyId, "azul");
                if (provider == null) return false;

                // Verificar que existan las credenciales necesarias
                return !string.IsNullOrEmpty(provider.StoreId) &&
                       !string.IsNullOrEmpty(provider.Auth1) &&
                       !string.IsNullOrEmpty(provider.Auth2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error validating Azul credentials for company {companyId}");
                return false;
            }
        }

        public async Task<string> GetPaymentStatusAsync(string transactionId, int companyId)
        {
            try
            {
                var provider = await _providerRepo.GetActiveProviderAsync(companyId, "azul");
                if (provider == null)
                    throw new Exception("Azul payment provider not configured");

                // TODO: Implementar consulta de estado cuando Azul proporcione el endpoint
                _logger.LogWarning("Payment status check not yet implemented for Azul");
                return "UNKNOWN";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking payment status for transaction {transactionId}");
                throw;
            }
        }

        private void ConfigureSSLCertificates(PaymentProvider provider)
        {
            try
            {
                if (!File.Exists(provider.CertificatePath) || !File.Exists(provider.PrivateKeyPath))
                {
                    _logger.LogWarning("SSL certificate files not found, proceeding without client certificates");
                    return;
                }

                // Cargar certificado .pem y llave privada .key
                var certificate = X509Certificate2.CreateFromPemFile(
                    provider.CertificatePath,
                    provider.PrivateKeyPath
                );

                // Nota: La configuración del certificado en HttpClient debe hacerse
                // a través de HttpClientFactory en Program.cs para mejor manejo
                _logger.LogInformation("SSL certificates loaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading SSL certificates");
                throw new Exception("Failed to load SSL certificates", ex);
            }
        }

        public bool VerifyWebhookSignature(string payload, string signature)
        {
            // TODO: Implementar verificación de firma del webhook de Azul
            // Por ahora retornamos true para permitir el procesamiento
            _logger.LogWarning("Webhook signature verification not yet implemented");
            return true;
        }

        public async Task<object> GetConfiguration(int companyId)
        {
            var provider = await _providerRepo.GetActiveProviderAsync(companyId, "azul");
            if (provider == null)
            {
                return new { configured = false };
            }

            return new 
            { 
                configured = true,
                providerId = provider.Id,
                providerType = provider.Provider
            };
        }
    }
}