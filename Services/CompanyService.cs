using Microsoft.EntityFrameworkCore;
using Npgsql;
using WebsiteBuilderAPI.Data;
using WebsiteBuilderAPI.DTOs.Company;
using System.Text.Json;
using WebsiteBuilderAPI.DTOs.CheckoutSettings;
using WebsiteBuilderAPI.Models;

namespace WebsiteBuilderAPI.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CompanyService> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly WebsiteBuilderAPI.Services.Storage.IStorageService _storage;

        public CompanyService(
            ApplicationDbContext context,
            ILogger<CompanyService> logger,
            IWebHostEnvironment environment,
            WebsiteBuilderAPI.Services.Storage.IStorageService storage)
        {
            _context = context;
            _logger = logger;
            _environment = environment;
            _storage = storage;
        }

        public async Task<CompanyResponseDto?> GetCurrentCompanyAsync()
        {
            // Obtener la única empresa del sistema
            var currentCompany = await _context.Companies.FirstOrDefaultAsync();
            if (currentCompany == null)
            {
                // Si no existe, crear una por defecto
                currentCompany = new Company
                {
                    Name = "Mi Empresa",
                    IsActive = true,
                    PrimaryColor = "#22c55e",
                    SecondaryColor = "#64748b",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Companies.Add(currentCompany);
                await _context.SaveChangesAsync();
            }

            return MapToResponseDto(currentCompany);
        }

        public async Task<CompanyResponseDto?> GetCompanyByIdAsync(int companyId)
        {
            var company = await _context.Companies.FindAsync(companyId);
            if (company == null)
            {
                return null;
            }

            return MapToResponseDto(company);
        }

        public async Task<Company?> GetCompanyEntityByIdAsync(int companyId)
        {
            return await _context.Companies.FindAsync(companyId);
        }

        private CompanyResponseDto MapToResponseDto(Company company)
        {
            return new CompanyResponseDto
            {
                Id = company.Id,
                Name = company.Name,
                Domain = company.Domain,
                CustomDomain = company.CustomDomain,
                Subdomain = company.Subdomain,
                Logo = company.Logo,
                LogoSize = company.LogoSize,
                PrimaryColor = company.PrimaryColor,
                SecondaryColor = company.SecondaryColor,
                IsActive = company.IsActive,
                
                // Profile Section
                PhoneNumber = company.PhoneNumber,
                ContactEmail = company.ContactEmail,
                SenderEmail = company.SenderEmail,
                
                // Billing Information
                LegalBusinessName = company.LegalBusinessName,
                Country = company.Country,
                Region = company.Region,
                Address = company.Address,
                Apartment = company.Apartment,
                City = company.City,
                State = company.State,
                PostalCode = company.PostalCode,
                
                // Time Zone & Units
                TimeZone = company.TimeZone,
                MetricSystem = company.MetricSystem,
                WeightUnit = company.WeightUnit,
                
                // Store Currency
                Currency = company.Currency,
                
                // Order ID Format
                OrderIdPrefix = company.OrderIdPrefix,
                OrderIdSuffix = company.OrderIdSuffix,

                // Maps & Geolocation
                GeolocationProvider = company.GeolocationProvider,
                GeolocationToken = company.GeolocationToken,
                
                CreatedAt = company.CreatedAt,
                UpdatedAt = company.UpdatedAt
            };
        }

        public async Task<CompanyResponseDto?> UpdateCurrentCompanyAsync(UpdateCompanyRequestDto request)
        {
            // Obtener la única empresa del sistema
            var company = await _context.Companies.FirstOrDefaultAsync();

            if (company == null)
            {
                return null;
            }

            // Actualizar campos básicos - SOLO si tienen valor
            if (!string.IsNullOrEmpty(request.Name))
                company.Name = request.Name;
            
            if (request.Logo != null)
                company.Logo = request.Logo;
            
            if (!string.IsNullOrEmpty(request.PrimaryColor))
                company.PrimaryColor = request.PrimaryColor;
            
            if (!string.IsNullOrEmpty(request.SecondaryColor))
                company.SecondaryColor = request.SecondaryColor;

            // Profile Section - SOLO actualizar si se proporciona un valor (no null)
            // NO actualizar con string vacío, mantener el valor actual
            if (request.PhoneNumber != null && request.PhoneNumber != "")
                company.PhoneNumber = request.PhoneNumber;
            else if (request.PhoneNumber == "")
                company.PhoneNumber = null;
                
            if (request.ContactEmail != null && request.ContactEmail != "")
                company.ContactEmail = request.ContactEmail;
            else if (request.ContactEmail == "")
                company.ContactEmail = null;
                
            if (request.SenderEmail != null && request.SenderEmail != "")
                company.SenderEmail = request.SenderEmail;
            else if (request.SenderEmail == "")
                company.SenderEmail = null;

            // Billing Information - NO actualizar si es null
            if (request.LegalBusinessName != null && request.LegalBusinessName != "")
                company.LegalBusinessName = request.LegalBusinessName;
            else if (request.LegalBusinessName == "")
                company.LegalBusinessName = null;
                
            if (request.Country != null && request.Country != "")
                company.Country = request.Country;
            else if (request.Country == "")
                company.Country = null;
                
            if (request.Region != null && request.Region != "")
                company.Region = request.Region;
            else if (request.Region == "")
                company.Region = null;
                
            if (request.Address != null && request.Address != "")
                company.Address = request.Address;
            else if (request.Address == "")
                company.Address = null;
                
            if (request.Apartment != null && request.Apartment != "")
                company.Apartment = request.Apartment;
            else if (request.Apartment == "")
                company.Apartment = null;
                
            if (request.City != null && request.City != "")
                company.City = request.City;
            else if (request.City == "")
                company.City = null;
                
            if (request.State != null && request.State != "")
                company.State = request.State;
            else if (request.State == "")
                company.State = null;
                
            if (request.PostalCode != null && request.PostalCode != "")
                company.PostalCode = request.PostalCode;
            else if (request.PostalCode == "")
                company.PostalCode = null;

            // Time Zone & Units - NO actualizar si es null
            if (request.TimeZone != null && request.TimeZone != "")
                company.TimeZone = request.TimeZone;
            else if (request.TimeZone == "")
                company.TimeZone = null;
                
            if (request.MetricSystem != null && request.MetricSystem != "")
                company.MetricSystem = request.MetricSystem;
            else if (request.MetricSystem == "")
                company.MetricSystem = null;
                
            if (request.WeightUnit != null && request.WeightUnit != "")
                company.WeightUnit = request.WeightUnit;
            else if (request.WeightUnit == "")
                company.WeightUnit = null;

            // Store Currency - NO actualizar si es null
            if (request.Currency != null && request.Currency != "")
                company.Currency = request.Currency;
            else if (request.Currency == "")
                company.Currency = null;

            // Order ID Format - NO actualizar si es null
            if (request.OrderIdPrefix != null && request.OrderIdPrefix != "")
                company.OrderIdPrefix = request.OrderIdPrefix;
            else if (request.OrderIdPrefix == "")
                company.OrderIdPrefix = null;
                
            if (request.OrderIdSuffix != null && request.OrderIdSuffix != "")
                company.OrderIdSuffix = request.OrderIdSuffix;
            else if (request.OrderIdSuffix == "")
                company.OrderIdSuffix = null;

            // Maps & Geolocation
            if (request.GeolocationProvider != null && request.GeolocationProvider != "")
                company.GeolocationProvider = request.GeolocationProvider;
            else if (request.GeolocationProvider == "")
                company.GeolocationProvider = null;

            if (request.GeolocationToken != null && request.GeolocationToken != "")
                company.GeolocationToken = request.GeolocationToken;
            else if (request.GeolocationToken == "")
                company.GeolocationToken = null;

            company.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Company {CompanyId} updated successfully", company.Id);
                
                return await GetCurrentCompanyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating company {CompanyId}", company.Id);
                throw;
            }
        }

        public async Task<string> UploadLogoAsync(IFormFile file)
        {
            // Obtener la única empresa
            var company = await _context.Companies.FirstOrDefaultAsync();
            if (company == null)
            {
                throw new InvalidOperationException("No company configuration found");
            }

            // Crear directorio si no existe
            var uploadsPath = Path.Combine(_environment.WebRootPath ?? "wwwroot", "uploads", "company");
            Directory.CreateDirectory(uploadsPath);

            // Generar nombre único para el archivo
            var fileExtension = Path.GetExtension(file.FileName);
            var fileName = $"logo_{DateTime.UtcNow:yyyyMMddHHmmss}{fileExtension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            // Guardar archivo
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Actualizar empresa con la URL del logo
            var logoUrl = $"/uploads/company/{fileName}";
            if (company != null)
            {
                // Eliminar logo anterior si existe
                if (!string.IsNullOrEmpty(company.Logo))
                {
                    var oldLogoPath = Path.Combine(_environment.WebRootPath ?? "wwwroot", company.Logo.TrimStart('/'));
                    if (File.Exists(oldLogoPath))
                    {
                        File.Delete(oldLogoPath);
                    }
                }

                company.Logo = logoUrl;
                company.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("Logo uploaded for company: {LogoUrl}", logoUrl);
            return logoUrl;
        }

        public async Task<CompanyConfigDto?> GetCompanyConfigAsync()
        {
            try
            {
                var currentCompany = await _context.Companies.AsNoTracking().FirstOrDefaultAsync();
                if (currentCompany == null)
                {
                    // Return safe defaults if no company exists yet
                    return new CompanyConfigDto
                    {
                        Name = "WebsiteBuilder",
                        Logo = null,
                        LogoSize = 120,
                        PrimaryColor = "#22c55e",
                        SecondaryColor = "#64748b",
                        Currency = "USD",
                        TimeZone = null,
                        OrderIdPrefix = "#",
                        OrderIdSuffix = string.Empty
                    };
                }

                return new CompanyConfigDto
                {
                    Name = currentCompany.Name,
                    Logo = currentCompany.Logo,
                    LogoSize = currentCompany.LogoSize,
                    PrimaryColor = currentCompany.PrimaryColor ?? "#22c55e",
                    SecondaryColor = currentCompany.SecondaryColor ?? "#64748b",
                    Currency = currentCompany.Currency ?? "USD",
                    TimeZone = currentCompany.TimeZone,
                    OrderIdPrefix = currentCompany.OrderIdPrefix ?? "#",
                    OrderIdSuffix = currentCompany.OrderIdSuffix ?? string.Empty
                };
            }
            catch (PostgresException ex) when (ex.SqlState == "42703")
            {
                // Column not found (e.g., new columns not yet migrated). Return defaults so login page doesn't break.
                _logger.LogError(ex, "Company config fallback: missing DB columns. Returning defaults.");
                return new CompanyConfigDto
                {
                    Name = "WebsiteBuilder",
                    Logo = null,
                    LogoSize = 120,
                    PrimaryColor = "#22c55e",
                    SecondaryColor = "#64748b",
                    Currency = "USD",
                    TimeZone = null,
                    OrderIdPrefix = "#",
                    OrderIdSuffix = string.Empty
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting company config. Returning defaults to keep UI working.");
                return new CompanyConfigDto
                {
                    Name = "WebsiteBuilder",
                    Logo = null,
                    LogoSize = 120,
                    PrimaryColor = "#22c55e",
                    SecondaryColor = "#64748b",
                    Currency = "USD",
                    TimeZone = null,
                    OrderIdPrefix = "#",
                    OrderIdSuffix = string.Empty
                };
            }
        }

        public async Task UpdateLogoSizeAsync(int size)
        {
            var currentCompany = await _context.Companies.FirstOrDefaultAsync();
            if (currentCompany != null)
            {
                currentCompany.LogoSize = size;
                currentCompany.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateLogoAsync(string logoUrl)
        {
            var currentCompany = await _context.Companies.FirstOrDefaultAsync();
            if (currentCompany != null)
            {
                currentCompany.Logo = logoUrl;
                currentCompany.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Logo updated for company {CompanyId}: {LogoUrl}", currentCompany.Id, logoUrl);
            }
        }

        public async Task<string> UploadCheckoutLogoAsync(IFormFile file)
        {
            var currentCompany = await _context.Companies.FirstOrDefaultAsync();
            if (currentCompany == null)
            {
                throw new InvalidOperationException("No company found in the system");
            }

            // Ensure settings exist
            var settings = await _context.CheckoutSettings.FirstOrDefaultAsync(cs => cs.CompanyId == currentCompany.Id);
            if (settings == null)
            {
                settings = new CheckoutSettings
                {
                    CompanyId = currentCompany.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.CheckoutSettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            // Save using configured storage provider (local persistent disk or cloud)
            var fileExtension = Path.GetExtension(file.FileName);
            var fileName = $"checkout_logo_{DateTime.UtcNow:yyyyMMddHHmmss}{fileExtension}";
            await using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            ms.Position = 0;
            var logoUrl = await _storage.UploadAsync(ms, fileName, file.ContentType, "checkout");

            // Update settings with the new logo URL
            settings.CheckoutLogoUrl = logoUrl;
            settings.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Checkout logo uploaded: {LogoUrl}", logoUrl);
            return logoUrl;
        }

        // ===================== CURRENCY SETTINGS (MANUAL) =====================
        public async Task<CurrencySettingsDto> GetCurrencySettingsAsync(int companyId)
        {
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == companyId);
            if (company == null) throw new InvalidOperationException("Company not found");

            // Prefer new fields; fallback to legacy Company.Currency
            var baseCurrency = company.CurrencyBase ?? company.Currency ?? "USD";
            var enabled = !string.IsNullOrWhiteSpace(company.EnabledCurrenciesJson)
                ? JsonSerializer.Deserialize<List<string>>(company.EnabledCurrenciesJson!) ?? new List<string>()
                : new List<string> { baseCurrency };
            if (!enabled.Contains(baseCurrency)) enabled.Add(baseCurrency);

            _logger.LogInformation("Loading currency settings - Base: {Base}, EnabledJson: {Json}", baseCurrency, company.EnabledCurrenciesJson);
            _logger.LogInformation("ManualRatesJson from DB: {Rates}", company.ManualRatesJson);

            var manualRates = !string.IsNullOrWhiteSpace(company.ManualRatesJson)
                ? JsonSerializer.Deserialize<Dictionary<string, decimal>>(company.ManualRatesJson!) ?? new Dictionary<string, decimal>()
                : new Dictionary<string, decimal>();
            
            // Ensure base currency is always 1
            manualRates[baseCurrency] = 1m;
            
            // Initialize default rates for all supported currencies if not present
            // Default rates (approximate as of 2025)
            var defaultRates = new Dictionary<string, Dictionary<string, decimal>>
            {
                ["USD"] = new Dictionary<string, decimal> { ["USD"] = 1m, ["DOP"] = 61m, ["EUR"] = 0.95m },
                ["DOP"] = new Dictionary<string, decimal> { ["USD"] = 0.0164m, ["DOP"] = 1m, ["EUR"] = 0.0156m },
                ["EUR"] = new Dictionary<string, decimal> { ["USD"] = 1.05m, ["DOP"] = 64m, ["EUR"] = 1m }
            };
            
            // Initialize missing rates with sensible defaults
            var supportedCurrencies = new[] { "USD", "DOP", "EUR" };
            foreach (var currency in supportedCurrencies)
            {
                if (currency != baseCurrency && !manualRates.ContainsKey(currency))
                {
                    // Try to get default rate for this currency relative to base
                    if (defaultRates.ContainsKey(baseCurrency) && defaultRates[baseCurrency].ContainsKey(currency))
                    {
                        manualRates[currency] = defaultRates[baseCurrency][currency];
                        _logger.LogInformation("Initialized default rate for {Currency}: {Rate}", currency, manualRates[currency]);
                    }
                    else
                    {
                        // Fallback to a reasonable default
                        manualRates[currency] = 1m;
                        _logger.LogInformation("Initialized fallback rate for {Currency}: 1", currency);
                    }
                }
            }
            
            _logger.LogInformation("Final manual rates being returned: {Rates}", JsonSerializer.Serialize(manualRates));

            var rounding = !string.IsNullOrWhiteSpace(company.CurrencyRoundingRuleJson)
                ? JsonSerializer.Deserialize<Dictionary<string, int>>(company.CurrencyRoundingRuleJson!) ?? new Dictionary<string, int>()
                : new Dictionary<string, int>();
            
            // Initialize default rounding rules for all currencies
            foreach (var currency in supportedCurrencies)
            {
                if (!rounding.ContainsKey(currency))
                {
                    rounding[currency] = 2; // Default to 2 decimal places
                }
            }

            var dto = new CurrencySettingsDto
            {
                CurrencyBase = baseCurrency,
                EnabledCurrencies = enabled,
                ManualRates = manualRates,
                LockedUntil = company.CurrencyLockedUntil,
                RoundingRule = rounding
            };
            
            _logger.LogInformation("DTO being returned: {DTO}", JsonSerializer.Serialize(dto));
            
            return dto;
        }

        public async Task UpdateCurrencySettingsAsync(int companyId, CurrencySettingsDto settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (string.IsNullOrWhiteSpace(settings.CurrencyBase)) throw new ArgumentException("CurrencyBase is required");
            if (!settings.EnabledCurrencies.Contains(settings.CurrencyBase)) settings.EnabledCurrencies.Add(settings.CurrencyBase);
            
            // Ensure base currency is always 1
            settings.ManualRates[settings.CurrencyBase] = 1m;
            
            // Validate that all enabled currencies have rates
            foreach (var currency in settings.EnabledCurrencies)
            {
                if (!settings.ManualRates.ContainsKey(currency) || settings.ManualRates[currency] <= 0)
                {
                    if (currency != settings.CurrencyBase)
                    {
                        throw new ArgumentException($"Rate for {currency} must be greater than 0");
                    }
                }
            }

            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == companyId);
            if (company == null) throw new InvalidOperationException("Company not found");

            // Persist all fields into Company
            company.Currency = settings.CurrencyBase; // legacy field for compatibility
            company.CurrencyBase = settings.CurrencyBase;
            company.EnabledCurrenciesJson = JsonSerializer.Serialize(settings.EnabledCurrencies);
            company.ManualRatesJson = JsonSerializer.Serialize(settings.ManualRates);
            company.CurrencyLockedUntil = settings.LockedUntil;
            company.CurrencyRoundingRuleJson = JsonSerializer.Serialize(settings.RoundingRule ?? new Dictionary<string,int>());
            company.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        #region Checkout Settings Methods

        public async Task<CheckoutSettingsDto?> GetCheckoutSettingsAsync()
        {
            var currentCompany = await _context.Companies.FirstOrDefaultAsync();
            if (currentCompany == null)
            {
                return null;
            }

            var settings = await _context.CheckoutSettings
                .FirstOrDefaultAsync(cs => cs.CompanyId == currentCompany.Id);

            if (settings == null)
            {
                return null;
            }

            return MapToDto(settings);
        }

        public async Task<CheckoutSettingsDto> CreateDefaultCheckoutSettingsAsync()
        {
            var currentCompany = await _context.Companies.FirstOrDefaultAsync();
            if (currentCompany == null)
            {
                throw new InvalidOperationException("No company found in the system");
            }

            // Check if settings already exist
            var existingSettings = await _context.CheckoutSettings
                .FirstOrDefaultAsync(cs => cs.CompanyId == currentCompany.Id);
            
            if (existingSettings != null)
            {
                return MapToDto(existingSettings);
            }

            // Create default settings
            var defaultSettings = new CheckoutSettings
            {
                CompanyId = currentCompany.Id,
                ContactMethod = "email",
                FullNameOption = "firstAndLast",
                CompanyNameField = "optional",
                AddressLine2Field = "optional",
                PhoneNumberField = "optional",
                RequireShippingAddress = true,
                RequireBillingAddress = true,
                AllowGuestCheckout = true,
                CollectMarketingConsent = false,
                ShowTermsAndConditions = true,
                CheckoutLogoAlignment = "center",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.CheckoutSettings.Add(defaultSettings);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Default checkout settings created for company {CompanyId}", currentCompany.Id);
            return MapToDto(defaultSettings);
        }

        public async Task<CheckoutSettingsDto> UpdateCheckoutSettingsAsync(UpdateCheckoutSettingsDto request)
        {
            var currentCompany = await _context.Companies.FirstOrDefaultAsync();
            if (currentCompany == null)
            {
                throw new InvalidOperationException("No company found in the system");
            }

            var settings = await _context.CheckoutSettings
                .FirstOrDefaultAsync(cs => cs.CompanyId == currentCompany.Id);

            if (settings == null)
            {
                // Create new settings if they don't exist
                settings = new CheckoutSettings
                {
                    CompanyId = currentCompany.Id,
                    CreatedAt = DateTime.UtcNow
                };
                _context.CheckoutSettings.Add(settings);
            }

            // Update settings from request
            settings.ContactMethod = request.ContactMethod;
            settings.FullNameOption = request.FullNameOption;
            settings.CompanyNameField = request.CompanyNameField;
            settings.AddressLine2Field = request.AddressLine2Field;
            settings.PhoneNumberField = request.PhoneNumberField;
            settings.RequireShippingAddress = request.RequireShippingAddress;
            settings.RequireBillingAddress = request.RequireBillingAddress;
            settings.AllowGuestCheckout = request.AllowGuestCheckout;
            settings.CollectMarketingConsent = request.CollectMarketingConsent;
            settings.ShowTermsAndConditions = request.ShowTermsAndConditions;
            settings.TermsAndConditionsUrl = request.TermsAndConditionsUrl;
            // Branding
            settings.CheckoutLogoUrl = string.IsNullOrWhiteSpace(request.CheckoutLogoUrl) ? settings.CheckoutLogoUrl : request.CheckoutLogoUrl;
            settings.CheckoutLogoAlignment = string.IsNullOrWhiteSpace(request.CheckoutLogoAlignment) ? settings.CheckoutLogoAlignment : request.CheckoutLogoAlignment;
            settings.CheckoutLogoWidthPx = request.CheckoutLogoWidthPx ?? settings.CheckoutLogoWidthPx;
            if (!string.IsNullOrWhiteSpace(request.CheckoutPayButtonColor)) settings.CheckoutPayButtonColor = request.CheckoutPayButtonColor;
            if (!string.IsNullOrWhiteSpace(request.CheckoutPayButtonTextColor)) settings.CheckoutPayButtonTextColor = request.CheckoutPayButtonTextColor;
            settings.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Checkout settings updated for company {CompanyId}", currentCompany.Id);
            return MapToDto(settings);
        }

        public async Task<CheckoutSettingsDto> ResetCheckoutSettingsAsync()
        {
            var currentCompany = await _context.Companies.FirstOrDefaultAsync();
            if (currentCompany == null)
            {
                throw new InvalidOperationException("No company found in the system");
            }

            var settings = await _context.CheckoutSettings
                .FirstOrDefaultAsync(cs => cs.CompanyId == currentCompany.Id);

            if (settings != null)
            {
                // Reset to default values
                settings.ContactMethod = "email";
                settings.FullNameOption = "firstAndLast";
                settings.CompanyNameField = "optional";
                settings.AddressLine2Field = "optional";
                settings.PhoneNumberField = "optional";
                settings.RequireShippingAddress = true;
                settings.RequireBillingAddress = true;
                settings.AllowGuestCheckout = true;
                settings.CollectMarketingConsent = false;
                settings.ShowTermsAndConditions = true;
                settings.TermsAndConditionsUrl = null;
                settings.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Checkout settings reset for company {CompanyId}", currentCompany.Id);
                return MapToDto(settings);
            }
            else
            {
                // Create default settings if they don't exist
                return await CreateDefaultCheckoutSettingsAsync();
            }
        }

        private CheckoutSettingsDto MapToDto(CheckoutSettings settings)
        {
            return new CheckoutSettingsDto
            {
                Id = settings.Id,
                CompanyId = settings.CompanyId,
                ContactMethod = settings.ContactMethod,
                FullNameOption = settings.FullNameOption,
                CompanyNameField = settings.CompanyNameField,
                AddressLine2Field = settings.AddressLine2Field,
                PhoneNumberField = settings.PhoneNumberField,
                RequireShippingAddress = settings.RequireShippingAddress,
                RequireBillingAddress = settings.RequireBillingAddress,
                AllowGuestCheckout = settings.AllowGuestCheckout,
                CollectMarketingConsent = settings.CollectMarketingConsent,
                ShowTermsAndConditions = settings.ShowTermsAndConditions,
                TermsAndConditionsUrl = settings.TermsAndConditionsUrl,
                CheckoutLogoUrl = settings.CheckoutLogoUrl,
                CheckoutLogoAlignment = settings.CheckoutLogoAlignment,
                CheckoutLogoWidthPx = settings.CheckoutLogoWidthPx,
                CheckoutPayButtonColor = settings.CheckoutPayButtonColor,
                CheckoutPayButtonTextColor = settings.CheckoutPayButtonTextColor,
                CreatedAt = settings.CreatedAt,
                UpdatedAt = settings.UpdatedAt
            };
        }

        #endregion
    }
}
