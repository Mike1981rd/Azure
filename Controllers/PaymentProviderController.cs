using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsiteBuilderAPI.Attributes;
using WebsiteBuilderAPI.DTOs.PaymentProvider;
using WebsiteBuilderAPI.Services;

namespace WebsiteBuilderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] // TODO: Re-enable after fixing auth
    public class PaymentProviderController : ControllerBase
    {
        private readonly IPaymentProviderService _paymentProviderService;
        private readonly ILogger<PaymentProviderController> _logger;

        public PaymentProviderController(
            IPaymentProviderService paymentProviderService, 
            ILogger<PaymentProviderController> logger)
        {
            _paymentProviderService = paymentProviderService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los proveedores de pago del hotel actual
        /// </summary>
        [HttpGet]
        //[RequirePermission("payment_methods", "read")] // TODO: Re-enable after fixing permissions
        public async Task<ActionResult<IEnumerable<PaymentProviderResponseDto>>> GetAll()
        {
            try
            {
                var providers = await _paymentProviderService.GetAllAsync();
                return Ok(providers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment providers");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Obtiene un proveedor de pago específico
        /// </summary>
        [HttpGet("{id}")]
        //[RequirePermission("payment_methods", "read")] // TODO: Re-enable after fixing permissions
        public async Task<ActionResult<PaymentProviderResponseDto>> GetById(int id)
        {
            try
            {
                var provider = await _paymentProviderService.GetByIdAsync(id);
                if (provider == null)
                {
                    return NotFound(new { message = "Payment provider not found" });
                }

                return Ok(provider);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment provider {Id}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Configura o actualiza un proveedor de pago con archivos
        /// </summary>
        [HttpPost("configure")]
        [Consumes("multipart/form-data")]
        //[RequirePermission("payment_methods", "create")] // TODO: Re-enable after fixing permissions
        public async Task<ActionResult<PaymentProviderResponseDto>> ConfigureWithFiles([FromForm] ConfigureProviderFormDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("ConfigureWithFiles called with provider: {Provider}", request.Provider);
                _logger.LogInformation("Files received - Certificate: {HasCert}, PrivateKey: {HasKey}", 
                    request.CertificateFile != null, request.PrivateKeyFile != null);
                
                // Check if provider already exists
                var existingProviders = await _paymentProviderService.GetAllAsync();
                var existingProvider = existingProviders.FirstOrDefault(p => p.Provider.Equals(request.Provider, StringComparison.OrdinalIgnoreCase));
                
                PaymentProviderResponseDto providerDto;
                
                if (existingProvider != null)
                {
                    // Update existing provider - solo enviar campos no vacíos
                    var updateRequest = new UpdatePaymentProviderRequestDto
                    {
                        Name = request.Name,
                        IsActive = existingProvider.IsActive, // Keep current active state
                        IsTestMode = request.IsTestMode,
                        TransactionFee = request.TransactionFee,
                        StoreId = request.StoreId, // StoreId siempre se puede actualizar incluso si es vacío
                        // Solo actualizar credenciales si tienen valor
                        Auth1 = !string.IsNullOrWhiteSpace(request.Auth1) ? request.Auth1 : null,
                        Auth2 = !string.IsNullOrWhiteSpace(request.Auth2) ? request.Auth2 : null,
                        ApiKey = !string.IsNullOrWhiteSpace(request.ApiKey) ? request.ApiKey : null,
                        SecretKey = !string.IsNullOrWhiteSpace(request.SecretKey) ? request.SecretKey : null,
                        ClientId = !string.IsNullOrWhiteSpace(request.ClientId) ? request.ClientId : null,
                        ClientSecret = !string.IsNullOrWhiteSpace(request.ClientSecret) ? request.ClientSecret : null,
                        WebhookSecret = !string.IsNullOrWhiteSpace(request.WebhookSecret) ? request.WebhookSecret : null
                    };
                    
                    var updateResult = await _paymentProviderService.UpdateAsync(existingProvider.Id, updateRequest);
                    if (updateResult == null)
                    {
                        return NotFound(new { message = "Provider not found" });
                    }
                    providerDto = updateResult;
                }
                else
                {
                    // Create new provider
                    var createRequest = new CreatePaymentProviderRequestDto
                    {
                        Provider = request.Provider,
                        Name = request.Name,
                        IsTestMode = request.IsTestMode,
                        TransactionFee = request.TransactionFee,
                        StoreId = request.StoreId,
                        Auth1 = request.Auth1,
                        Auth2 = request.Auth2,
                        ApiKey = request.ApiKey,
                        SecretKey = request.SecretKey,
                        ClientId = request.ClientId,
                        ClientSecret = request.ClientSecret,
                        WebhookSecret = request.WebhookSecret
                    };
                    
                    providerDto = await _paymentProviderService.CreateAsync(createRequest);
                }

                // Upload files if provided
                if (request.CertificateFile != null && providerDto != null)
                {
                    await _paymentProviderService.UploadCertificateAsync(providerDto.Id, request.CertificateFile);
                }
                
                if (request.PrivateKeyFile != null && providerDto != null)
                {
                    await _paymentProviderService.UploadPrivateKeyAsync(providerDto.Id, request.PrivateKeyFile);
                }

                // Return updated provider info
                if (providerDto != null)
                {
                    var updatedProvider = await _paymentProviderService.GetByIdAsync(providerDto.Id);
                    return Ok(updatedProvider);
                }
                
                return BadRequest(new { message = "Failed to create payment provider" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error configuring payment provider");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Crea un nuevo proveedor de pago
        /// </summary>
        [HttpPost]
        //[RequirePermission("payment_methods", "create")] // TODO: Re-enable after fixing permissions
        public async Task<ActionResult<PaymentProviderResponseDto>> Create([FromBody] CreatePaymentProviderRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var provider = await _paymentProviderService.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = provider.Id }, provider);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment provider");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Actualiza un proveedor de pago existente
        /// </summary>
        [HttpPut("{id}")]
        //[RequirePermission("payment_methods", "update")] // TODO: Re-enable after fixing permissions
        public async Task<ActionResult<PaymentProviderResponseDto>> Update(int id, [FromBody] UpdatePaymentProviderRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var provider = await _paymentProviderService.UpdateAsync(id, request);
                if (provider == null)
                {
                    return NotFound(new { message = "Payment provider not found" });
                }

                return Ok(provider);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment provider {Id}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Elimina un proveedor de pago
        /// </summary>
        [HttpDelete("{id}")]
        //[RequirePermission("payment_methods", "delete")] // TODO: Re-enable after fixing permissions
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var success = await _paymentProviderService.DeleteAsync(id);
                if (!success)
                {
                    return NotFound(new { message = "Payment provider not found" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting payment provider {Id}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Activa/desactiva un proveedor de pago
        /// </summary>
        [HttpPatch("{id}/toggle")]
        //[RequirePermission("payment_methods", "update")] // TODO: Re-enable after fixing permissions
        public async Task<ActionResult<PaymentProviderResponseDto>> ToggleActive(int id)
        {
            try
            {
                var provider = await _paymentProviderService.ToggleActiveAsync(id);
                if (provider == null)
                {
                    return NotFound(new { message = "Payment provider not found" });
                }

                return Ok(provider);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling payment provider {Id}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Sube certificado SSL para Azul Dominicana (.pem)
        /// </summary>
        [HttpPost("{id}/certificate")]
        //[RequirePermission("payment_methods", "update")] // TODO: Re-enable after fixing permissions
        public async Task<ActionResult> UploadCertificate(int id, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "No file provided" });
                }

                // Validar extensión
                if (!file.FileName.EndsWith(".pem", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = "Invalid file type. Only .pem files are allowed." });
                }

                var success = await _paymentProviderService.UploadCertificateAsync(id, file);
                if (!success)
                {
                    return NotFound(new { message = "Payment provider not found" });
                }

                return Ok(new { message = "Certificate uploaded successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading certificate for provider {Id}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Sube llave privada para Azul Dominicana (.key)
        /// </summary>
        [HttpPost("{id}/private-key")]
        //[RequirePermission("payment_methods", "update")] // TODO: Re-enable after fixing permissions
        public async Task<ActionResult> UploadPrivateKey(int id, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "No file provided" });
                }

                // Validar extensión
                if (!file.FileName.EndsWith(".key", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = "Invalid file type. Only .key files are allowed." });
                }

                var success = await _paymentProviderService.UploadPrivateKeyAsync(id, file);
                if (!success)
                {
                    return NotFound(new { message = "Payment provider not found" });
                }

                return Ok(new { message = "Private key uploaded successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading private key for provider {Id}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Obtiene los proveedores disponibles para configurar
        /// </summary>
        [HttpGet("available")]
        //[RequirePermission("payment_methods", "read")] // TODO: Re-enable after fixing permissions
        public async Task<ActionResult<IEnumerable<AvailableProviderDto>>> GetAvailableProviders()
        {
            try
            {
                var providers = await _paymentProviderService.GetAvailableProvidersAsync();
                return Ok(providers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available providers");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}