# 📝 Serilog - Sistema de Logging Avanzado

## 🚀 Instalación Completada

Se ha configurado Serilog en tu proyecto ASP.NET Core con las siguientes características:

### ✅ Paquetes Instalados
- **Serilog.AspNetCore** - Integración principal con ASP.NET Core
- **Serilog.Sinks.File** - Escritura de logs en archivos
- **Serilog.Sinks.Console** - Logs en consola con colores
- **Serilog.Enrichers.Environment** - Información del entorno
- **Serilog.Enrichers.Thread** - Información de threads
- **Serilog.Settings.Configuration** - Configuración desde appsettings.json

## 📁 Estructura de Logs

Los logs se guardan en la siguiente estructura:

```
WebsiteBuilderAPI/
├── logs/
│   ├── websitebuilder-20241210.log     # Log general diario
│   ├── websitebuilder-20241211.log     # Rotación automática
│   └── errors/
│       ├── error-20241210.log          # Solo errores (500+)
│       └── error-20241211.log          # Rotación automática
└── logs/debug/                          # Solo en desarrollo
    └── debug-20241210.log              # Logs detallados de debug
```

## 🎯 Niveles de Logging

### En Desarrollo (Development)
- **Debug**: Información detallada para debugging
- **Information**: Eventos importantes del flujo normal
- **Warning**: Situaciones anormales pero manejables
- **Error**: Errores que necesitan atención
- **Fatal**: Errores críticos que causan terminación

### En Producción
- **Information** y superiores (menos verbose)
- Archivos separados para errores críticos

## 💻 Cómo Ejecutar y Ver Logs

### 1. Restaurar Paquetes NuGet
Primero, restaura los paquetes desde PowerShell o CMD:

```powershell
# Desde la carpeta del proyecto
cd "C:\Users\hp\Documents\Visual Studio 2022\Projects\WebsiteBuilderAPI"

# Restaurar paquetes
dotnet restore
```

### 2. Ejecutar la Aplicación con Logs en Tiempo Real

#### Opción A: Desde PowerShell (Recomendado)
```powershell
# Ejecutar en modo Development para ver logs detallados
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run

# O especificar el entorno directamente
dotnet run --environment Development
```

#### Opción B: Desde Visual Studio
1. Presiona **F5** o click en **▶ IIS Express**
2. Los logs aparecerán en la ventana **Output** → **Show output from: ASP.NET Core Web Server**

### 3. Ver Logs en Tiempo Real en PowerShell

Para ver logs con colores y formato mejorado:

```powershell
# Ejecutar y ver logs con colores
dotnet run | Out-Host

# O para filtrar solo errores
dotnet run | Select-String "ERR"

# Ver solo warnings y errores
dotnet run | Select-String "WRN|ERR|FTL"
```

### 4. Monitorear Archivos de Log en Tiempo Real

#### En PowerShell:
```powershell
# Ver el log principal en tiempo real
Get-Content "logs\websitebuilder-$(Get-Date -Format 'yyyyMMdd').log" -Wait -Tail 50

# Ver solo errores en tiempo real
Get-Content "logs\errors\error-$(Get-Date -Format 'yyyyMMdd').log" -Wait

# Ver últimas 100 líneas del log
Get-Content "logs\websitebuilder-$(Get-Date -Format 'yyyyMMdd').log" -Tail 100
```

#### En Linux/WSL:
```bash
# Si estás en WSL
tail -f logs/websitebuilder-$(date +%Y%m%d).log

# Ver con colores
tail -f logs/websitebuilder-$(date +%Y%m%d).log | grep --color=auto -E "ERR|WRN|"
```

## 🔍 Qué Se Registra Automáticamente

### ✅ Información Capturada Automáticamente:
1. **Todas las peticiones HTTP**:
   - Método (GET, POST, etc.)
   - URL solicitada
   - Código de respuesta
   - Tiempo de respuesta
   - IP del cliente
   - User-Agent

2. **Errores 500**:
   - Stack trace completo
   - Mensaje de excepción
   - Contexto de la petición
   - Usuario autenticado (si aplica)

3. **Problemas de Autenticación**:
   - Intentos de login fallidos
   - Tokens expirados
   - Accesos no autorizados

4. **Operaciones de Base de Datos** (en Development):
   - Queries SQL ejecutadas
   - Tiempos de ejecución
   - Errores de conexión

## 📊 Ejemplos de Logs

### Formato en Consola:
```
[14:23:45 INF] 🚀 Starting WebsiteBuilder API Application
[14:23:46 INF] Configuring PostgreSQL connection
[14:23:46 INF] Configuring JWT Authentication with Issuer: WebsiteBuilderAPI
[14:23:47 INF] HTTP GET /api/products responded 200 in 45.3210 ms
[14:23:48 WRN] Failed login attempt for email: user@test.com - Invalid credentials
[14:23:49 ERR] Error during login attempt for email: user@test.com
System.NullReferenceException: Object reference not set to an instance...
```

### Formato en Archivo:
```
2024-12-10 14:23:45.123 -04:00 [INF] [WebsiteBuilderAPI.Program] Starting WebsiteBuilder API Application
2024-12-10 14:23:46.456 -04:00 [INF] [WebsiteBuilderAPI.Controllers.AuthController] Successful login for user: admin@test.com with UserId: 1
2024-12-10 14:23:47.789 -04:00 [ERR] [WebsiteBuilderAPI.Controllers.OrdersController] Error creating order
System.InvalidOperationException: Cannot create order without items
   at WebsiteBuilderAPI.Services.OrderService.CreateAsync(Int32 companyId, CreateOrderDto dto)
```

