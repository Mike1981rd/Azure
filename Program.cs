using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Serilog;
using Serilog.Events;
using System.Diagnostics;
using System.Text;
using WebsiteBuilderAPI.Data;
using WebsiteBuilderAPI.Filters;
using WebsiteBuilderAPI.Repositories;
using WebsiteBuilderAPI.Services;

// Configurar Serilog ANTES de crear el WebApplication
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/websitebuilder-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
        retainedFileCountLimit: 30,
        fileSizeLimitBytes: 10_485_760, // 10MB
        rollOnFileSizeLimit: true,
        shared: true,
        flushToDiskInterval: TimeSpan.FromSeconds(1))
    .WriteTo.File(
        path: "logs/errors/error-.log",
        restrictedToMinimumLevel: LogEventLevel.Error,
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
        retainedFileCountLimit: 30)
    .CreateLogger();

try
{
    Log.Information("🚀 Starting WebsiteBuilder API Application");
    
    // Configurar el AppContext para manejar fechas UTC con PostgreSQL
    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", false);
    AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", false);
    
    var builder = WebApplication.CreateBuilder(args);

    // Usar Serilog en lugar del logger por defecto
    builder.Host.UseSerilog();

    // Add services to the container.

    // Configurar Npgsql DataSource con soporte para JSON dinámico
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    Log.Information("Configuring PostgreSQL connection");

    // Log a safe summary of the connection target (no secrets)
    try
    {
        var csb = new Npgsql.NpgsqlConnectionStringBuilder(connectionString);
        var host = csb.Host;
        var port = csb.Port;
        var database = csb.Database;
        var username = csb.Username;
        Log.Information("DB target => Host: {Host}, Port: {Port}, DB: {Database}, User: {User}", host, port, database, username);
    }
    catch { /* ignore logging issues */ }

    var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
    dataSourceBuilder.EnableDynamicJson();
    var dataSource = dataSourceBuilder.Build();

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(dataSource, o => 
        {
            o.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
            o.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), null);
        }));

    // Configurar JWT Authentication
    var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
    var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "WebsiteBuilderAPI";
    var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "WebsiteBuilderClient";

    Log.Information("Configuring JWT Authentication with Issuer: {Issuer}", jwtIssuer);

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };

        // JWT Bearer events removed - they were blocking login
    });

    // Configurar CORS para permitir peticiones desde el frontend Next.js
    Log.Information("Configuring CORS policy");
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(builder =>
        {
            builder.WithOrigins(
                    "http://localhost:3000",
                    "http://localhost:3001", 
                    "http://127.0.0.1:3000",
                    "http://127.0.0.1:3001",
                    "http://172.25.64.1:3000",  // WSL2 host IP
                    "http://172.25.64.1:3001")  // WSL2 host IP alternate port
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
                .SetIsOriginAllowed(origin => true); // Temporalmente permitir todo para debugging
        });
        
        options.AddPolicy("AllowNextJsApp",
            builder =>
            {
                builder.WithOrigins(
                        "http://localhost:3000",
                        "http://localhost:3001",
                        "http://127.0.0.1:3000",
                        "http://127.0.0.1:3001",
                        "http://172.25.64.1:3000",  // WSL2 host IP
                        "http://172.25.64.1:3001",  // WSL2 host IP alternate port
                        "https://websitebuilder-admin-nag9lyc0h.vercel.app",  // Vercel production
                        "https://websitebuilder-admin-azveynj6r.vercel.app",  // Vercel production alternate
                        "https://websitebuilder-admin.vercel.app",  // Vercel production main domain
                        "https://websitebuilder-admin-h3dxx3720.vercel.app")  // Latest deployment
                    .SetIsOriginAllowed(origin => origin.Contains("vercel.app") || origin.Contains("localhost")) // Allow all Vercel preview deployments
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        
        // Nueva política para Vercel + Supabase
        options.AddPolicy("VercelPolicy",
            builder =>
            {
                builder.WithOrigins(
                        "https://websitebuilder-admin.vercel.app",
                        "http://localhost:3000",
                        "http://172.25.64.1:3000")
                    .SetIsOriginAllowed(origin => origin.Contains("vercel.app") || origin.Contains("localhost"))
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
    });

    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            // Configure JSON serialization to use camelCase for property names
            options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            // Don't change dictionary keys - keep them as-is (important for currency codes like USD, EUR, DOP)
            options.JsonSerializerOptions.DictionaryKeyPolicy = null;
            // Serialize enums as strings instead of numbers
            options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        });
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "WebsiteBuilder API",
            Version = "v1",
            Description = "API for Website Builder with file upload support"
        });

        // Enable annotations
        c.EnableAnnotations();
        
        // Configure Swagger to handle file uploads properly
        c.OperationFilter<FileUploadOperationFilter>();
    });
    
    // Add memory cache for theme configuration performance
    builder.Services.AddMemoryCache();
    builder.Services.AddDistributedMemoryCache(); // For development, use Redis in production

    // Register Services - Logging which services are being registered
    Log.Debug("Registering application services");
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<ICompanyService, CompanyService>();
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IRoleService, RoleService>();
    builder.Services.AddScoped<IPermissionService, PermissionService>();
    builder.Services.AddScoped<IUploadService, UploadService>();
    // Configure storage provider (Cloudinary or Local)
    var storageProvider = builder.Configuration["Storage:Provider"] ?? Environment.GetEnvironmentVariable("STORAGE__PROVIDER") ?? "local";
    if (string.Equals(storageProvider, "cloudinary", StringComparison.OrdinalIgnoreCase))
    {
        Log.Information("Using Cloudinary storage provider");
        builder.Services.AddSingleton<WebsiteBuilderAPI.Services.Storage.IStorageService, WebsiteBuilderAPI.Services.Storage.CloudinaryStorageService>();
    }
    else
    {
        Log.Information("Using Local storage provider");
        builder.Services.AddSingleton<WebsiteBuilderAPI.Services.Storage.IStorageService, WebsiteBuilderAPI.Services.Storage.LocalStorageService>();
    }
    builder.Services.AddScoped<IShippingService, ShippingService>();
    builder.Services.AddScoped<ILocationService, LocationService>();
    builder.Services.AddScoped<INotificationSettingsService, NotificationSettingsService>();
    builder.Services.AddScoped<ICustomerService, CustomerService>();
    builder.Services.AddScoped<ICollectionService, CollectionService>();
    builder.Services.AddScoped<IProductService, ProductService>();
    builder.Services.AddScoped<INewsletterSubscriberService, NewsletterSubscriberService>();
    builder.Services.AddScoped<IPaginasService, PaginasService>();
    builder.Services.AddScoped<IPolicyService, PolicyService>();
    builder.Services.AddScoped<INavigationMenuService, NavigationMenuService>();
    builder.Services.AddScoped<IRoomService, RoomService>();
    builder.Services.AddScoped<IReservationService, ReservationService>();
    builder.Services.AddScoped<IAvailabilityService, AvailabilityService>();
    builder.Services.AddScoped<IOrderService, OrderService>();
    builder.Services.AddScoped<IDomainService, DomainService>();
    builder.Services.AddScoped<IGlobalThemeConfigService, GlobalThemeConfigService>();
    builder.Services.AddScoped<IWebsiteBuilderService, WebsiteBuilderService>();
    builder.Services.AddScoped<IWebsiteBuilderCacheService, WebsiteBuilderCacheService>();
    
    // Register Structural Components services (Phase 2)
    builder.Services.AddScoped<IStructuralComponentsService, StructuralComponentsService>();
    builder.Services.AddScoped<IEditorHistoryService, EditorHistoryService>();

    // Registrar repositorios
    builder.Services.AddScoped<IPaymentProviderRepository, PaymentProviderRepository>();

    // Registrar servicios de encriptación
    builder.Services.AddScoped<IEncryptionService, EncryptionService>();
    builder.Services.AddScoped<IPaymentProviderService, PaymentProviderService>();
    
    // Registrar servicio de Azul Payment
    builder.Services.AddScoped<IAzulPaymentService, AzulPaymentService>();
    
    // Registrar servicio de Reviews
    builder.Services.AddScoped<IReviewService, ReviewService>();
    builder.Services.AddScoped<HostService>();
    builder.Services.AddScoped<IConfigOptionService, ConfigOptionService>();
    
    // Registrar servicio de Email
    builder.Services.AddScoped<IEmailService, EmailService>();
    
    
    // Registrar servicio de WhatsApp Twilio Integration (mantener compatibilidad)
    builder.Services.AddScoped<ITwilioWhatsAppService, TwilioWhatsAppService>();
    
    // Registrar nuevos servicios de WhatsApp con Factory Pattern
    builder.Services.AddWhatsAppServices(builder.Configuration);
    
    // Green API polling service not needed - using webhooks

    // Add HttpContextAccessor
    builder.Services.AddHttpContextAccessor();

    var app = builder.Build();

    // Honor X-Forwarded-* headers when running behind Nginx/Reverse proxy
    var forwardedHeadersOptions = new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    };
    // Trust all proxies/networks (explicitly cleared); rely on external firewalling
    forwardedHeadersOptions.KnownNetworks.Clear();
    forwardedHeadersOptions.KnownProxies.Clear();
    app.UseForwardedHeaders(forwardedHeadersOptions);

    // Agregar Serilog Request Logging Middleware
    app.UseSerilogRequestLogging(options =>
    {
        // Personalizar el mensaje de log
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        
        // Filtrar logs de health checks y archivos estáticos
        options.GetLevel = (httpContext, elapsed, ex) => 
        {
            if (ex != null || httpContext.Response.StatusCode >= 500)
                return LogEventLevel.Error;
            
            if (httpContext.Response.StatusCode >= 400)
                return LogEventLevel.Warning;
            
            if (httpContext.Request.Path.StartsWithSegments("/health"))
                return LogEventLevel.Verbose;
            
            return LogEventLevel.Information;
        };
        
        // Enriquecer con información adicional
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
            diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString());
            
            if (httpContext.User?.Identity?.IsAuthenticated == true)
            {
                diagnosticContext.Set("UserName", httpContext.User.Identity.Name);
                diagnosticContext.Set("UserId", httpContext.User.FindFirst("userId")?.Value);
            }
        };
    });

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        Log.Information("Running in Development mode - Swagger enabled");
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    else
    {
        Log.Information("Running in Production mode");
    }

    // Middleware personalizado para capturar excepciones
    app.Use(async (context, next) =>
    {
        try
        {
            await next();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unhandled exception occurred while processing {RequestPath}", context.Request.Path);
            
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            
            var response = new
            {
                error = "Internal Server Error",
                message = app.Environment.IsDevelopment() ? ex.Message : "An error occurred processing your request",
                traceId = Activity.Current?.Id ?? context.TraceIdentifier
            };
            
            await context.Response.WriteAsJsonAsync(response);
        }
    });

    // Preflight rápido para cualquier ruta (previene bloqueos por proxies)
    app.Use(async (context, next) =>
    {
        var origin = context.Request.Headers["Origin"].ToString();
        if (!string.IsNullOrEmpty(origin))
        {
            context.Response.Headers["Access-Control-Allow-Origin"] = origin;
            context.Response.Headers["Vary"] = "Origin";
            context.Response.Headers["Access-Control-Allow-Credentials"] = "true";
            context.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type, Authorization, X-Requested-With, X-CSRF-Token";
            context.Response.Headers["Access-Control-Allow-Methods"] = "GET,POST,PUT,DELETE,OPTIONS";
        }
        if (string.Equals(context.Request.Method, "OPTIONS", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = StatusCodes.Status204NoContent;
            await context.Response.CompleteAsync();
            return;
        }
        await next();
    });

    // CORS debe ir ANTES de cualquier otro middleware que pueda cortocircuitar la cadena
    // En producción, usar explícitamente la política de Vercel/Next.js
    app.UseCors("AllowNextJsApp");

    app.UseHttpsRedirection();

    // Servir archivos estáticos desde wwwroot
    app.UseStaticFiles();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    // Health check endpoint
    app.MapGet("/health", () =>
    {
        Log.Debug("Health check requested");
        return Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    });

    // Render health check endpoint (Render uses /healthz)
    app.MapGet("/healthz", () =>
    {
        Log.Debug("Render health check requested");
        return Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    });

    // Database health endpoint (Development use)
    app.MapGet("/db/health", async (ApplicationDbContext db) =>
    {
        try
        {
            var canConnect = await db.Database.CanConnectAsync();
            var provider = db.Database.ProviderName;
            var pendingMigrations = await db.Database.GetPendingMigrationsAsync();
            var conn = db.Database.GetDbConnection();
            return Results.Ok(new
            {
                canConnect,
                provider,
                database = conn.Database,
                dataSource = conn.DataSource,
                pendingMigrations = pendingMigrations.ToArray()
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "DB health check failed");
            return Results.Problem("DB health check failed: " + ex.Message);
        }
    });

    // List tables (Development use)
    app.MapGet("/db/tables", async (ApplicationDbContext db) =>
    {
        try
        {
            var sql = @"SELECT table_schema, table_name
                        FROM information_schema.tables
                        WHERE table_type = 'BASE TABLE'
                          AND table_schema NOT IN ('pg_catalog','information_schema')
                        ORDER BY table_schema, table_name";
            using var cmd = db.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = sql;
            if (cmd.Connection.State != System.Data.ConnectionState.Open)
                await cmd.Connection.OpenAsync();
            var list = new List<object>();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new { schema = reader.GetString(0), name = reader.GetString(1) });
            }
            return Results.Ok(list);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "DB tables listing failed");
            return Results.Problem("DB tables listing failed: " + ex.Message);
        }
    });

    // Database connection diagnostics
    try
    {
        var cs = app.Configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(cs))
        {
            var csBuilder = new Npgsql.NpgsqlConnectionStringBuilder(cs);
            Log.Information("DB Config - Host: {Host}, Port: {Port}, Database: {Database}, Username: {Username}", 
                csBuilder.Host, csBuilder.Port, csBuilder.Database, csBuilder.Username);
            
            // Test direct connection
            try
            {
                using var conn = new Npgsql.NpgsqlConnection(cs);
                await conn.OpenAsync();
                using var cmd = new Npgsql.NpgsqlCommand("SELECT 1", conn);
                var result = await cmd.ExecuteScalarAsync();
                Log.Information("Direct PG connection SUCCESS - SELECT 1 returned: {Result}", result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Direct PG connection FAILED: {Message}", ex.Message);
                if (ex.InnerException != null)
                {
                    Log.Error("Inner exception: {InnerMessage}", ex.InnerException.Message);
                }
            }
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to test database connection");
    }

            // Seed data
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
            Log.Information("Initializing database with EF Core");
            var defaultConn = app.Configuration.GetConnectionString("DefaultConnection");
            var migrationsConn = app.Configuration["MIGRATIONS__DefaultConnection"];
            
            // Test EF Core connection
            var context = services.GetRequiredService<ApplicationDbContext>();
            var canConnect = await context.Database.CanConnectAsync();
            Log.Information("EF Core CanConnect = {CanConnect}", canConnect);

            // Prefer dedicated migrations connection (e.g., direct 5432) when available
            if (!string.IsNullOrWhiteSpace(migrationsConn))
            {
                var dsb = new NpgsqlDataSourceBuilder(migrationsConn);
                dsb.EnableDynamicJson();
                using var migDataSource = dsb.Build();
                var migOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseNpgsql(migDataSource)
                    .Options;
                using var migContext = new ApplicationDbContext(migOptions);
                migContext.Database.Migrate();
            }
            else
            {
                // Fallback: use the default registered context (may be behind a pooler)
                var dbContext = services.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.Migrate();
            }

                    // SeedData.Initialize está comentado - el sistema ya tiene datos iniciales
                    // await SeedData.Initialize(services);
                    Log.Information("Database initialization completed successfully");

                    // Ensure WhatsApp tables have required columns and sizes for widget/email workflow
                    try
                    {
                        var conn = context.Database.GetDbConnection();
                        await conn.OpenAsync();
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = @"
DO $$
BEGIN
  -- Conversations: add columns if missing
  IF NOT EXISTS (
    SELECT 1 FROM information_schema.columns 
    WHERE table_name='WhatsAppConversations' AND column_name='CustomerEmail'
  ) THEN
    ALTER TABLE public.""WhatsAppConversations"" ADD COLUMN ""CustomerEmail"" varchar(255);
  END IF;
  IF NOT EXISTS (
    SELECT 1 FROM information_schema.columns 
    WHERE table_name='WhatsAppConversations' AND column_name='Source'
  ) THEN
    ALTER TABLE public.""WhatsAppConversations"" ADD COLUMN ""Source"" varchar(20) NOT NULL DEFAULT 'whatsapp';
  END IF;
  IF NOT EXISTS (
    SELECT 1 FROM information_schema.columns 
    WHERE table_name='WhatsAppConversations' AND column_name='SessionId'
  ) THEN
    ALTER TABLE public.""WhatsAppConversations"" ADD COLUMN ""SessionId"" varchar(100);
  END IF;

  -- Messages: widen From/To and add columns if missing
  -- Widen types only if current length is smaller than 255
  IF EXISTS (
    SELECT 1 FROM information_schema.columns 
    WHERE table_name='WhatsAppMessages' AND column_name='From' AND character_maximum_length IS NOT NULL AND character_maximum_length < 255
  ) THEN
    ALTER TABLE public.""WhatsAppMessages"" ALTER COLUMN ""From"" TYPE varchar(255);
  END IF;
  IF EXISTS (
    SELECT 1 FROM information_schema.columns 
    WHERE table_name='WhatsAppMessages' AND column_name='To' AND character_maximum_length IS NOT NULL AND character_maximum_length < 255
  ) THEN
    ALTER TABLE public.""WhatsAppMessages"" ALTER COLUMN ""To"" TYPE varchar(255);
  END IF;
  IF NOT EXISTS (
    SELECT 1 FROM information_schema.columns 
    WHERE table_name='WhatsAppMessages' AND column_name='Source'
  ) THEN
    ALTER TABLE public.""WhatsAppMessages"" ADD COLUMN ""Source"" varchar(20);
  END IF;
  IF NOT EXISTS (
    SELECT 1 FROM information_schema.columns 
    WHERE table_name='WhatsAppMessages' AND column_name='SessionId'
  ) THEN
    ALTER TABLE public.""WhatsAppMessages"" ADD COLUMN ""SessionId"" varchar(100);
  END IF;

  -- Helpful indexes
  CREATE INDEX IF NOT EXISTS ""IX_WhatsAppMessages_Source"" ON public.""WhatsAppMessages""(""Source"");
  CREATE INDEX IF NOT EXISTS ""IX_WhatsAppMessages_SessionId"" ON public.""WhatsAppMessages""(""SessionId"");
END $$;";
                            await cmd.ExecuteNonQueryAsync();
                        }
                        Log.Information("Ensured WhatsApp schema columns and sizes are up to date");
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Failed to apply WhatsApp schema ensure DDL (non-fatal)");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "An error occurred while initializing the database");
                }
    }

    Log.Information("🎯 WebsiteBuilder API started successfully on {Urls}", string.Join(", ", builder.WebHost.GetSetting("urls")?.Split(';') ?? ["http://localhost:5000"]));
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "💥 Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
