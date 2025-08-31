# 📋 PLAN DE IMPLEMENTACIÓN - MÓDULO CLIENTES (CUSTOMERS)

## 📌 INFORMACIÓN GENERAL
- **Fecha**: 2025-08-08
- **Módulo**: Clientes/Customers
- **Tipo**: CRUD completo con gestión avanzada
- **Prioridad**: Alta (Fase 2 del proyecto)
- **Tiempo estimado**: 6-8 horas

## 🎯 OBJETIVOS
1. Implementar sistema completo de gestión de clientes externos (compradores/huéspedes)
2. Separar claramente de Users (usuarios internos del sistema)
3. Mantener diseño idéntico a las UI proporcionadas
4. Seguir todas las reglas de CLAUDE.md para implementación

## ⚠️ REGLAS CRÍTICAS A SEGUIR (CLAUDE.md + Guardado.md)

### 1. SEPARACIÓN DE CONCEPTOS
- **Customer** ≠ **User**
- Customer = Cliente externo que compra/reserva
- User = Usuario interno del sistema (admin, empleado)
- NO mezclar lógica de ambos modelos

### 2. UI OBLIGATORIA - CHECKLIST
- ✅ Traducciones i18n (ES/EN)
- ✅ Color primario dinámico
- ✅ Dark mode completo
- ✅ Responsividad móvil (mobile-first)
- ✅ Botones con estados (loading/disabled)
- ✅ Breadcrumbs desktop / título móvil
- ✅ Focus states con color primario
- ✅ Selector de país con banderas

### 3. ARQUITECTURA
- Controllers máximo 300 líneas
- Services con interfaces separadas
- DTOs para cada operación
- Sin Repository pattern (usar Service directamente)

### 4. TROUBLESHOOTING CRUD (CRÍTICO - de Guardado.md)
- **CompanyId SIEMPRE en minúscula** con fallback a 1
- **EnableDynamicJson() OBLIGATORIO** para JSONB en Program.cs
- **Puerto API 5266** (NUNCA 3000 ni 7224 en dev)
- **SIEMPRE refetch después de operaciones CRUD**
- **Manejar strings vacíos explícitamente** en updates parciales
- **Implementar debouncing para auto-save** (1 segundo)
- **Separar endpoints para logo/avatar de datos generales**

### 5. PUNTOS CRÍTICOS DE GUARDADO.md
- ✅ **Token Claims**: Usar `User.FindFirst("companyId")` en minúscula
- ✅ **Error Handling**: Siempre loggear detalles en console
- ✅ **Validation**: Manejar errores 400 con detalles específicos
- ✅ **Loading States**: Implementar estados loading/saving separados
- ✅ **Token Expiration**: Redirigir a login en 401
- ✅ **CORS**: Configurar para localhost:3000 en dev
- ✅ **Partial Updates**: Solo actualizar campos no-null del DTO

## 📊 ANÁLISIS DE DATOS - MODELOS REQUERIDOS

### 1️⃣ **Modelo Principal: Customer**
```csharp
public class Customer
{
    // Identificación
    public int Id { get; set; }
    public string CustomerId { get; set; }  // Formato: #895280
    public int CompanyId { get; set; }
    
    // Información personal
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string? Avatar { get; set; }
    public string? Phone { get; set; }
    public string Country { get; set; }
    public string Status { get; set; }  // Active/Inactive/Pending
    
    // Métricas financieras
    public decimal AccountBalance { get; set; }
    public decimal TotalSpent { get; set; }
    public int TotalOrders { get; set; }
    
    // Programa de lealtad
    public int LoyaltyPoints { get; set; }
    public string LoyaltyTier { get; set; }  // Platinum/Gold/Silver
    
    // Seguridad
    public bool TwoFactorEnabled { get; set; }
    public string? TwoFactorPhone { get; set; }
    public string? TwoFactorSecret { get; set; }
    
    // Auditoría
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime? DeletedAt { get; set; }  // Soft delete
    
    // Navegación
    public Company Company { get; set; }
    public List<CustomerAddress> Addresses { get; set; }
    public List<CustomerPaymentMethod> PaymentMethods { get; set; }
    public List<CustomerNotificationPreference> NotificationPreferences { get; set; }
    public List<CustomerDevice> Devices { get; set; }
    public List<CustomerWishlistItem> WishlistItems { get; set; }
    public List<CustomerCoupon> Coupons { get; set; }
}
```

