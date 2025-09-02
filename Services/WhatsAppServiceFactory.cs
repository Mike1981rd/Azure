using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebsiteBuilderAPI.Configuration;
using WebsiteBuilderAPI.Data;

namespace WebsiteBuilderAPI.Services
{
    /// <summary>
    /// Factory for creating the appropriate WhatsApp service based on configuration
    /// </summary>
    public interface IWhatsAppServiceFactory
    {
        IWhatsAppService GetService();
        IWhatsAppService GetService(string providerName);
        string GetActiveProvider();
        List<string> GetAvailableProviders();
    }

    /// <summary>
    /// Implementation of WhatsApp service factory
    /// </summary>
    public class WhatsAppServiceFactory : IWhatsAppServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly WhatsAppSettings _settings;
        private readonly ILogger<WhatsAppServiceFactory> _logger;
        private readonly ApplicationDbContext _context;

        public WhatsAppServiceFactory(
            IServiceProvider serviceProvider,
            IOptions<WhatsAppSettings> settings,
            ILogger<WhatsAppServiceFactory> logger,
            ApplicationDbContext context)
        {
            _serviceProvider = serviceProvider;
            _settings = settings.Value;
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Gets the active WhatsApp service based on configuration in database
        /// </summary>
        public IWhatsAppService GetService()
        {
            // Try to get provider from database first (company 1 for now)
            var config = _context.WhatsAppConfigs
                .FirstOrDefault(c => c.CompanyId == 1);
            
            if (config != null && !string.IsNullOrEmpty(config.Provider))
            {
                _logger.LogInformation("Using provider from database: {Provider}", config.Provider);
                return GetService(config.Provider);
            }
            
            // Fallback to settings
            return GetService(_settings.Provider);
        }

        /// <summary>
        /// Gets a specific WhatsApp service by provider name
        /// </summary>
        /// <param name="providerName">Provider name: "Twilio" or "GreenAPI"</param>
        public IWhatsAppService GetService(string providerName)
        {
            // Normalize provider name to handle different casing variations
            var normalizedProvider = providerName?.ToUpperInvariant()?.Replace(" ", "") ?? "";
            
            IWhatsAppService service = normalizedProvider switch
            {
                "TWILIO" => _serviceProvider.GetRequiredService<TwilioWhatsAppService>(),
                "GREENAPI" or "GREENAPI" => _serviceProvider.GetRequiredService<GreenApiWhatsAppService>(),
                _ => throw new ArgumentException($"Unknown WhatsApp provider: {providerName}. Available providers: Twilio, GreenAPI")
            };

            _logger.LogInformation("Selected WhatsApp provider: {ProviderName}", providerName);
            return service;
        }

        /// <summary>
        /// Gets the name of the currently active provider
        /// </summary>
        public string GetActiveProvider()
        {
            return _settings.Provider;
        }

        /// <summary>
        /// Gets list of all available providers
        /// </summary>
        public List<string> GetAvailableProviders()
        {
            return new List<string> { "Twilio", "GreenAPI" };
        }
    }

    /// <summary>
    /// Extension methods for service registration
    /// </summary>
    public static class WhatsAppServiceExtensions
    {
        /// <summary>
        /// Registers WhatsApp services in DI container
        /// </summary>
        public static IServiceCollection AddWhatsAppServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure WhatsApp settings
            services.Configure<WhatsAppSettings>(configuration.GetSection("WhatsApp"));

            // Register individual services
            services.AddScoped<TwilioWhatsAppService>();
            services.AddScoped<GreenApiWhatsAppService>();
            
            // Register Green API specific services
            // Polling service removed - webhook based only

            // Register factory
            services.AddScoped<IWhatsAppServiceFactory, WhatsAppServiceFactory>();

            // Register the active service based on configuration
            services.AddScoped<IWhatsAppService>(provider =>
            {
                var factory = provider.GetRequiredService<IWhatsAppServiceFactory>();
                return factory.GetService();
            });

            // Configure HttpClient specifically for GREEN-API
            services.AddHttpClient<GreenApiWhatsAppService>(client =>
            {
                client.BaseAddress = new Uri("https://api.green-api.com");
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("User-Agent", "WebsiteBuilderAPI/1.0");
            });
            
            // Ensure general HttpClient is also registered
            services.AddHttpClient();

            return services;
        }
    }
}