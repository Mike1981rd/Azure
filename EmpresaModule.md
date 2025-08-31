# 🏢 EMPRESA MODULE - PLANIFICACIÓN Y DESARROLLO

## 📋 **RESUMEN EJECUTIVO**

Implementación del módulo Empresa que incluye los tabs Store Details y Payments, con sistema multi-tenant, traducciones i18n, diseño responsive y migración segura de base de datos.

---

## 🎯 **ALCANCE DEL MÓDULO**

### **Tabs Incluidos:**
1. **Store Details** - Configuración general de la empresa/hotel
2. **Payments** - Configuración de métodos de pago y pasarelas

### **Objetivos:**
- ✅ Actualizar modelo Hotel con campos completos
- ✅ Crear modelo PaymentProvider
- ✅ Implementar UI pixel-perfect según imágenes
- ✅ Sistema i18n (ES/EN) integrado
- ✅ Diseño mobile-first responsive
- ✅ Migración segura de base de datos

---

## 🗄️ **MODELOS DE BASE DE DATOS**

### **Hotel Model - Campos Actualizados:**

#### **Campos Existentes:**
```csharp
public int Id { get; set; }
public string Name { get; set; } = string.Empty;
public string? Domain { get; set; }
public string? CustomDomain { get; set; }
public string? Subdomain { get; set; }
public string? Logo { get; set; }
public string? PrimaryColor { get; set; }
public string? SecondaryColor { get; set; }
public bool IsActive { get; set; } = true;
public DateTime CreatedAt { get; set; }
public DateTime UpdatedAt { get; set; }
```

#### **Campos Nuevos:**
```csharp
// Profile Section
public string? PhoneNumber { get; set; }
public string? ContactEmail { get; set; }
public string? SenderEmail { get; set; }

// Billing Information
public string? LegalBusinessName { get; set; }
public string? Country { get; set; }
public string? Region { get; set; }
public string? Address { get; set; }
public string? Apartment { get; set; }
public string? City { get; set; }
public string? State { get; set; }
public string? PostalCode { get; set; }

// Time Zone & Units
public string? TimeZone { get; set; }      // "(GMT-12:00) International Date Line West"
public string? MetricSystem { get; set; }  // "Metric"
public string? WeightUnit { get; set; }    // "Kilograms"

// Store Currency
public string? Currency { get; set; }      // "USD", "DOP", etc.

// Order ID Format
public string? OrderIdPrefix { get; set; } // "#"
public string? OrderIdSuffix { get; set; } // ""
```

### **PaymentProvider Model - Nuevo:**

```csharp
public class PaymentProvider 
{
    public int Id { get; set; }
    public int HotelId { get; set; }  // Multi-tenant
    
    // Información Básica
    public string Name { get; set; } = string.Empty;        // "PayPal", "Azul Dominicana"
    public string Provider { get; set; } = string.Empty;    // "paypal", "azul", "stripe"
    public string? Logo { get; set; }
    
    // Estado y Configuración
    public bool IsActive { get; set; } = false;
    public bool IsManual { get; set; } = false;      // Manual vs Gateway
    public decimal TransactionFee { get; set; } = 0; // 2.99
    public bool IsTestMode { get; set; } = true;     // Modo prueba
    
    // Credenciales Generales (encriptadas)
    public string? ApiKey { get; set; }
    public string? SecretKey { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    
    // Específicos para Azul Dominicana
    public string? StoreId { get; set; }         // 39035544035
    public string? Auth1 { get; set; }           // Usuario (encriptado)
    public string? Auth2 { get; set; }           // Contraseña (encriptado)
    public string? CertificatePath { get; set; } // Ruta .pem
    public string? PrivateKeyPath { get; set; }  // Ruta .key
    
    // Auditoría
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navegación
    public Hotel Hotel { get; set; } = null!;
}
```

---

## 🎨 **UI/UX REQUERIMIENTOS**

### **Design System:**
- **Color Primario:** Según `colorprimario.md`
- **Responsive:** Mobile-first approach
- **Pixel-Perfect:** Exacto a las imágenes proporcionadas
- **Traducciones:** Sistema i18n ES/EN completo

### **Imágenes de Referencia:**
1. **Store Details:** `C:\Users\hp\Desktop\storedetailstab.png`
2. **Payments:** `C:\Users\hp\Desktop\paymentstab.png`
3. **Azul Config:** `C:\Users\hp\Desktop\payment configuration.png`

### **Funcionalidades UI:**
- Formularios con validación en tiempo real
- Toggle switches para configuraciones
- Upload de archivos (certificados SSL)
- Dropdowns para selección de países/zonas horarias
- Estados visuales (Active/Inactive) con badges

---

## 🔐 **SEGURIDAD Y CONFIGURACIÓN**

### **Providers por Defecto:**
1. **Azul Dominicana** 🇩🇴 - Configuración completa
2. **Cardnet** 💳 - Preparado para configuración
3. **PayPal** 🏦 - Preparado para configuración  
4. **Stripe** ⚡ - Preparado para configuración

### **Funcionalidades de Seguridad:**
- **Encriptación** de credenciales sensibles
- **Upload seguro** de certificados SSL fuera del directorio público
- **Validación** de campos requeridos
- **Multi-tenant** isolation automático

---

## ⏳ **TABS FALTANTES (FUTURA IMPLEMENTACIÓN)**