### 2️⃣ **Modelos Relacionados**

#### CustomerAddress
```csharp
public class CustomerAddress
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string Type { get; set; }  // Home/Office/Family/Other
    public string Label { get; set; }  // "23 Shatinon Mekalan", "45 Roker Terrace"
    public string Street { get; set; }
    public string? Apartment { get; set; }
    public string City { get; set; }
    public string? State { get; set; }
    public string Country { get; set; }
    public string? PostalCode { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
    public Customer Customer { get; set; }
}
```

#### CustomerPaymentMethod
```csharp
public class CustomerPaymentMethod
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CardType { get; set; }  // Mastercard/Visa/AmericanExpress
    public string CardholderName { get; set; }
    public string Last4Digits { get; set; }
    public string ExpiryMonth { get; set; }
    public string ExpiryYear { get; set; }
    public string? BillingAddress { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime CreatedAt { get; set; }
    public Customer Customer { get; set; }
}
```

#### CustomerNotificationPreference
```csharp
public class CustomerNotificationPreference
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string NotificationType { get; set; }  // NewForYou/AccountActivity/BrowserLogin/DeviceLinked
    public string DisplayName { get; set; }
    public bool EmailEnabled { get; set; }
    public bool BrowserEnabled { get; set; }
    public bool AppEnabled { get; set; }
    public Customer Customer { get; set; }
}
```

#### CustomerDevice
```csharp
public class CustomerDevice
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string Browser { get; set; }  // Chrome/Firefox/Safari/Edge
    public string DeviceType { get; set; }  // Desktop/Mobile/Tablet
    public string DeviceName { get; set; }  // "HP Spectre 360", "iPhone 12x"
    public string OperatingSystem { get; set; }  // Windows/macOS/iOS/Android
    public string IpAddress { get; set; }
    public string Location { get; set; }  // "Switzerland", "Australia"
    public DateTime LastActivity { get; set; }
    public bool IsTrusted { get; set; }
    public Customer Customer { get; set; }
}
```

#### CustomerWishlistItem
```csharp
public class CustomerWishlistItem
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int ProductId { get; set; }
    public DateTime AddedAt { get; set; }
    public Customer Customer { get; set; }
    public Product Product { get; set; }
}
```

#### CustomerCoupon
```csharp
public class CustomerCoupon
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
    public decimal DiscountAmount { get; set; }
    public string DiscountType { get; set; }  // Percentage/Fixed
    public DateTime ValidFrom { get; set; }
    public DateTime ValidUntil { get; set; }
    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }
    public Customer Customer { get; set; }
}
```

## 🔧 IMPLEMENTACIÓN BACKEND

### 1. DTOs Requeridos

#### Request DTOs
```csharp
// DTOs/Customers/CreateCustomerDto.cs
public class CreateCustomerDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string? Phone { get; set; }
    public string Country { get; set; }
    public string Status { get; set; } = "Active";
}

// DTOs/Customers/UpdateCustomerDto.cs
public class UpdateCustomerDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Country { get; set; }
    public string? Status { get; set; }
    public string? Avatar { get; set; }
}

// DTOs/Customers/ChangePasswordDto.cs
public class ChangePasswordDto
{
    public string NewPassword { get; set; }
    public string ConfirmPassword { get; set; }
}

// DTOs/Customers/AddAddressDto.cs
public class AddAddressDto
{
    public string Type { get; set; }
    public string Label { get; set; }
    public string Street { get; set; }
    public string? Apartment { get; set; }
    public string City { get; set; }
    public string? State { get; set; }
    public string Country { get; set; }
    public string? PostalCode { get; set; }
    public bool IsDefault { get; set; }
}

// DTOs/Customers/AddPaymentMethodDto.cs
public class AddPaymentMethodDto
{
    public string CardType { get; set; }
    public string CardholderName { get; set; }
    public string CardNumber { get; set; }  // Se guardará solo last4
    public string ExpiryMonth { get; set; }
    public string ExpiryYear { get; set; }
    public string? BillingAddress { get; set; }
    public bool IsPrimary { get; set; }
}

// DTOs/Customers/UpdateNotificationPreferencesDto.cs
public class UpdateNotificationPreferencesDto
{
    public List<NotificationPreferenceDto> Preferences { get; set; }
}

public class NotificationPreferenceDto
{
    public string NotificationType { get; set; }
    public bool EmailEnabled { get; set; }
    public bool BrowserEnabled { get; set; }
    public bool AppEnabled { get; set; }
}
```

