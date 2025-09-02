# 📋 Sistema de Roles y Usuarios - Documentación Técnica

## 🎯 Visión General

Sistema de autorización basado en roles (RBAC) con permisos granulares de 5 niveles para controlar tanto la interfaz de usuario como las operaciones del backend.

## 🏗️ Arquitectura

### Modelo de Datos
```
Usuario (N) ←→ (N) Roles (N) ←→ (N) Permisos
```

### Niveles de Permisos
1. **View** - Controla visibilidad en menú/sidebar
2. **Read** - Permite acceso a la página/recurso
3. **Create** - Habilita creación de nuevos registros
4. **Update** - Permite editar registros existentes
5. **Delete** - Autoriza eliminación de registros

## 📊 Estructura de Permisos

### Formato
```
recurso.acción
```

### Recursos del Sistema
- **dashboard** - Panel principal (view, read)
- **users** - Gestión de usuarios (view, read, create, update, delete)
- **roles** - Gestión de roles (view, read, create, update, delete)
- **products** - Catálogo de productos (view, read, create, update, delete)
- **rooms** - Habitaciones del hotel (view, read, create, update, delete)
- **reservations** - Sistema de reservas (view, read, create, update, delete)
- **website** - Website builder (view, read, update)
- **reports** - Reportes y analytics (view, read)
- **settings** - Configuración del sistema (view, read, update)

## 👥 Roles Predefinidos

### SuperAdmin
- **Descripción**: Administrador del sistema completo
- **Permisos**: Todos (bypass de validaciones)
- **Multi-tenant**: Acceso a todos los hoteles
- **Sistema**: Sí (no se puede eliminar)

### HotelAdmin
- **Descripción**: Administrador del hotel
- **Permisos**: Todo excepto gestión de usuarios/roles del sistema
- **Multi-tenant**: Solo su hotel
- **Sistema**: Sí

### Editor
- **Descripción**: Editor de contenido web
- **Permisos**: 
  - website.* (todo)
  - products.* (todo)
  - pages.* (todo)
  - media.* (todo)
- **Sistema**: Sí

### Receptionist
- **Descripción**: Recepcionista del hotel
- **Permisos**:
  - rooms.view, rooms.read, rooms.update
  - reservations.* (todo)
  - guests.* (todo)
  - dashboard.view, dashboard.read
- **Sistema**: Sí

### Viewer
- **Descripción**: Solo lectura
- **Permisos**: *.view, *.read en todos los recursos
- **Sistema**: Sí

## 🔐 Implementación Técnica

### DTOs

#### CreateRoleDto
```csharp
{
    Name: string (required, unique)
    Description: string (required)
    Permissions: int[] (permission IDs)
}
```

#### UpdateRoleDto
```csharp
{
    Name: string (required)
    Description: string (required)
    Permissions: int[] (permission IDs)
}
```

#### RoleDto
```csharp
{
    Id: int
    Name: string
    Description: string
    IsSystemRole: bool
    Permissions: PermissionDto[]
    UserCount: int
    CreatedAt: DateTime
}
```

#### CreateUserDto
```csharp
{
    Email: string (required, email format)
    Password: string (required, min 6 chars)
    FirstName: string (required)
    LastName: string (required)
    PhoneNumber: string (optional)
    RoleIds: int[] (required, min 1)
}
```

#### UpdateUserDto
```csharp
{
    FirstName: string (required)
    LastName: string (required)
    PhoneNumber: string (optional)
    IsActive: bool
    RoleIds: int[] (required, min 1)
}
```

#### UserDto
```csharp
{
    Id: int
    Email: string
    FullName: string
    PhoneNumber: string
    IsActive: bool
    EmailConfirmed: bool
    LastLoginAt: DateTime?
    Roles: RoleDto[]
    EffectivePermissions: string[] (e.g., ["products.view", "products.read"])
    CreatedAt: DateTime
    UpdatedAt: DateTime
}
```

### Endpoints API

#### Roles
- `GET /api/roles` - Lista todos los roles
- `GET /api/roles/{id}` - Obtiene un rol específico
- `POST /api/roles` - Crea un nuevo rol
- `PUT /api/roles/{id}` - Actualiza un rol
- `DELETE /api/roles/{id}` - Elimina un rol (no sistema)
- `POST /api/roles/{id}/permissions` - Asigna permisos

#### Users
- `GET /api/users` - Lista usuarios (filtrado por hotel)
- `GET /api/users/{id}` - Obtiene un usuario
- `POST /api/users` - Crea un usuario
- `PUT /api/users/{id}` - Actualiza un usuario
- `DELETE /api/users/{id}` - Desactiva un usuario
- `GET /api/users/{id}/permissions` - Permisos efectivos

#### Permissions
- `GET /api/permissions` - Lista todos los permisos
- `GET /api/permissions/grouped` - Agrupados por recurso

### Autorización

#### Attribute Usage
```csharp
[RequirePermission("products.create")]
public async Task<IActionResult> CreateProduct() { }
```

#### JWT Claims
```json
{
  "sub": "user-id",
  "email": "user@hotel.com",
  "hotelId": "1",
  "roles": ["HotelAdmin", "Editor"],
  "permissions": ["products.view", "products.read", "products.create", ...],
  "exp": 1234567890
}
```

## 🎨 UI/UX Consideraciones

### Sidebar Dinámico
```typescript
// Solo muestra opciones con permiso 'view'
{canView('products') && (
  <SidebarItem href="/products">Productos</SidebarItem>
)}
```

### Botones Condicionales
```typescript
// Botón crear solo si tiene permiso
{canCreate('products') && (
  <Button onClick={handleCreate}>Nuevo Producto</Button>
)}
```

### Protección de Rutas
```typescript
// Página completa protegida
if (!canRead('products')) {
  return <AccessDenied />;
}
```

## 🔄 Flujo de Autorización

1. **Login**: Usuario se autentica
2. **Token**: JWT generado con roles y permisos
3. **Request**: Cliente envía token en header
4. **Validación**: Middleware verifica permisos
5. **Acceso**: Permitido o denegado (403)

## 🧪 Casos de Prueba

### Escenario 1: Secretario
- ✅ Ve "Productos" en menú (products.view)
- ✅ Accede a lista de productos (products.read)
- ❌ No ve botón "Nuevo" (no tiene products.create)
- ✅ Puede editar productos (products.update)
- ❌ No puede eliminar (no tiene products.delete)

### Escenario 2: Multi-tenant
- Usuario A (Hotel 1) no puede ver datos de Hotel 2
- SuperAdmin puede ver todos los hoteles
- Queries filtradas automáticamente por HotelId

## 🚀 Mejoras Futuras

1. **Permisos Dinámicos**: Crear permisos desde UI
2. **Delegación**: Usuarios pueden delegar permisos temporalmente
3. **Audit Trail**: Log de todos los cambios de permisos
4. **Permisos por Registro**: Permisos a nivel de registro individual
5. **Grupos de Permisos**: Agrupar permisos relacionados

## 📝 Notas de Implementación

- Los roles del sistema (IsSystemRole = true) no se pueden eliminar
- Un usuario debe tener al menos un rol activo
- Los permisos se cachean por 5 minutos para performance
- La validación ocurre tanto en API como en UI
- Multi-tenancy se aplica automáticamente via filtros globales

---

**Última actualización**: Agosto 2025
**Versión**: 1.0
**Autor**: Equipo de Desarrollo WebsiteBuilder