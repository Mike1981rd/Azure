using Microsoft.EntityFrameworkCore;
using WebsiteBuilderAPI.Data;
using WebsiteBuilderAPI.DTOs.PaymentProvider;
using WebsiteBuilderAPI.Models;
using WebsiteBuilderAPI.Services.Encryption;

namespace WebsiteBuilderAPI.Services
{
    public class PaymentProviderService : IPaymentProviderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEncryptionService _encryptionService;
        private readonly ILogger<PaymentProviderService> _logger;
        private readonly IWebHostEnvironment _environment;

        public PaymentProviderService(
            ApplicationDbContext context,
            IEncryptionService encryptionService,
            ILogger<PaymentProviderService> logger,
            IWebHostEnvironment environment)
        {
            _context = context;
            _encryptionService = encryptionService;
            _logger = logger;
            _environment = environment;
        }

        public async Task<IEnumerable<PaymentProviderResponseDto>> GetAllAsync()
        {
            // Obtener la única empresa
            var company = await _context.Companies.FirstOrDefaultAsync();
            if (company == null)
            {
                return Enumerable.Empty<PaymentProviderResponseDto>();
            }

            // Verificar si existen proveedores, si no, inicializar los predeterminados
            var hasProviders = await _context.PaymentProviders
                .AnyAsync(p => p.CompanyId == company.Id);
                
            if (!hasProviders)
            {
                await InitializeDefaultProvidersAsync(company.Id);
            }

            var providers = await _context.PaymentProviders
                .Where(p => p.CompanyId == company.Id)
                .OrderBy(p => p.Name)
                .ToListAsync();

            return providers.Select(MapToResponseDto);
        }

        public async Task<PaymentProviderResponseDto?> GetByIdAsync(int id)
        {
            // Obtener la única empresa
            var company = await _context.Companies.FirstOrDefaultAsync();
            if (company == null)
            {
                return null;
            }

            var provider = await _context.PaymentProviders
                .FirstOrDefaultAsync(p => p.Id == id && p.CompanyId == company.Id);

            return provider != null ? MapToResponseDto(provider) : null;
        }

