using Microsoft.EntityFrameworkCore;
using WebsiteBuilderAPI.Models;

namespace WebsiteBuilderAPI.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            // Usar el nuevo PermissionsSeeder para crear permisos y roles
            await PermissionsSeeder.SeedPermissionsAsync(context);
            await PermissionsSeeder.SeedRolesAsync(context);
            
            // Crear empresa por defecto si no existe
            if (!await context.Companies.AnyAsync())
            {
                var defaultCompany = new Company
                {
                    Name = "My Company",
                    Domain = "localhost",
                    Subdomain = "demo",
                    Logo = null,
                    LogoSize = 120,
                    PrimaryColor = "#22c55e",
                    SecondaryColor = "#64748b",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    // Profile Section
                    PhoneNumber = "+1 809-555-0100",
                    ContactEmail = "info@mycompany.com",
                    SenderEmail = "noreply@mycompany.com",
                    // Billing Information
                    LegalBusinessName = "My Company LLC",
                    Country = "DO",
                    Region = "Caribbean",
                    Address = "123 Main Street",
                    Apartment = null,
                    City = "Santo Domingo",
                    State = "DN",
                    PostalCode = "10101",
                    // Time Zone & Units
                    TimeZone = "America/Santo_Domingo",
                    MetricSystem = "metric",
                    WeightUnit = "kg",
                    // Store Currency
                    Currency = "USD",
                    // Order ID Format
                    OrderIdPrefix = "ORD",
                    OrderIdSuffix = null
                };

                await context.Companies.AddAsync(defaultCompany);
                await context.SaveChangesAsync();
            }
            
            // Crear usuario admin por defecto
            await PermissionsSeeder.SeedDefaultUserAsync(context);
        }
    }
}