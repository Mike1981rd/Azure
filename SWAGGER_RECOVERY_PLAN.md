# PLAN DE RECUPERACIÓN - Error Swagger después de implementar Clientes

## 🔴 DIAGNÓSTICO
El sistema funcionaba correctamente hasta que se implementó el módulo de Clientes. El error de Swagger apareció después de:
1. Crear los modelos Customer y relacionados
2. Agregar CustomersController
3. Registrar CustomerService en Program.cs
4. Conectar la ruta en el panel lateral

## ✅ ACCIONES REALIZADAS PARA AISLAR EL PROBLEMA (ACTUALIZADO)

### 1. Deshabilitación temporal del módulo Customers:
```bash
# Renombrado el controller
mv Controllers/CustomersController.cs Controllers/CustomersController.cs.disabled

# Renombrado el servicio y su interfaz
mv Services/CustomerService.cs Services/CustomerService.cs.disabled
mv Services/ICustomerService.cs Services/ICustomerService.cs.disabled
```

### 2. Comentado en Program.cs:
```csharp
// builder.Services.AddScoped<ICustomerService, CustomerService>();
```

### 3. Comentado en ApplicationDbContext.cs:
```csharp
// public DbSet<Customer> Customers { get; set; }
// public DbSet<CustomerAddress> CustomerAddresses { get; set; }
// public DbSet<CustomerPaymentMethod> CustomerPaymentMethods { get; set; }
// public DbSet<CustomerNotificationPreference> CustomerNotificationPreferences { get; set; }
// public DbSet<CustomerDevice> CustomerDevices { get; set; }
// public DbSet<CustomerWishlistItem> CustomerWishlistItems { get; set; }
// public DbSet<CustomerCoupon> CustomerCoupons { get; set; }
```

### 4. Restaurado el servicio de encriptación original:
```bash
mv Services/EncryptionService.cs.old Services/EncryptionService.cs
```

### 5. Revertido AzulPaymentService a su estado original (sin using adicional)

## 🔧 PASOS PARA VERIFICAR Y SOLUCIONAR

### Paso 1: Verificar que Swagger funciona sin el módulo Customer
1. Reconstruir el proyecto en Visual Studio
2. Ejecutar la aplicación
3. Navegar a `/swagger`
4. Si funciona, el problema está aislado al módulo Customer

### Paso 2: Si Swagger funciona, reactivar gradualmente:

#### A. Primero, solo los modelos (sin relaciones):
1. Descomentar SOLO los DbSets en ApplicationDbContext
2. NO descomentar las configuraciones en OnModelCreating
3. Probar Swagger

#### B. Si funciona, agregar el servicio:
1. Descomentar `builder.Services.AddScoped<ICustomerService, CustomerService>();`
2. Probar Swagger

#### C. Si funciona, agregar el controller:
1. Renombrar `CustomersController.cs.disabled` a `CustomersController.cs`
2. Probar Swagger

### Paso 3: Si algún paso falla, el problema está ahí

## 🎯 SOLUCIÓN PROPUESTA

### Opción 1: Simplificar los modelos
- Eliminar todas las relaciones circulares
- Usar DTOs simples sin navegación
- No exponer entidades directamente

### Opción 2: Configurar Swagger para ignorar problemas
```csharp
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
    c.CustomSchemaIds(type => type.FullName);
});
```

### Opción 3: Usar Newtonsoft.Json en lugar de System.Text.Json
```csharp
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
    });
```

## 📄 CONFIGURACIONES A MANTENER EN OnModelCreating

Si necesitas las configuraciones de Customer, úsalas SIN las relaciones de navegación:

```csharp
// Comentar temporalmente todas las configuraciones de Customer:
// modelBuilder.Entity<Customer>(...)
// modelBuilder.Entity<CustomerAddress>(...)
// etc.
```

## ⚠️ IMPORTANTE
- NO hacer migraciones hasta que Swagger funcione
- Probar cada cambio individualmente
- Documentar qué cambio especifica causa el error

## 🔄 PARA REVERTIR TODO
```bash
# Restaurar CustomersController
mv Controllers/CustomersController.cs.disabled Controllers/CustomersController.cs

# Descomentar en Program.cs y ApplicationDbContext.cs
# Luego aplicar las soluciones propuestas arriba
```