#### Response DTOs
```csharp
// DTOs/Customers/CustomerDto.cs
public class CustomerDto
{
    public int Id { get; set; }
    public string CustomerId { get; set; }
    public string FullName { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string? Avatar { get; set; }
    public string? Phone { get; set; }
    public string Country { get; set; }
    public string Status { get; set; }
    public decimal AccountBalance { get; set; }
    public decimal TotalSpent { get; set; }
    public int TotalOrders { get; set; }
    public int LoyaltyPoints { get; set; }
    public string LoyaltyTier { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

// DTOs/Customers/CustomerDetailDto.cs
public class CustomerDetailDto : CustomerDto
{
    public List<CustomerAddressDto> Addresses { get; set; }
    public List<CustomerPaymentMethodDto> PaymentMethods { get; set; }
    public List<CustomerNotificationPreferenceDto> NotificationPreferences { get; set; }
    public List<CustomerDeviceDto> RecentDevices { get; set; }
    public int WishlistCount { get; set; }
    public int CouponsCount { get; set; }
}
```

### 2. Controller

```csharp
// Controllers/CustomersController.cs
[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    
    // GET: api/customers
    [HttpGet]
    public async Task<IActionResult> GetCustomers(
        [FromQuery] int page = 1, 
        [FromQuery] int size = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? status = "Active",
        [FromQuery] string? country = null)
    
    // GET: api/customers/{id}
    [HttpGet("{id}")]
    
    // POST: api/customers
    [HttpPost]
    
    // PUT: api/customers/{id}
    [HttpPut("{id}")]
    
    // DELETE: api/customers/{id}
    [HttpDelete("{id}")]
    
    // POST: api/customers/{id}/change-password
    [HttpPost("{id}/change-password")]
    
    // GET: api/customers/{id}/addresses
    [HttpGet("{id}/addresses")]
    
    // POST: api/customers/{id}/addresses
    [HttpPost("{id}/addresses")]
    
    // PUT: api/customers/{id}/addresses/{addressId}
    [HttpPut("{id}/addresses/{addressId}")]
    
    // DELETE: api/customers/{id}/addresses/{addressId}
    [HttpDelete("{id}/addresses/{addressId}")]
    
    // GET: api/customers/{id}/payment-methods
    [HttpGet("{id}/payment-methods")]
    
    // POST: api/customers/{id}/payment-methods
    [HttpPost("{id}/payment-methods")]
    
    // DELETE: api/customers/{id}/payment-methods/{methodId}
    [HttpDelete("{id}/payment-methods/{methodId}")]
    
    // GET: api/customers/{id}/notifications
    [HttpGet("{id}/notifications")]
    
    // PUT: api/customers/{id}/notifications
    [HttpPut("{id}/notifications")]
    
    // GET: api/customers/{id}/devices
    [HttpGet("{id}/devices")]
    
    // DELETE: api/customers/{id}/devices/{deviceId}
    [HttpDelete("{id}/devices/{deviceId}")]
    
    // POST: api/customers/{id}/two-factor/enable
    [HttpPost("{id}/two-factor/enable")]
    
    // POST: api/customers/{id}/two-factor/disable
    [HttpPost("{id}/two-factor/disable")]
    
    // GET: api/customers/{id}/orders
    [HttpGet("{id}/orders")]
    
    // GET: api/customers/{id}/wishlist
    [HttpGet("{id}/wishlist")]
    
    // GET: api/customers/{id}/coupons
    [HttpGet("{id}/coupons")]
}
```

### 3. Service

