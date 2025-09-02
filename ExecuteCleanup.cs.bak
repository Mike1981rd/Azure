using Microsoft.EntityFrameworkCore;
using WebsiteBuilderAPI.Data;

// Temporary file to execute cleanup
public class ExecuteCleanup
{
    public static async Task Main()
    {
        var connectionString = "Host=localhost;Database=websitebuilderdb;Username=postgres;Password=dota2.top.1";
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        
        using (var context = new ApplicationDbContext(optionsBuilder.Options))
        {
            await context.Database.ExecuteSqlRawAsync("DELETE FROM \"CustomerNotificationPreferences\"");
            Console.WriteLine("Cleanup completed successfully!");
        }
    }
}