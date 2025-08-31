using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsiteBuilderAPI.DTOs.Azul;
using WebsiteBuilderAPI.DTOs.Payment;
using WebsiteBuilderAPI.DTOs.PaymentProvider;
using WebsiteBuilderAPI.Services;

namespace WebsiteBuilderAPI.Controllers
{
    //[Authorize] // TODO: Re-enable after fixing auth
    [ApiController]
    [Route("api/payments")]
    public class PaymentController : ControllerBase
    {
        private readonly IAzulPaymentService _azulService;
        private readonly IPaymentProviderService _providerService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IAzulPaymentService azulService,
            IPaymentProviderService providerService,
            ILogger<PaymentController> logger)
        {
            _azulService = azulService;
            _providerService = providerService;
            _logger = logger;
        }

        [HttpPost("process")]
        public async Task<ActionResult<PaymentResponseDto>> ProcessPayment([FromBody] ProcessPaymentDto request)
        {
            try
            {
                var companyId = GetCompanyId(); // TODO: Obtener del contexto de usuario

                // Determinar proveedor activo
                var activeProvider = await _providerService.GetAllAsync();
                var provider = activeProvider.FirstOrDefault(p => p.IsActive);
                
                if (provider == null)
                    return BadRequest(new { error = "No active payment provider configured" });

                // Procesar según proveedor
                switch (provider.Provider.ToLower())
                {
                    case "azul":
                        var azulRequest = MapToAzulRequest(request);
                        var azulResponse = await _azulService.ProcessPaymentAsync(azulRequest, companyId);
                        return Ok(MapFromAzulResponse(azulResponse));
                    
                    case "stripe":
                        // TODO: Implementar Stripe
                        return BadRequest(new { error = "Stripe integration not yet implemented" });
                    
                    case "paypal":
                        // TODO: Implementar PayPal
                        return BadRequest(new { error = "PayPal integration not yet implemented" });
                    
                    default:
                        return BadRequest(new { error = "Unsupported payment provider" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment");
                return StatusCode(500, new { error = "Payment processing failed", message = ex.Message });
            }
        }

        [HttpGet("providers")]
        public async Task<ActionResult<IEnumerable<PaymentProviderResponseDto>>> GetProviders()
        {
            try
            {
                var providers = await _providerService.GetAllAsync();
                return Ok(providers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment providers");
                return StatusCode(500, new { error = "Failed to retrieve providers" });
            }
        }

        [HttpPost("providers/configure")]
        public async Task<ActionResult<object>> ConfigureProvider([FromForm] ConfigureProviderDto request)
        {
            try
            {
                var companyId = GetCompanyId();

                // Mapear a los DTOs existentes del servicio
                var createDto = new DTOs.PaymentProvider.CreatePaymentProviderRequestDto
                {
                    Name = request.Name,
                    Provider = request.Provider,
                    IsActive = request.IsActive,
                    IsTestMode = request.IsTestMode,
                    TransactionFee = request.TransactionFee,
                    
                    // Credenciales generales
                    ApiKey = request.ApiKey,
                    SecretKey = request.SecretKey,
                    ClientId = request.ClientId,
                    ClientSecret = request.ClientSecret,
                    WebhookSecret = request.WebhookSecret,
                    
                    // Específicos de Azul
                    StoreId = request.StoreId,
                    Auth1 = request.Auth1,
                    Auth2 = request.Auth2
                };

                var result = await _providerService.CreateAsync(createDto);

                // Manejar archivos de certificados si es Azul
                if (request.Provider.ToLower() == "azul" && result != null)
                {
                    if (request.CertificateFile != null)
                    {
                        await _providerService.UploadCertificateAsync(result.Id, request.CertificateFile);
                    }
                    
                    if (request.PrivateKeyFile != null)
                    {
                        await _providerService.UploadPrivateKeyAsync(result.Id, request.PrivateKeyFile);
                    }
                }

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error configuring payment provider");
                return StatusCode(500, new { error = "Failed to configure provider" });
            }
        }

        [HttpPut("providers/{providerId}/activate")]
        public async Task<ActionResult> ActivateProvider(int providerId)
        {
            try
            {
                var result = await _providerService.ToggleActiveAsync(providerId);
                if (result == null)
                    return NotFound(new { error = "Provider not found" });

                return Ok(new { message = "Provider activation toggled successfully", provider = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating payment provider");
                return StatusCode(500, new { error = "Failed to activate provider" });
            }
        }

        [HttpDelete("providers/{providerId}")]
        public async Task<ActionResult> DeleteProvider(int providerId)
        {
            try
            {
                var success = await _providerService.DeleteAsync(providerId);
                if (!success)
                    return NotFound(new { error = "Provider not found" });

                return Ok(new { message = "Provider deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting payment provider");
                return StatusCode(500, new { error = "Failed to delete provider" });
            }
        }

        [HttpGet("providers/available")]
        public async Task<ActionResult<IEnumerable<object>>> GetAvailableProviders()
        {
            try
            {
                var providers = await _providerService.GetAvailableProvidersAsync();
                return Ok(providers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available providers");
                return StatusCode(500, new { error = "Failed to retrieve available providers" });
            }
        }

        [HttpPost("validate-credentials")]
        public async Task<ActionResult<bool>> ValidateCredentials([FromBody] ValidateCredentialsDto request)
        {
            try
            {
                var companyId = GetCompanyId();
                
                switch (request.Provider.ToLower())
                {
                    case "azul":
                        var isValid = await _azulService.ValidateCredentialsAsync(companyId);
                        return Ok(new { isValid });
                    
                    default:
                        return BadRequest(new { error = "Provider validation not implemented" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating credentials");
                return StatusCode(500, new { error = "Failed to validate credentials" });
            }
        }

        private AzulPaymentRequestDto MapToAzulRequest(ProcessPaymentDto request)
        {
            // Formatear fecha de expiración MM/YY a YYYYMM
            var expParts = request.CardExpiry.Split('/');
            if (expParts.Length != 2)
                throw new ArgumentException("Invalid card expiry format. Expected MM/YY");

            var expMonth = expParts[0].PadLeft(2, '0');
            var expYear = expParts[1].Length == 2 ? $"20{expParts[1]}" : expParts[1];
            
            return new AzulPaymentRequestDto
            {
                CardNumber = request.CardNumber.Replace(" ", ""),
                Expiration = $"{expYear}{expMonth}",
                CVC = request.CardCVC,
                Amount = ((int)(request.Amount * 100)).ToString(), // Convertir a centavos sin decimales
                Itbis = ((int)(request.Amount * 0.18m * 100)).ToString(), // 18% ITBIS
                OrderNumber = request.OrderId,
                RRN = GenerateRRN(),
                CustomerServicePhone = request.CustomerPhone ?? "",
                ECommerceUrl = request.ReturnUrl ?? "",
                CustomOrderId = request.OrderId
            };
        }

        private PaymentResponseDto MapFromAzulResponse(AzulPaymentResponseDto response)
        {
            return new PaymentResponseDto
            {
                Success = response.ResponseCode == "00",
                TransactionId = response.RRN,
                AuthorizationCode = response.AuthorizationCode,
                Message = response.ResponseMessage,
                ErrorCode = response.ResponseCode != "00" ? response.ResponseCode : null,
                CardLastFour = response.CardNumber?.Length >= 4 
                    ? response.CardNumber.Substring(response.CardNumber.Length - 4) 
                    : null,
                ProcessedAt = response.DateTime,
                AdditionalData = new Dictionary<string, string>
                {
                    { "reasonCode", response.ReasonCode },
                    { "reasonMessage", response.ReasonMessage },
                    { "isoCode", response.IsoCode },
                    { "ticket", response.Ticket }
                }
            };
        }

        private string GenerateRRN()
        {
            // Generar referencia única de 12 dígitos
            return DateTime.Now.ToString("yyMMddHHmmss");
        }

        private int GetCompanyId()
        {
            // TODO: Obtener del contexto de autenticación/tenant
            // Por ahora retornamos 1 como placeholder
            return 1;
        }
    }

    public class ValidateCredentialsDto
    {
        public string Provider { get; set; } = string.Empty;
    }
}