```csharp
// Services/ICustomerService.cs
public interface ICustomerService
{
    Task<PagedResult<CustomerDto>> GetCustomersAsync(int companyId, CustomerFilterDto filter);
    Task<CustomerDetailDto> GetCustomerByIdAsync(int companyId, int id);
    Task<CustomerDto> CreateCustomerAsync(int companyId, CreateCustomerDto dto);
    Task<CustomerDto> UpdateCustomerAsync(int companyId, int id, UpdateCustomerDto dto);
    Task DeleteCustomerAsync(int companyId, int id);
    Task ChangePasswordAsync(int id, ChangePasswordDto dto);
    Task<List<CustomerAddressDto>> GetAddressesAsync(int customerId);
    Task<CustomerAddressDto> AddAddressAsync(int customerId, AddAddressDto dto);
    Task<CustomerAddressDto> UpdateAddressAsync(int customerId, int addressId, AddAddressDto dto);
    Task DeleteAddressAsync(int customerId, int addressId);
    // ... más métodos
}

// Services/CustomerService.cs
public class CustomerService : ICustomerService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CustomerService> _logger;
    
    // Implementación con:
    // - Generación automática de CustomerId (#895280)
    // - Hash de contraseñas con BCrypt
    // - Soft delete
    // - Logging de operaciones
    // - Validaciones de negocio
}
```

## 🎨 IMPLEMENTACIÓN FRONTEND

### 1. ESTRUCTURA DE ARCHIVOS

```
websitebuilder-admin/src/
├── app/dashboard/clientes/
│   ├── page.tsx                    # Lista principal
│   ├── [id]/
│   │   ├── page.tsx                # Detalle con tabs
│   │   └── edit/
│   │       └── page.tsx            # Edición
│   └── new/
│       └── page.tsx                # Crear nuevo
├── components/clientes/
│   ├── CustomersList.tsx           # Tabla principal
│   ├── CustomerForm.tsx            # Formulario crear/editar
│   ├── CustomerDetailTabs.tsx      # Contenedor de tabs
│   ├── OverviewTab.tsx            # Tab Overview
│   ├── SecurityTab.tsx            # Tab Security  
│   ├── AddressBillingTab.tsx      # Tab Address & Billing
│   ├── NotificationsTab.tsx       # Tab Notifications
│   ├── CustomerMetrics.tsx        # Cards de métricas
│   ├── OrdersHistory.tsx          # Tabla de órdenes
│   ├── AddressBook.tsx            # Gestión direcciones
│   ├── PaymentMethods.tsx         # Gestión tarjetas
│   ├── DevicesList.tsx            # Lista dispositivos
│   ├── TwoFactorSettings.tsx      # Config 2FA
│   ├── NotificationMatrix.tsx     # Matriz notificaciones
│   └── CustomerAvatar.tsx         # Avatar con upload
├── hooks/
│   └── useCustomers.ts             # Hook para API
└── lib/
    └── api/customers.ts            # Llamadas API
```

### 2. COMPONENTES PRINCIPALES

#### Lista Principal (CustomersList.tsx)
```typescript
interface CustomersListProps {
  // Props con filtros, paginación, etc.
}

export function CustomersList({ }: CustomersListProps) {
  const { t } = useI18n();
  const [primaryColor, setPrimaryColor] = useState('#22c55e');
  const [filters, setFilters] = useState({
    search: '',
    status: 'Active',
    country: '',
    page: 1,
    size: 20
  });
  
  // Features:
  // - Tabla con avatar, ID, email, país con bandera
  // - Búsqueda en tiempo real
  // - Filtros por status y país
  // - Paginación
  // - Exportación (CSV, Excel, PDF)
  // - Botón "Add Customer"
  // - Responsive con cards en móvil
}
```

#### Tab Overview (OverviewTab.tsx)
```typescript
export function OverviewTab({ customer }: { customer: CustomerDetailDto }) {
  // Secciones:
  // 1. Info básica con avatar y "Edit Details"
  // 2. Cards de métricas:
  //    - Account Balance (con credit left)
  //    - Loyalty Program (con puntos y tier)
  //    - Wishlist (items count)
  //    - Coupons (disponibles)
  // 3. Orders History con tabla paginada
  // 4. Card "Upgrade to premium"
}
```

