using Microsoft.EntityFrameworkCore;
using WebsiteBuilderAPI.Models;
using WebsiteBuilderAPI.Models.Components;
using WebsiteBuilderAPI.Models.ThemeConfig;

namespace WebsiteBuilderAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets para todas las entidades
        public DbSet<Company> Companies { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<Collection> Collections { get; set; }
        public DbSet<CollectionProduct> CollectionProducts { get; set; }
        public DbSet<WebsitePage> WebsitePages { get; set; }
        public DbSet<PageSection> PageSections { get; set; }
        public DbSet<PageSectionChild> PageSectionChildren { get; set; }
        public DbSet<ThemeSettings> ThemeSettings { get; set; }
        public DbSet<NavigationMenu> NavigationMenus { get; set; }
        public DbSet<EditorHistory> EditorHistories { get; set; }
        public DbSet<StructuralComponentsSettings> StructuralComponentsSettings { get; set; }
        public DbSet<RoomReservationCart> RoomReservationCarts { get; set; }
        public DbSet<ProductShoppingCart> ProductShoppingCarts { get; set; }
        public DbSet<Perro> Perros { get; set; }
        public DbSet<Cat> Cats { get; set; }
        public DbSet<Gato> Gatos { get; set; }
        
        // Entidades de autenticación
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        
        // Entidades de empresa
        public DbSet<PaymentProvider> PaymentProviders { get; set; }
        public DbSet<CheckoutSettings> CheckoutSettings { get; set; }
        public DbSet<ShippingZone> ShippingZones { get; set; }
        public DbSet<ShippingRate> ShippingRates { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<NotificationSettings> NotificationSettings { get; set; }
        
        // Entidades de clientes
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerAddress> CustomerAddresses { get; set; }
        public DbSet<CustomerPaymentMethod> CustomerPaymentMethods { get; set; }
        public DbSet<CustomerNotificationPreference> CustomerNotificationPreferences { get; set; }
        public DbSet<CustomerDevice> CustomerDevices { get; set; }
        public DbSet<CustomerWishlistItem> CustomerWishlistItems { get; set; }
        public DbSet<CustomerCoupon> CustomerCoupons { get; set; }
        public DbSet<CustomerSecurityQuestion> CustomerSecurityQuestions { get; set; }
        
        // Dominios y DNS
        public DbSet<Domain> Domains { get; set; }
        public DbSet<DnsRecord> DnsRecords { get; set; }
        
        // Entidades de reservaciones
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<ReservationPayment> ReservationPayments { get; set; }
        
        // Newsletter Subscribers
        public DbSet<NewsletterSubscriber> NewsletterSubscribers { get; set; }
        
        // Páginas CMS
        public DbSet<Pagina> Paginas { get; set; }
        
        // Políticas
        public DbSet<Policy> Policies { get; set; }
        
        // Orders (E-commerce)
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderPayment> OrderPayments { get; set; }
        public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }
        
        // Sistema de Disponibilidad
        public DbSet<RoomAvailability> RoomAvailabilities { get; set; }
        public DbSet<RoomBlockPeriod> RoomBlockPeriods { get; set; }
        public DbSet<AvailabilityRule> AvailabilityRules { get; set; }
        
        // Website Builder Theme Configuration v2.0
        public DbSet<GlobalThemeConfig> GlobalThemeConfigs { get; set; }
        
        // Reviews System
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ReviewMedia> ReviewMedia { get; set; }
        public DbSet<ReviewInteraction> ReviewInteractions { get; set; }
        public DbSet<ReviewStatistics> ReviewStatistics { get; set; }
        
        // Host System
        public DbSet<WebsiteBuilderAPI.Models.Host> Hosts { get; set; }
        public DbSet<HostReview> HostReviews { get; set; }
        public DbSet<ConfigOption> ConfigOptions { get; set; }
        
        // WhatsApp entities
        public DbSet<WhatsAppConfig> WhatsAppConfigs { get; set; }
        public DbSet<WhatsAppMessage> WhatsAppMessages { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<ContactNotificationSettings> ContactNotificationSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configurar Entity Framework para NO convertir nombres a snake_case
            // Usar los nombres exactos como están en PostgreSQL (PascalCase)
            modelBuilder.UseIdentityColumns();

            // Configuración de Company
            modelBuilder.Entity<Company>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.HasIndex(e => e.Domain).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Configuración de Room
            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.BasePrice).HasPrecision(10, 2);
                entity.Property(e => e.Images).HasColumnType("jsonb");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasOne(e => e.Company)
                    .WithMany(h => h.Rooms)
                    .HasForeignKey(e => e.CompanyId);
            });

            // Configuración de Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.BasePrice).HasPrecision(10, 2);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasOne(e => e.Company)
                    .WithMany(h => h.Products)
                    .HasForeignKey(e => e.CompanyId);
            });

            // Configuración de ProductVariant
            modelBuilder.Entity<ProductVariant>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Price).HasPrecision(10, 2);
                entity.Property(e => e.Attributes).HasColumnType("jsonb");
                entity.HasOne(e => e.Product)
                    .WithMany(p => p.Variants)
                    .HasForeignKey(e => e.ProductId);
            });

            // Configuración de Collection
            modelBuilder.Entity<Collection>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Handle).IsRequired().HasMaxLength(255);
                // Unique index that only applies to non-deleted collections
                entity.HasIndex(e => new { e.CompanyId, e.Handle })
                    .IsUnique()
                    .HasFilter("\"DeletedAt\" IS NULL");  // PostgreSQL syntax for filtered index
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasOne(e => e.Company)
                    .WithMany(c => c.Collections)
                    .HasForeignKey(e => e.CompanyId);
            });

            // Configuración de CollectionProduct (tabla intermedia muchos a muchos)
            modelBuilder.Entity<CollectionProduct>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.CollectionId, e.ProductId }).IsUnique();
                entity.Property(e => e.AddedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasOne(e => e.Collection)
                    .WithMany(c => c.CollectionProducts)
                    .HasForeignKey(e => e.CollectionId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Product)
                    .WithMany(p => p.CollectionProducts)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración de WebsitePage
            modelBuilder.Entity<WebsitePage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PageType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasOne(e => e.Company)
                    .WithMany(h => h.WebsitePages)
                    .HasForeignKey(e => e.CompanyId);
            });

            // Configuración de PageSection
            modelBuilder.Entity<PageSection>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SectionType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Config).HasColumnType("jsonb").IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasOne(e => e.Page)
                    .WithMany(p => p.Sections)
                    .HasForeignKey(e => e.PageId);
            });

            // Configuración de ThemeSettings
            modelBuilder.Entity<ThemeSettings>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ColorScheme).HasColumnType("jsonb");
                entity.Property(e => e.Typography).HasColumnType("jsonb");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasOne(e => e.Company)
                    .WithOne(h => h.ThemeSettings)
                    .HasForeignKey<ThemeSettings>(e => e.CompanyId);
            });

            // Configuración de NavigationMenu
            modelBuilder.Entity<NavigationMenu>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.MenuType).HasMaxLength(50);
                entity.Property(e => e.Items).HasColumnType("jsonb");
                entity.HasOne(e => e.Company)
                    .WithMany(h => h.NavigationMenus)
                    .HasForeignKey(e => e.CompanyId);
            });

            // Configuración de EditorHistory
            modelBuilder.Entity<EditorHistory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.StateData).HasColumnType("jsonb").IsRequired();
                entity.Property(e => e.Description).HasMaxLength(255);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasOne(e => e.Page)
                    .WithMany(p => p.EditorHistories)
                    .HasForeignKey(e => e.PageId);
            });

            // Configuración de RoomReservationCart
            modelBuilder.Entity<RoomReservationCart>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SessionId).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Items).HasColumnType("jsonb");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId);
            });

            // Configuración de ProductShoppingCart
            modelBuilder.Entity<ProductShoppingCart>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SessionId).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Items).HasColumnType("jsonb");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId);
            });

            // Configuración de User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId)
                    .IsRequired(false);
            });

            // Configuración de Role
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.Description).IsRequired().HasMaxLength(255);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Configuración de UserRole (tabla de unión)
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId });
                entity.Property(e => e.AssignedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasOne(e => e.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(e => e.UserId);
                entity.HasOne(e => e.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(e => e.RoleId);
                entity.HasOne(e => e.AssignedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.AssignedByUserId)
                    .IsRequired(false);
            });

            // Configuración de Permission
            modelBuilder.Entity<Permission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Resource).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(255);
                entity.HasIndex(e => new { e.Resource, e.Action }).IsUnique();
            });

            // Configuración de RolePermission (tabla de unión)
            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(e => new { e.RoleId, e.PermissionId });
                entity.Property(e => e.GrantedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasOne(e => e.Role)
                    .WithMany(r => r.RolePermissions)
                    .HasForeignKey(e => e.RoleId);
                entity.HasOne(e => e.Permission)
                    .WithMany(p => p.RolePermissions)
                    .HasForeignKey(e => e.PermissionId);
            });

            // Configuración de PaymentProvider
            modelBuilder.Entity<PaymentProvider>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Provider).IsRequired().HasMaxLength(50);
                entity.Property(e => e.TransactionFee).HasPrecision(5, 4); // 99.9999%
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasOne(e => e.Company)
                    .WithMany(h => h.PaymentProviders)
                    .HasForeignKey(e => e.CompanyId);
                
                // Índices para optimización
                entity.HasIndex(e => new { e.CompanyId, e.Provider }).IsUnique();
                entity.HasIndex(e => e.IsActive);
            });

            // Configuración de CheckoutSettings
            modelBuilder.Entity<CheckoutSettings>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ContactMethod).IsRequired().HasMaxLength(20);
                entity.Property(e => e.FullNameOption).IsRequired().HasMaxLength(20);
                entity.Property(e => e.CompanyNameField).IsRequired().HasMaxLength(20);
                entity.Property(e => e.AddressLine2Field).IsRequired().HasMaxLength(20);
                entity.Property(e => e.PhoneNumberField).IsRequired().HasMaxLength(20);
                entity.Property(e => e.TermsAndConditionsUrl).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasOne(e => e.Company)
                    .WithOne()
                    .HasForeignKey<CheckoutSettings>(e => e.CompanyId);
                
                // Índice único para asegurar una configuración por empresa
                entity.HasIndex(e => e.CompanyId).IsUnique();
            });

            // Configuración de ShippingZone
            modelBuilder.Entity<ShippingZone>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ZoneType).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Countries).HasColumnType("jsonb");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId);
                
                // Índices para optimización
                entity.HasIndex(e => e.CompanyId);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.DisplayOrder);
            });

            // Configuración de ShippingRate
            modelBuilder.Entity<ShippingRate>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RateType).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Condition).HasMaxLength(100);
                entity.Property(e => e.Price).HasPrecision(10, 2);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasOne(e => e.ShippingZone)
                    .WithMany(z => z.Rates)
                    .HasForeignKey(e => e.ShippingZoneId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                // Índices para optimización
                entity.HasIndex(e => e.ShippingZoneId);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.DisplayOrder);
            });

            // Configuración de Location
            modelBuilder.Entity<Location>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Address).HasMaxLength(255);
                entity.Property(e => e.Apartment).HasMaxLength(100);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.State).HasMaxLength(100);
                entity.Property(e => e.Country).HasMaxLength(100);
                entity.Property(e => e.PinCode).HasMaxLength(20);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId);
                
                // Índices para optimización
                entity.HasIndex(e => e.CompanyId);
                entity.HasIndex(e => e.IsDefault);
            });

            // Configuración de NotificationSettings
            modelBuilder.Entity<NotificationSettings>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NotificationType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
                entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId);
                
                // Índices para optimización
                entity.HasIndex(e => new { e.CompanyId, e.NotificationType }).IsUnique();
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.SortOrder);
            });

            // Configuración de Customer
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CustomerId).IsRequired().HasMaxLength(10);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Country).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LoyaltyTier).HasMaxLength(20);
                entity.Property(e => e.AccountBalance).HasPrecision(10, 2);
                entity.Property(e => e.TotalSpent).HasPrecision(10, 2);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId);
                
                // Índices únicos y de optimización
                entity.HasIndex(e => new { e.CompanyId, e.Email }).IsUnique();
                entity.HasIndex(e => new { e.CompanyId, e.Username }).IsUnique();
                entity.HasIndex(e => e.CustomerId).IsUnique();
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.Country);
                entity.HasIndex(e => e.DeletedAt); // Para soft delete
            });

            // Configuración de CustomerAddress
            modelBuilder.Entity<CustomerAddress>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Label).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Street).IsRequired().HasMaxLength(255);
                entity.Property(e => e.City).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Country).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                entity.HasOne(e => e.Customer)
                    .WithMany(c => c.Addresses)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.IsDefault);
            });

            // Configuración de CustomerPaymentMethod
            modelBuilder.Entity<CustomerPaymentMethod>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CardType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CardholderName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Last4Digits).IsRequired().HasMaxLength(4);
                entity.Property(e => e.ExpiryMonth).IsRequired().HasMaxLength(2);
                entity.Property(e => e.ExpiryYear).IsRequired().HasMaxLength(4);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                entity.HasOne(e => e.Customer)
                    .WithMany(c => c.PaymentMethods)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.IsPrimary);
            });

            // Configuración de CustomerNotificationPreference
            modelBuilder.Entity<CustomerNotificationPreference>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // Configure one-to-one relationship
                entity.HasOne(e => e.Customer)
                    .WithOne(c => c.NotificationPreference)
                    .HasForeignKey<CustomerNotificationPreference>(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                // Ensure one notification preference per customer
                entity.HasIndex(e => e.CustomerId).IsUnique();
                
                // Configure string properties
                entity.Property(e => e.DoNotDisturbStart).HasMaxLength(10);
                entity.Property(e => e.DoNotDisturbEnd).HasMaxLength(10);
                entity.Property(e => e.Timezone).HasMaxLength(100);
            });

            // Configuración de CustomerDevice
            modelBuilder.Entity<CustomerDevice>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Browser).IsRequired().HasMaxLength(50);
                entity.Property(e => e.DeviceType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.DeviceName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.OperatingSystem).IsRequired().HasMaxLength(50);
                entity.Property(e => e.IpAddress).IsRequired().HasMaxLength(45);
                entity.Property(e => e.Location).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastActivity).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.FirstSeen).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                entity.HasOne(e => e.Customer)
                    .WithMany(c => c.Devices)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.LastActivity);
            });

            // Configuración de CustomerWishlistItem
            modelBuilder.Entity<CustomerWishlistItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AddedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                entity.HasOne(e => e.Customer)
                    .WithMany(c => c.WishlistItems)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Product)
                    .WithMany()
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.ProductVariant)
                    .WithMany()
                    .HasForeignKey(e => e.ProductVariantId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);
                
                // Unique constraint to prevent duplicate items
                entity.HasIndex(e => new { e.CustomerId, e.ProductId, e.ProductVariantId }).IsUnique();
            });

            // Configuración de CustomerCoupon
            modelBuilder.Entity<CustomerCoupon>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(200);
                entity.Property(e => e.DiscountType).IsRequired().HasMaxLength(20);
                entity.Property(e => e.DiscountAmount).HasPrecision(10, 2);
                entity.Property(e => e.MinimumPurchase).HasPrecision(10, 2);
                entity.Property(e => e.ValidFrom).IsRequired();
                entity.Property(e => e.ValidUntil).IsRequired();
                entity.Property(e => e.ApplicableProducts).HasColumnType("jsonb");
                entity.Property(e => e.ApplicableCategories).HasColumnType("jsonb");
                
                entity.HasOne(e => e.Customer)
                    .WithMany(c => c.Coupons)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.Code);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => new { e.ValidFrom, e.ValidUntil });
            });

            // Configuración de NewsletterSubscriber
            modelBuilder.Entity<NewsletterSubscriber>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.SourcePage).HasMaxLength(255);
                entity.Property(e => e.SourceCampaign).HasMaxLength(50);
                entity.Property(e => e.Language).HasMaxLength(50).HasDefaultValue("es");
                entity.Property(e => e.UnsubscribeReason).HasMaxLength(255);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                // Relación con Company
                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                // Relación opcional con Customer (si se convirtió)
                entity.HasOne(e => e.Customer)
                    .WithMany()
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .IsRequired(false);
                
                // Índices únicos y de optimización
                entity.HasIndex(e => new { e.CompanyId, e.Email })
                    .IsUnique()
                    .HasFilter("\"DeletedAt\" IS NULL"); // Único por company, solo activos
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.ConvertedToCustomer);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.OptInDate);
                entity.HasIndex(e => e.Language);
                entity.HasIndex(e => e.DeletedAt); // Para soft delete
            });

            // Configuración de Pagina
            modelBuilder.Entity<Pagina>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Slug).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Content).HasColumnType("text");
                entity.Property(e => e.PublishStatus).IsRequired().HasMaxLength(20).HasDefaultValue("draft");
                entity.Property(e => e.Template).IsRequired().HasMaxLength(50).HasDefaultValue("default");
                
                // SEO fields
                entity.Property(e => e.MetaTitle).HasMaxLength(255);
                entity.Property(e => e.MetaDescription).HasMaxLength(500);
                entity.Property(e => e.MetaKeywords).HasMaxLength(500);
                entity.Property(e => e.OgImage).HasMaxLength(500);
                entity.Property(e => e.OgTitle).HasMaxLength(255);
                entity.Property(e => e.OgDescription).HasMaxLength(500);
                
                // Search engine control
                entity.Property(e => e.CanonicalUrl).HasMaxLength(500);
                entity.Property(e => e.Robots).HasMaxLength(100);
                
                // Timestamps
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                // Relations
                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.CreatedBy)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedByUserId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .IsRequired(false);
                
                entity.HasOne(e => e.UpdatedBy)
                    .WithMany()
                    .HasForeignKey(e => e.UpdatedByUserId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .IsRequired(false);
                
                // Indices
                entity.HasIndex(e => new { e.CompanyId, e.Slug }).IsUnique();
                entity.HasIndex(e => e.IsVisible);
                entity.HasIndex(e => e.PublishStatus);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.PublishedAt);
            });
            
            // Configuración de Policy
            modelBuilder.Entity<Policy>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Content).HasColumnType("text");
                entity.Property(e => e.IsRequired).HasDefaultValue(false);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                // Relación con Company
                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                // Índice único para evitar duplicados de tipo por empresa
                entity.HasIndex(e => new { e.CompanyId, e.Type }).IsUnique();
            });
            
            // Configuración de RoomAvailability
            modelBuilder.Entity<RoomAvailability>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Date).IsRequired();
                entity.Property(e => e.IsAvailable).HasDefaultValue(true);
                entity.Property(e => e.IsBlocked).HasDefaultValue(false);
                entity.Property(e => e.BlockReason).HasMaxLength(255);
                entity.Property(e => e.CustomPrice).HasPrecision(10, 2);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                // Índice único para evitar duplicados
                entity.HasIndex(e => new { e.RoomId, e.Date }).IsUnique();
                
                // Relaciones
                entity.HasOne(e => e.Room)
                    .WithMany()
                    .HasForeignKey(e => e.RoomId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.Reservation)
                    .WithMany()
                    .HasForeignKey(e => e.ReservationId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
            
            // Configuración de RoomBlockPeriod
            modelBuilder.Entity<RoomBlockPeriod>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.StartDate).IsRequired();
                entity.Property(e => e.EndDate).IsRequired();
                entity.Property(e => e.Reason).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.RecurrencePattern).HasMaxLength(50);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                // Relaciones
                entity.HasOne(e => e.Room)
                    .WithMany()
                    .HasForeignKey(e => e.RoomId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.CreatedBy)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            
            // Configuración de AvailabilityRule
            modelBuilder.Entity<AvailabilityRule>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RuleType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.RuleValue).IsRequired().HasColumnType("jsonb");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.Priority).HasDefaultValue(0);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                // Relaciones
                entity.HasOne(e => e.Room)
                    .WithMany()
                    .HasForeignKey(e => e.RoomId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                // Índices
                entity.HasIndex(e => new { e.CompanyId, e.RoomId, e.RuleType });
                entity.HasIndex(e => e.IsActive);
            });
            
            // Configuración de Order
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.OrderStatus).IsRequired().HasMaxLength(20);
                entity.Property(e => e.PaymentStatus).IsRequired().HasMaxLength(20);
                entity.Property(e => e.DeliveryStatus).IsRequired().HasMaxLength(20);
                entity.Property(e => e.SubTotal).HasPrecision(10, 2);
                entity.Property(e => e.DiscountAmount).HasPrecision(10, 2);
                entity.Property(e => e.TaxAmount).HasPrecision(10, 2);
                entity.Property(e => e.ShippingAmount).HasPrecision(10, 2);
                entity.Property(e => e.TotalAmount).HasPrecision(10, 2);
                entity.Property(e => e.Currency).IsRequired().HasMaxLength(3);
                entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CustomerEmail).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ShippingAddressSnapshot).HasColumnType("jsonb");
                entity.Property(e => e.BillingAddressSnapshot).HasColumnType("jsonb");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                // Relaciones
                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Customer)
                    .WithMany()
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.ShippingAddress)
                    .WithMany()
                    .HasForeignKey(e => e.ShippingAddressId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.BillingAddress)
                    .WithMany()
                    .HasForeignKey(e => e.BillingAddressId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                // Índices
                entity.HasIndex(e => e.OrderNumber).IsUnique();
                entity.HasIndex(e => e.CompanyId);
                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.OrderStatus);
                entity.HasIndex(e => e.PaymentStatus);
                entity.HasIndex(e => e.DeliveryStatus);
                entity.HasIndex(e => e.OrderDate);
                entity.HasIndex(e => new { e.CompanyId, e.OrderStatus });
            });
            
            // Configuración de OrderItem
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProductName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ProductAttributes).HasColumnType("jsonb");
                entity.Property(e => e.UnitPrice).HasPrecision(10, 2);
                entity.Property(e => e.DiscountAmount).HasPrecision(10, 2);
                entity.Property(e => e.TaxAmount).HasPrecision(10, 2);
                entity.Property(e => e.TotalPrice).HasPrecision(10, 2);
                
                // Relaciones
                entity.HasOne(e => e.Order)
                    .WithMany(o => o.OrderItems)
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.Product)
                    .WithMany()
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.ProductVariant)
                    .WithMany()
                    .HasForeignKey(e => e.ProductVariantId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                // Índices
                entity.HasIndex(e => e.OrderId);
                entity.HasIndex(e => e.ProductId);
            });
            
            // Configuración de OrderPayment
            modelBuilder.Entity<OrderPayment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasPrecision(10, 2);
                entity.Property(e => e.RefundAmount).HasPrecision(10, 2);
                entity.Property(e => e.Currency).IsRequired().HasMaxLength(3);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.Property(e => e.GatewayResponse).HasColumnType("jsonb");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                // Relaciones
                entity.HasOne(e => e.Order)
                    .WithMany(o => o.OrderPayments)
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.PaymentProvider)
                    .WithMany()
                    .HasForeignKey(e => e.PaymentProviderId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.CustomerPaymentMethod)
                    .WithMany()
                    .HasForeignKey(e => e.CustomerPaymentMethodId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                // Índices
                entity.HasIndex(e => e.OrderId);
                entity.HasIndex(e => e.TransactionId);
                entity.HasIndex(e => e.Status);
            });
            
            // Configuración de OrderStatusHistory
            modelBuilder.Entity<OrderStatusHistory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.StatusType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Metadata).HasColumnType("jsonb");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                // Relaciones
                entity.HasOne(e => e.Order)
                    .WithMany(o => o.OrderStatusHistories)
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                // Índices
                entity.HasIndex(e => e.OrderId);
                entity.HasIndex(e => e.StatusType);
                entity.HasIndex(e => e.Timestamp);
            });
            
            // Configuración de GlobalThemeConfig (Website Builder v2.0)
            modelBuilder.Entity<GlobalThemeConfig>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // Unique constraint: one configuration per company
                entity.HasIndex(e => e.CompanyId).IsUnique();
                
                // JSONB columns for each configuration module
                entity.Property(e => e.Appearance).HasColumnType("jsonb");
                entity.Property(e => e.Typography).HasColumnType("jsonb");
                entity.Property(e => e.ColorSchemes).HasColumnType("jsonb");
                entity.Property(e => e.ProductCards).HasColumnType("jsonb");
                entity.Property(e => e.ProductBadges).HasColumnType("jsonb");
                entity.Property(e => e.Cart).HasColumnType("jsonb");
                entity.Property(e => e.Favicon).HasColumnType("jsonb");
                entity.Property(e => e.Navigation).HasColumnType("jsonb");
                entity.Property(e => e.SocialMedia).HasColumnType("jsonb");
                entity.Property(e => e.Swatches).HasColumnType("jsonb");
                
                // Version control
                entity.Property(e => e.ConfigVersion)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasDefaultValue("2.0.0");
                
                // Timestamps
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                // Publishing status
                entity.Property(e => e.IsPublished).HasDefaultValue(false);
                
                // Relationship with Company
                entity.HasOne(e => e.Company)
                    .WithOne()
                    .HasForeignKey<GlobalThemeConfig>(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                // Indexes for performance
                entity.HasIndex(e => e.IsPublished);
                entity.HasIndex(e => e.UpdatedAt);
            });

            // Configuración de Review
            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AuthorName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.AuthorEmail).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Content).IsRequired().HasMaxLength(5000);
                entity.Property(e => e.Rating).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                // Relationships
                entity.HasOne(e => e.Product)
                    .WithMany()
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.Room)
                    .WithMany()
                    .HasForeignKey(e => e.RoomId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.Customer)
                    .WithMany()
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.ApprovedBy)
                    .WithMany()
                    .HasForeignKey(e => e.ApprovedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                entity.HasOne(e => e.RepliedBy)
                    .WithMany()
                    .HasForeignKey(e => e.RepliedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);
                
                // Indexes
                entity.HasIndex(e => new { e.CompanyId, e.ProductId }).HasFilter("\"ProductId\" IS NOT NULL");
                entity.HasIndex(e => new { e.CompanyId, e.RoomId }).HasFilter("\"RoomId\" IS NOT NULL");
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.Rating);
                entity.HasIndex(e => e.CreatedAt);
                
                // Ensure either ProductId or RoomId is set (but not both)
                entity.HasCheckConstraint("CK_Review_ProductOrRoom", 
                    "(\"ProductId\" IS NOT NULL AND \"RoomId\" IS NULL) OR (\"ProductId\" IS NULL AND \"RoomId\" IS NOT NULL)");
            });

            // Configuración de ReviewMedia
            modelBuilder.Entity<ReviewMedia>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.MediaUrl).IsRequired().HasMaxLength(500);
                entity.Property(e => e.UploadedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                entity.HasOne(e => e.Review)
                    .WithMany(r => r.Media)
                    .HasForeignKey(e => e.ReviewId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasIndex(e => e.ReviewId);
            });

            // Configuración de ReviewInteraction
            modelBuilder.Entity<ReviewInteraction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                entity.HasOne(e => e.Review)
                    .WithMany(r => r.Interactions)
                    .HasForeignKey(e => e.ReviewId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.Customer)
                    .WithMany()
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                // Unique constraint to prevent duplicate interactions
                entity.HasIndex(e => new { e.ReviewId, e.CustomerId, e.Type })
                    .IsUnique()
                    .HasFilter("\"CustomerId\" IS NOT NULL");
                    
                entity.HasIndex(e => new { e.ReviewId, e.SessionId, e.Type })
                    .IsUnique()
                    .HasFilter("\"SessionId\" IS NOT NULL");
            });

            // Configuración de ReviewStatistics
            modelBuilder.Entity<ReviewStatistics>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AverageRating).HasPrecision(3, 2);
                entity.Property(e => e.PositivePercentage).HasPrecision(5, 2);
                entity.Property(e => e.WeeklyGrowthPercentage).HasPrecision(5, 2);
                entity.Property(e => e.WeeklyTrend).HasColumnType("jsonb");
                entity.Property(e => e.LastCalculatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.Product)
                    .WithMany()
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.Room)
                    .WithMany()
                    .HasForeignKey(e => e.RoomId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                // Unique index for company-product statistics
                entity.HasIndex(e => new { e.CompanyId, e.ProductId })
                    .IsUnique()
                    .HasFilter("\"ProductId\" IS NOT NULL AND \"RoomId\" IS NULL");
                    
                // Unique index for company-room statistics
                entity.HasIndex(e => new { e.CompanyId, e.RoomId })
                    .IsUnique()
                    .HasFilter("\"RoomId\" IS NOT NULL AND \"ProductId\" IS NULL");
                    
                // Index for global company statistics
                entity.HasIndex(e => e.CompanyId)
                    .HasFilter("\"ProductId\" IS NULL AND \"RoomId\" IS NULL");
            });
        }
    }
}