        public async Task<PaymentProviderResponseDto> CreateAsync(CreatePaymentProviderRequestDto request)
        {
            // Obtener la única empresa
            var company = await _context.Companies.FirstOrDefaultAsync();
            if (company == null)
            {
                throw new InvalidOperationException("No company configuration found");
            }

            // Verificar que no existe otro provider del mismo tipo para este company
            var existingProvider = await _context.PaymentProviders
                .FirstOrDefaultAsync(p => p.CompanyId == company.Id && p.Provider == request.Provider);

            if (existingProvider != null)
            {
                throw new ArgumentException($"A {request.Provider} provider already exists for this company");
            }

            var provider = new PaymentProvider
            {
                CompanyId = company.Id,
                Name = request.Name,
                Provider = request.Provider,
                Logo = request.Logo,
                IsActive = request.IsActive,
                IsManual = request.IsManual,
                TransactionFee = request.TransactionFee,
                IsTestMode = request.IsTestMode,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Encriptar credenciales si se proporcionan
            if (!string.IsNullOrEmpty(request.ApiKey))
                provider.ApiKey = _encryptionService.Encrypt(request.ApiKey);
            
            if (!string.IsNullOrEmpty(request.SecretKey))
                provider.SecretKey = _encryptionService.Encrypt(request.SecretKey);
            
            if (!string.IsNullOrEmpty(request.ClientId))
                provider.ClientId = _encryptionService.Encrypt(request.ClientId);
            
            if (!string.IsNullOrEmpty(request.ClientSecret))
                provider.ClientSecret = _encryptionService.Encrypt(request.ClientSecret);
            
            if (!string.IsNullOrEmpty(request.WebhookSecret))
                provider.WebhookSecret = _encryptionService.Encrypt(request.WebhookSecret);

            // Credenciales específicas de Azul
            if (!string.IsNullOrEmpty(request.StoreId))
                provider.StoreId = request.StoreId;
            
            if (!string.IsNullOrEmpty(request.Auth1))
                provider.Auth1 = _encryptionService.Encrypt(request.Auth1);
            
            if (!string.IsNullOrEmpty(request.Auth2))
                provider.Auth2 = _encryptionService.Encrypt(request.Auth2);

            _context.PaymentProviders.Add(provider);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Payment provider {Provider} created for company {CompanyId}", request.Provider, company.Id);
            
            return MapToResponseDto(provider);
        }

        public async Task<PaymentProviderResponseDto?> UpdateAsync(int id, UpdatePaymentProviderRequestDto request)
        {
            // Obtener la única empresa
            var company = await _context.Companies.FirstOrDefaultAsync();
            if (company == null)
            {
                return null;
            }

            var provider = await _context.PaymentProviders
                .FirstOrDefaultAsync(p => p.Id == id && p.CompanyId == company.Id);

            if (provider == null)
            {
                return null;
            }

            // Actualizar campos básicos
            provider.Name = request.Name ?? provider.Name;
            provider.Logo = request.Logo ?? provider.Logo;
            provider.IsActive = request.IsActive ?? provider.IsActive;
            provider.IsManual = request.IsManual ?? provider.IsManual;
            provider.TransactionFee = request.TransactionFee ?? provider.TransactionFee;
            provider.IsTestMode = request.IsTestMode ?? provider.IsTestMode;

            // Actualizar credenciales si se proporcionan
            if (!string.IsNullOrEmpty(request.ApiKey))
                provider.ApiKey = _encryptionService.Encrypt(request.ApiKey);
            
            if (!string.IsNullOrEmpty(request.SecretKey))
                provider.SecretKey = _encryptionService.Encrypt(request.SecretKey);
            
            if (!string.IsNullOrEmpty(request.ClientId))
                provider.ClientId = _encryptionService.Encrypt(request.ClientId);
            
            if (!string.IsNullOrEmpty(request.ClientSecret))
                provider.ClientSecret = _encryptionService.Encrypt(request.ClientSecret);
            
            if (!string.IsNullOrEmpty(request.WebhookSecret))
                provider.WebhookSecret = _encryptionService.Encrypt(request.WebhookSecret);

            // Credenciales específicas de Azul
            // StoreId puede ser actualizado a vacío
            if (request.StoreId != null)
                provider.StoreId = request.StoreId;
            
            // Auth1 y Auth2 solo se actualizan si tienen valor (no null y no vacío)
            if (!string.IsNullOrWhiteSpace(request.Auth1))
                provider.Auth1 = _encryptionService.Encrypt(request.Auth1);
            
            if (!string.IsNullOrWhiteSpace(request.Auth2))
                provider.Auth2 = _encryptionService.Encrypt(request.Auth2);

            provider.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Payment provider {Id} updated for company {CompanyId}", id, company.Id);
                
                return MapToResponseDto(provider);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment provider {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            // Obtener la única empresa
            var company = await _context.Companies.FirstOrDefaultAsync();
            if (company == null)
            {
                return false;
            }

            var provider = await _context.PaymentProviders
                .FirstOrDefaultAsync(p => p.Id == id && p.CompanyId == company.Id);

            if (provider == null)
            {
                return false;
            }

            // Eliminar archivos de certificados si existen
            await DeleteCertificateFilesAsync(provider);

            _context.PaymentProviders.Remove(provider);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Payment provider {Id} deleted for company {CompanyId}", id, company.Id);
            return true;
        }

        public async Task<PaymentProviderResponseDto?> ToggleActiveAsync(int id)
        {
            // Obtener la única empresa
            var company = await _context.Companies.FirstOrDefaultAsync();
            if (company == null)
            {
                return null;
            }

            var provider = await _context.PaymentProviders
                .FirstOrDefaultAsync(p => p.Id == id && p.CompanyId == company.Id);

            if (provider == null)
            {
                return null;
            }

            provider.IsActive = !provider.IsActive;
            provider.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Payment provider {Id} toggled to {Status} for company {CompanyId}", 
                id, provider.IsActive ? "Active" : "Inactive", company.Id);
            
            return MapToResponseDto(provider);
        }

        public async Task<bool> UploadCertificateAsync(int id, IFormFile file)
        {
            // Obtener la única empresa
            var company = await _context.Companies.FirstOrDefaultAsync();
            if (company == null)
            {
                return false;
            }

            var provider = await _context.PaymentProviders
                .FirstOrDefaultAsync(p => p.Id == id && p.CompanyId == company.Id);

            if (provider == null)
            {
                return false;
            }

            var filePath = await SaveSecureFileAsync(file, company.Id, "certificates", ".pem");
            
            // Eliminar certificado anterior si existe
            if (!string.IsNullOrEmpty(provider.CertificatePath) && File.Exists(provider.CertificatePath))
            {
                File.Delete(provider.CertificatePath);
            }

            provider.CertificatePath = filePath;
            provider.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UploadPrivateKeyAsync(int id, IFormFile file)
        {
            // Obtener la única empresa
            var company = await _context.Companies.FirstOrDefaultAsync();
            if (company == null)
            {
                return false;
            }

            var provider = await _context.PaymentProviders
                .FirstOrDefaultAsync(p => p.Id == id && p.CompanyId == company.Id);

            if (provider == null)
            {
                return false;
            }

            var filePath = await SaveSecureFileAsync(file, company.Id, "keys", ".key");
            
            // Eliminar llave anterior si existe
            if (!string.IsNullOrEmpty(provider.PrivateKeyPath) && File.Exists(provider.PrivateKeyPath))
            {
                File.Delete(provider.PrivateKeyPath);
            }

            provider.PrivateKeyPath = filePath;
            provider.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<AvailableProviderDto>> GetAvailableProvidersAsync()
        {
            // Obtener la única empresa
            var company = await _context.Companies.FirstOrDefaultAsync();
            if (company == null)
            {
                return Enumerable.Empty<AvailableProviderDto>();
            }

            // Providers configurados para este company
            var configuredProviders = await _context.PaymentProviders
                .Where(p => p.CompanyId == company.Id)
                .Select(p => p.Provider)
                .ToListAsync();

            // Todos los providers disponibles
            var allProviders = GetDefaultProviders();

            // Filtrar los que ya están configurados
            return allProviders.Where(p => !configuredProviders.Contains(p.Provider));
        }

        public async Task InitializeDefaultProvidersAsync(int companyId)
        {
            var existingProviders = await _context.PaymentProviders
                .Where(p => p.CompanyId == companyId)
                .Select(p => p.Provider)
                .ToListAsync();

            var defaultProviders = GetDefaultProviders()
                .Where(p => !existingProviders.Contains(p.Provider))
                .Select(p => new PaymentProvider
                {
                    CompanyId = companyId,
                    Name = p.Name,
                    Provider = p.Provider,
                    Logo = p.Logo,
                    IsActive = false,
                    IsManual = p.IsManual,
                    TransactionFee = p.TransactionFee,
                    IsTestMode = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });

            if (defaultProviders.Any())
            {
                _context.PaymentProviders.AddRange(defaultProviders);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Initialized {Count} default payment providers for company {CompanyId}", 
                    defaultProviders.Count(), companyId);
            }
        }

        private PaymentProviderResponseDto MapToResponseDto(PaymentProvider provider)
        {
            return new PaymentProviderResponseDto
            {
                Id = provider.Id,
                Name = provider.Name,
                Provider = provider.Provider,
                Logo = provider.Logo,
                IsActive = provider.IsActive,
                IsManual = provider.IsManual,
                TransactionFee = provider.TransactionFee,
                IsTestMode = provider.IsTestMode,
                StoreId = provider.StoreId, // No encriptado
                HasCertificate = !string.IsNullOrEmpty(provider.CertificatePath),
                HasPrivateKey = !string.IsNullOrEmpty(provider.PrivateKeyPath),
                CreatedAt = provider.CreatedAt,
                UpdatedAt = provider.UpdatedAt
            };
        }

        private async Task<string> SaveSecureFileAsync(IFormFile file, int companyId, string folder, string expectedExtension)
        {
            // Directorio seguro fuera de wwwroot
            var securePath = Path.Combine(_environment.ContentRootPath, "secure", "payments", companyId.ToString(), folder);
            Directory.CreateDirectory(securePath);

            var fileName = $"{Guid.NewGuid()}{expectedExtension}";
            var filePath = Path.Combine(securePath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return filePath;
        }

        private async Task DeleteCertificateFilesAsync(PaymentProvider provider)
        {
            if (!string.IsNullOrEmpty(provider.CertificatePath) && File.Exists(provider.CertificatePath))
            {
                File.Delete(provider.CertificatePath);
            }

            if (!string.IsNullOrEmpty(provider.PrivateKeyPath) && File.Exists(provider.PrivateKeyPath))
            {
                File.Delete(provider.PrivateKeyPath);
            }
        }

        private IEnumerable<AvailableProviderDto> GetDefaultProviders()
        {
            return new[]
            {
                new AvailableProviderDto
                {
                    Provider = "azul",
                    Name = "Azul Dominicana",
                    Logo = "/images/providers/azul.png",
                    TransactionFee = 3.95m,
                    IsManual = false,
                    Description = "Procesador de pagos líder en República Dominicana"
                },
                new AvailableProviderDto
                {
                    Provider = "cardnet",
                    Name = "Cardnet",
                    Logo = "/images/providers/cardnet.png",
                    TransactionFee = 3.25m,
                    IsManual = false,
                    Description = "Procesador de pagos con tarjetas de crédito"
                },
                new AvailableProviderDto
                {
                    Provider = "portal",
                    Name = "Portal Pagos",
                    Logo = "/images/providers/portal.png",
                    TransactionFee = 2.75m,
                    IsManual = false,
                    Description = "Gateway de pagos moderno"
                },
                new AvailableProviderDto
                {
                    Provider = "paypal",
                    Name = "PayPal",
                    Logo = "/images/providers/paypal.png",
                    TransactionFee = 3.49m,
                    IsManual = false,
                    Description = "Procesador de pagos internacional"
                },
                new AvailableProviderDto
                {
                    Provider = "stripe",
                    Name = "Stripe",
                    Logo = "/images/providers/stripe.png",
                    TransactionFee = 2.90m,
                    IsManual = false,
                    Description = "Plataforma de pagos moderna y segura"
                }
            };
        }
    }
}