#### Tab Security (SecurityTab.tsx)
```typescript
export function SecurityTab({ customer }: { customer: CustomerDetailDto }) {
  // Secciones:
  // 1. Change Password (con validación)
  // 2. Two-factor authentication (toggle + SMS)
  // 3. Recent Devices (tabla con browser, device, location, activity)
  // 4. Opción de logout remoto de dispositivos
}
```

#### Tab Address & Billing (AddressBillingTab.tsx)
```typescript
export function AddressBillingTab({ customer }: { customer: CustomerDetailDto }) {
  // Secciones:
  // 1. Address Book:
  //    - Lista de direcciones con tipo e ícono
  //    - Default badge para principal
  //    - Botones edit/delete
  //    - "Add new address" modal
  // 2. Payment Methods:
  //    - Cards con logo de marca
  //    - Expiración y last 4 digits
  //    - Primary/Secondary badge
  //    - "Add payment method" modal
}
```

#### Tab Notifications (NotificationsTab.tsx)
```typescript
export function NotificationsTab({ customer }: { customer: CustomerDetailDto }) {
  // Matriz de checkboxes:
  // - Filas: Tipos de notificación
  // - Columnas: Email, Browser, App
  // - Botones "Save changes" y "Discard"
  // - Descripción de cada tipo
}
```

### 3. HOOKS Y UTILIDADES

```typescript
// hooks/useCustomers.ts
export function useCustomers() {
  const { data, loading, error, refetch } = useQuery('/api/customers');
  
  const createCustomer = async (data: CreateCustomerDto) => {
    // POST con refetch
  };
  
  const updateCustomer = async (id: number, data: UpdateCustomerDto) => {
    // PUT con refetch
  };
  
  const deleteCustomer = async (id: number) => {
    // DELETE con confirmación
  };
  
  return {
    customers: data,
    loading,
    error,
    createCustomer,
    updateCustomer,
    deleteCustomer,
    refetch
  };
}
```

## 🎨 DISEÑO UI/UX ESPECÍFICO

### 1. COLORES Y ESTILOS
```css
/* Badges de status */
.status-active { background: #22c55e20; color: #22c55e; }
.status-inactive { background: #ef444420; color: #ef4444; }
.status-pending { background: #f59e0b20; color: #f59e0b; }

/* Order status */
.status-delivered { background: #22c55e20; color: #22c55e; }
.status-ready-to-pickup { background: #3b82f620; color: #3b82f6; }
.status-dispatched { background: #f59e0b20; color: #f59e0b; }

/* Cards métricas */
.metric-card {
  border-radius: 12px;
  padding: 20px;
  background: white;
  dark:background: gray-800;
}

/* Avatar con iniciales */
.avatar-initials {
  width: 40px;
  height: 40px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, primaryColor, primaryColor80);
  color: white;
  font-weight: 600;
}
```

### 2. COMPONENTES REUTILIZABLES

```typescript
// components/ui/StatusBadge.tsx
export function StatusBadge({ status }: { status: string }) {
  const colors = {
    Active: 'bg-green-100 text-green-700 dark:bg-green-900/20',
    Inactive: 'bg-red-100 text-red-700 dark:bg-red-900/20',
    Pending: 'bg-yellow-100 text-yellow-700 dark:bg-yellow-900/20'
  };
  
  return (
    <span className={`px-2 py-1 rounded-full text-xs font-medium ${colors[status]}`}>
      {status}
    </span>
  );
}

// components/ui/MetricCard.tsx
export function MetricCard({ 
  icon, 
  title, 
  value, 
  subtitle, 
  color 
}: MetricCardProps) {
  return (
    <div className="bg-white dark:bg-gray-800 p-6 rounded-xl">
      <div className="flex items-start justify-between">
        <div className="flex-1">
          <div className="flex items-center gap-2 mb-2">
            <span style={{ color }}>{icon}</span>
            <h3 className="text-sm text-gray-600 dark:text-gray-400">{title}</h3>
          </div>
          <p className="text-2xl font-bold text-gray-900 dark:text-white">{value}</p>
          <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">{subtitle}</p>
        </div>
      </div>
    </div>
  );
}
```

### 3. RESPONSIVE DESIGN