### **Módulos Pendientes:**
1. **Checkout** → Modelo: `CheckoutSettings`
2. **Shipping & Delivery** → Modelo: `ShippingMethod`
3. **Locations** → Modelo: `Location`
4. **Notifications** → Modelo: `NotificationSettings`
5. **Order** → Modelo: `Order`, `OrderItem`
6. **Customer** → Modelo: `Customer`
7. **Manage Reviews** → Modelo: `Review`, `Rating`
8. **Referrals** → Modelo: `Referral`, `ReferralCode`
9. **Logistics** → Modelo: `Inventory`, `Stock`
10. **Invoice** → Modelo: `Invoice`, `InvoiceItem`

**Nota:** Se requieren imágenes de cada tab para implementación correcta.

---

## ✅ **CHECKLIST DE IMPLEMENTACIÓN**

### **FASE 1: PREPARACIÓN DE MODELOS**
- [ ] **1.1** Actualizar modelo `Hotel` con nuevos campos
- [ ] **1.2** Crear modelo `PaymentProvider` completo
- [ ] **1.3** Configurar relaciones en `ApplicationDbContext`
- [ ] **1.4** Crear migración (PREPARAR, no ejecutar)
- [ ] **1.5** Proporcionar comando de migración al usuario

### **FASE 2: BACKEND IMPLEMENTATION**
- [ ] **2.1** Crear `HotelController` con endpoints CRUD
- [ ] **2.2** Crear `PaymentProviderController` con endpoints CRUD
- [ ] **2.3** Implementar `IHotelService` e `HotelService`
- [ ] **2.4** Implementar `IPaymentProviderService` e `PaymentProviderService`
- [ ] **2.5** Crear DTOs para requests/responses
- [ ] **2.6** Implementar validaciones de campos
- [ ] **2.7** Configurar encriptación para credenciales sensibles
- [ ] **2.8** Implementar upload seguro de archivos SSL

### **FASE 3: FRONTEND - STORE DETAILS TAB**
- [ ] **3.1** Crear página `/empresa/configuracion`
- [ ] **3.2** Implementar formulario Profile Section
  - [ ] Store Name, Phone Number
  - [ ] Store contact email, Sender email
- [ ] **3.3** Implementar formulario Billing Information
  - [ ] Legal business name, Country/region
  - [ ] Address, Apartment, City, State, PIN Code
- [ ] **3.4** Implementar Time Zone & Units Section
  - [ ] Time Zone dropdown
  - [ ] Metric system, Weight unit dropdowns
- [ ] **3.5** Implementar Store Currency Section
- [ ] **3.6** Implementar Order ID Format Section
- [ ] **3.7** Agregar traducciones i18n (ES/EN)
- [ ] **3.8** Hacer responsive mobile-first
- [ ] **3.9** Aplicar color primario según documentación

### **FASE 4: FRONTEND - PAYMENTS TAB**
- [ ] **4.1** Crear página `/empresa/pagos`
- [ ] **4.2** Implementar Payment Providers Section
  - [ ] Botón "Choose A Provider"
  - [ ] Modal de selección de providers
- [ ] **4.3** Implementar Supported Payment Methods Section
  - [ ] Lista de providers configurados
  - [ ] Estados (Active/Inactive) con badges
  - [ ] Transaction fees display
  - [ ] Botones de activación/desactivación
- [ ] **4.4** Implementar Manual Payment Methods Section
  - [ ] Dropdown "Add Manual Payment Method"
  - [ ] Configuración de métodos manuales
- [ ] **4.5** Crear modal de configuración de Azul Dominicana
  - [ ] Credenciales API (Store ID, Auth1, Auth2)
  - [ ] Upload de certificados SSL (.pem, .key)
  - [ ] Toggle Modo de Prueba
  - [ ] Información importante
- [ ] **4.6** Agregar traducciones i18n (ES/EN)
- [ ] **4.7** Hacer responsive mobile-first
- [ ] **4.8** Aplicar color primario según documentación

### **FASE 5: INTEGRACIÓN Y TESTING**
- [ ] **5.1** Conectar frontend con API endpoints
- [ ] **5.2** Implementar manejo de errores
- [ ] **5.3** Agregar loading states
- [ ] **5.4** Implementar validaciones cliente-servidor
- [ ] **5.5** Testing de funcionalidades completas
- [ ] **5.6** Testing responsive en dispositivos móviles
- [ ] **5.7** Verificar traducciones funcionando
- [ ] **5.8** Testing de upload de archivos SSL

### **FASE 6: DOCUMENTACIÓN Y DEPLOYMENT**
- [ ] **6.1** Documentar endpoints API generados
- [ ] **6.2** Crear troubleshooting docs si es necesario
- [ ] **6.3** Actualizar PROJECT-PROGRESS.md
- [ ] **6.4** Preparar para commit (preguntar al usuario)

---

## 🚦 **CRITERIOS DE ÉXITO**

- ✅ Modelo Hotel expandido con todos los campos requeridos
- ✅ Modelo PaymentProvider funcional con Azul Dominicana
- ✅ UI pixel-perfect según imágenes de referencia
- ✅ Sistema i18n funcionando en ambos tabs
- ✅ Diseño completamente responsive
- ✅ Migración preparada correctamente
- ✅ Funcionalidades de seguridad implementadas
- ✅ Upload de certificados SSL funcionando
- ✅ Multi-tenancy funcionando correctamente

---

## 📝 **NOTAS IMPORTANTES**

1. **Migración:** Claude prepara, usuario ejecuta manualmente
2. **Traducciones:** Usar sistema i18n existente
3. **Responsive:** Mobile-first approach obligatorio
4. **Seguridad:** Encriptar credenciales antes de almacenar
5. **Multi-tenant:** Todos los datos filtrados por HotelId
6. **Testing:** Verificar en múltiples dispositivos

---

**Fecha de Creación:** 2025-01-18  
**Estado:** Planificación Completa - Listo para Implementación  
**Siguiente Paso:** Ejecutar FASE 1 del checklist