## 🛠️ Uso en Controllers

### Ejemplo Básico:
```csharp
public class ProductsController : ControllerBase
{
    private readonly ILogger<ProductsController> _logger;
    
    public ProductsController(ILogger<ProductsController> logger)
    {
        _logger = logger;
    }
    
    [HttpPost]
    public async Task<IActionResult> Create(ProductDto dto)
    {
        try
        {
            _logger.LogInformation("Creating new product: {ProductName}", dto.Name);
            
            // Tu lógica aquí
            
            _logger.LogInformation("Product created successfully with ID: {ProductId}", product.Id);
            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product: {ProductName}", dto.Name);
            return StatusCode(500);
        }
    }
}
```

### Mejores Prácticas:
1. **Usa parámetros estructurados** en lugar de string interpolation:
   ```csharp
   // ✅ CORRECTO - Estructurado
   _logger.LogInformation("User {UserId} logged in at {Time}", userId, DateTime.Now);
   
   // ❌ INCORRECTO - String interpolation
   _logger.LogInformation($"User {userId} logged in at {DateTime.Now}");
   ```

2. **Niveles apropiados**:
   - `LogDebug` - Información de debugging
   - `LogInformation` - Eventos importantes exitosos
   - `LogWarning` - Situaciones inusuales pero manejables
   - `LogError` - Errores que necesitan atención
   - `LogCritical` - Fallas del sistema

3. **Incluye contexto relevante**:
   ```csharp
   using (_logger.BeginScope("Processing Order {OrderId}", orderId))
   {
       _logger.LogInformation("Starting order processing");
       // Todo el logging aquí incluirá el OrderId automáticamente
   }
   ```

## 🔧 Configuración Adicional

### Cambiar Niveles de Log Dinámicamente

En `appsettings.Development.json`, puedes ajustar los niveles:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",        // Cambia a "Information" para menos logs
      "Override": {
        "Microsoft.EntityFrameworkCore": "Warning",  // Oculta queries SQL
        "System": "Warning"                          // Reduce logs del sistema
      }
    }
  }
}
```

### Filtrar Logs Específicos

Para ver solo logs de tu aplicación (sin logs del framework):
```powershell
dotnet run | Select-String "WebsiteBuilderAPI"
```

## 📈 Análisis de Logs

### Buscar Errores Específicos:
```powershell
# Buscar todos los errores 500
Select-String -Path "logs\*.log" -Pattern "responded 500"

# Buscar excepciones específicas
Select-String -Path "logs\errors\*.log" -Pattern "NullReferenceException"

# Contar errores por tipo
Get-Content "logs\errors\*.log" | Select-String "\[ERR\]" | Measure-Object
```

### Estadísticas de Rendimiento:
```powershell
# Ver requests más lentas
Select-String -Path "logs\*.log" -Pattern "responded .* in (\d{4,})" | Sort-Object

# Promedio de tiempo de respuesta
$times = Select-String -Path "logs\*.log" -Pattern "in (\d+\.\d+) ms" | 
         ForEach-Object { [double]($_.Matches[0].Groups[1].Value) }
$times | Measure-Object -Average -Maximum -Minimum
```

## 🚨 Troubleshooting

### Si no ves logs en consola:
1. Verifica que `ASPNETCORE_ENVIRONMENT` esté en "Development"
2. Restaura los paquetes: `dotnet restore`
3. Limpia y reconstruye: `dotnet clean && dotnet build`

### Si los archivos de log no se crean:
1. Verifica permisos de escritura en la carpeta del proyecto
2. La carpeta `logs/` se crea automáticamente al primer log
3. Revisa que no haya errores de sintaxis en `appsettings.json`

### Para logs más detallados:
Cambia temporalmente el nivel a "Verbose" en `Program.cs`:
```csharp
.MinimumLevel.Verbose()  // Máximo detalle
```

## 📌 Comandos Rápidos de Referencia

```powershell
# Ejecutar con logs
dotnet run --environment Development

# Ver log de hoy
Get-Content "logs\websitebuilder-$(Get-Date -Format 'yyyyMMdd').log" -Tail 50

# Ver errores de hoy
Get-Content "logs\errors\error-$(Get-Date -Format 'yyyyMMdd').log"

# Buscar un error específico
Select-String -Path "logs\*.log" -Pattern "500|Error|Exception"

# Limpiar logs antiguos (mantener últimos 7 días)
Get-ChildItem "logs" -Recurse -File | Where-Object { $_.LastWriteTime -lt (Get-Date).AddDays(-7) } | Remove-Item
```

## ✅ Validación de la Instalación

Para verificar que Serilog está funcionando:

1. Ejecuta la aplicación
2. Deberías ver en consola:
   - `🚀 Starting WebsiteBuilder API Application`
   - Logs con colores y formato estructurado
3. Verifica que se creó la carpeta `logs/`
4. Intenta un login fallido y verifica que se registra el warning

---

**Serilog está completamente configurado y listo para capturar todos los errores y eventos de tu aplicación** 🎉