```typescript
// Mobile: Cards apiladas
<div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
  {/* Metric cards */}
</div>

// Mobile: Tabla → Cards
<div className="hidden sm:block">
  {/* Desktop table */}
</div>
<div className="sm:hidden space-y-3">
  {/* Mobile cards */}
  {customers.map(customer => (
    <CustomerMobileCard key={customer.id} customer={customer} />
  ))}
</div>

// Mobile: Tabs horizontales con scroll
<div className="overflow-x-auto">
  <div className="flex space-x-1 border-b">
    {tabs.map(tab => (
      <TabButton key={tab.id} {...tab} />
    ))}
  </div>
</div>
```

## 📋 TAREAS DE IMPLEMENTACIÓN

### FASE 1: Backend (2-3 horas)
1. ✅ Crear modelos Customer y relacionados
2. ✅ Configurar relaciones en ApplicationDbContext
3. ✅ Crear DTOs (Request y Response)
4. ✅ Implementar CustomerService con lógica de negocio
5. ✅ Implementar CustomersController con todos los endpoints
6. ✅ Registrar servicio en Program.cs
7. ✅ Crear migración AddCustomerModels

### FASE 2: Frontend - Lista (1-2 horas)
1. ✅ Crear página /dashboard/clientes
2. ✅ Implementar CustomersList con tabla
3. ✅ Agregar búsqueda y filtros
4. ✅ Implementar paginación
5. ✅ Agregar exportación de datos
6. ✅ Diseño responsive con cards móvil

### FASE 3: Frontend - Detalle y Tabs (2-3 horas)
1. ✅ Crear página /dashboard/clientes/[id]
2. ✅ Implementar CustomerDetailTabs
3. ✅ Desarrollar OverviewTab con métricas
4. ✅ Desarrollar SecurityTab con 2FA
5. ✅ Desarrollar AddressBillingTab
6. ✅ Desarrollar NotificationsTab
7. ✅ Implementar todas las acciones CRUD

### FASE 4: Testing y Refinamiento (1 hora)
1. ✅ Pruebas de todos los endpoints
2. ✅ Verificar responsive en móvil
3. ✅ Validar dark mode
4. ✅ Revisar traducciones
5. ✅ Optimizar performance
6. ✅ Documentar implementación

## 🔒 CONSIDERACIONES DE SEGURIDAD

1. **Contraseñas**: Hash con BCrypt (factor 10)
2. **Tokens 2FA**: Encriptados en base de datos
3. **Tarjetas**: Solo guardar últimos 4 dígitos
4. **Permisos**: Verificar customer.view, customer.create, etc.
5. **Soft delete**: No eliminar físicamente, usar DeletedAt
6. **Auditoría**: Log de todas las operaciones críticas

## 🚦 CRITERIOS DE ÉXITO

- ✅ CRUD completo funcionando
- ✅ UI idéntica a los mockups
- ✅ 100% responsive
- ✅ Dark mode completo
- ✅ Traducciones ES/EN
- ✅ Sin errores de TypeScript
- ✅ Performance < 200ms en API
- ✅ Todos los campos de las UI implementados

## 📝 NOTAS ADICIONALES

1. **CustomerId Format**: Generar automáticamente como #XXXXXX (6 dígitos)
2. **Avatar**: Si no tiene, mostrar iniciales con color basado en nombre
3. **Países**: Usar componente CountryFlag existente
4. **Orders**: Por ahora mostrar datos mock, se integrará después
5. **Loyalty Program**: Sistema de puntos con tiers (Platinum > 3000, Gold > 1000, Silver < 1000)
6. **Coupons**: Integración pendiente con módulo de promociones

## 🔄 MIGRACIONES REQUERIDAS

```powershell
# Después de crear los modelos
Add-Migration AddCustomerModels -Context ApplicationDbContext

# Aplicar migración
Update-Database -Context ApplicationDbContext
```

## 📚 DOCUMENTACIÓN A CREAR

1. `/docs/implementations/features/2025-08-customers-implementation.md`
2. `/docs/troubleshooting/customers/customers-01-common-issues.md`
3. Actualizar PROJECT-PROGRESS.md con avance

---

**INICIO ESTIMADO**: Inmediato  
**DURACIÓN TOTAL**: 6-8 horas  
**PRIORIDAD**: Alta (módulo core del sistema)