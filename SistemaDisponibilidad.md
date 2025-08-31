# 📋 SISTEMA DE DISPONIBILIDAD DE HABITACIONES - PLAN DE IMPLEMENTACIÓN

## 🎯 OBJETIVO
Implementar un sistema completo de gestión de disponibilidad con calendario visual, bloqueos por mantenimiento y reglas de negocio avanzadas.

---

## **FASE 1: BACKEND - MODELOS Y BASE DE DATOS** (2 horas)

### 1.1 **Crear Modelos** 📦

#### Models/RoomAvailability.cs
```csharp
public class RoomAvailability
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int RoomId { get; set; }
    public DateTime Date { get; set; }
    public bool IsAvailable { get; set; } = true;
    public bool IsBlocked { get; set; } = false;
    public string? BlockReason { get; set; }
    public decimal? CustomPrice { get; set; }
    public int? MinNights { get; set; }
    
    // Navegación
    public Room Room { get; set; }
    public Company Company { get; set; }
}
```

#### Models/RoomBlockPeriod.cs
```csharp
public class RoomBlockPeriod
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int? RoomId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Reason { get; set; }
    public string? Notes { get; set; }
    public bool IsRecurring { get; set; } = false;
    public string? RecurrencePattern { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CreatedByUserId { get; set; }
    
    // Navegación
    public Room? Room { get; set; }
    public Company Company { get; set; }
    public User CreatedBy { get; set; }
}
```

#### Models/AvailabilityRule.cs
```csharp
public class AvailabilityRule
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int? RoomId { get; set; }
    public string RuleType { get; set; }
    public string RuleValue { get; set; }
    public bool IsActive { get; set; } = true;
    public int Priority { get; set; } = 0;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    // Navegación
    public Room? Room { get; set; }
    public Company Company { get; set; }
}
```

### 1.2 **Actualizar ApplicationDbContext** 🗄️
- Agregar DbSets
- Configurar índices únicos
- Configurar relaciones y cascadas

### 1.3 **Crear Migración** 🔄
```powershell
dotnet ef migrations add AddRoomAvailabilitySystem
dotnet ef database update
```

---

## **FASE 2: BACKEND - SERVICIOS Y LÓGICA** (3 horas)

### 2.1 **DTOs** 📄

#### DTOs/Availability/AvailabilityDtos.cs
- `RoomAvailabilityDto`
- `AvailabilityGridDto`
- `CreateBlockPeriodDto`
- `UpdateAvailabilityRuleDto`
- `DayAvailabilityDto`
- `OccupancyStatsDto`

### 2.2 **AvailabilityService** ⚙️

#### Services/IAvailabilityService.cs
```csharp
public interface IAvailabilityService
{
    Task<AvailabilityGridDto> GetAvailabilityGridAsync(int companyId, DateTime startDate, DateTime endDate, int[]? roomIds = null);
    Task<bool> CheckAvailabilityAsync(int roomId, DateTime checkIn, DateTime checkOut);
    Task<RoomBlockPeriod> CreateBlockPeriodAsync(int companyId, CreateBlockPeriodDto dto, int userId);
    Task<bool> RemoveBlockPeriodAsync(int companyId, int blockId);
    Task<List<AvailabilityRule>> GetActiveRulesAsync(int companyId, int? roomId = null);
    Task<AvailabilityRule> CreateOrUpdateRuleAsync(int companyId, UpdateAvailabilityRuleDto dto);
    Task SyncAvailabilityCalendarAsync(int companyId, int roomId, DateTime startDate, DateTime endDate);
    Task<OccupancyStatsDto> GetOccupancyStatsAsync(int companyId, DateTime startDate, DateTime endDate);
}
```

### 2.3 **AvailabilityController** 🎮

#### Endpoints:
- `GET /api/availability/grid` - Obtener grid de disponibilidad
- `GET /api/availability/check/{roomId}` - Verificar disponibilidad
- `POST /api/availability/block` - Crear período de bloqueo
- `DELETE /api/availability/block/{id}` - Eliminar bloqueo
- `GET /api/availability/rules` - Obtener reglas
- `POST /api/availability/rules` - Crear/actualizar regla
- `POST /api/availability/sync/{roomId}` - Sincronizar calendario
- `GET /api/availability/stats/occupancy` - Estadísticas de ocupación

---

## **FASE 3: FRONTEND - CALENDARIO VISUAL** (4 horas)

### 3.1 **Estructura de Archivos** 📁
```
websitebuilder-admin/src/
├── app/dashboard/disponibilidad/
│   ├── page.tsx
│   ├── layout.tsx
│   └── configuracion/
│       └── page.tsx
├── components/availability/
│   ├── AvailabilityGrid.tsx
│   ├── RoomAvailabilityRow.tsx
│   ├── DayCell.tsx
│   ├── BlockPeriodModal.tsx
│   ├── AvailabilityLegend.tsx
│   ├── QuickStats.tsx
│   └── RulesManager.tsx
└── lib/api/availability.ts
```

### 3.2 **Diseño UI** 🎨

#### Paleta de Colores
- **Disponible**: `bg-emerald-100 dark:bg-emerald-900/30`
- **Ocupado**: `bg-red-100 dark:bg-red-900/30`
- **Bloqueado**: `bg-gray-400 dark:bg-gray-600`
- **Check-in**: `bg-blue-500 dark:bg-blue-600`
- **Check-out**: `bg-orange-500 dark:bg-orange-600`
- **Precio especial**: `bg-yellow-100 dark:bg-yellow-900/30`
- **Seleccionado**: `ring-2 ring-primary`
- **Hoy**: `ring-2 ring-blue-400`

#### Componentes Visuales
1. **Grid Principal**: CSS Grid responsive con scroll horizontal
2. **Celdas Interactivas**: Hover effects, drag selection
3. **Tooltips Informativos**: Detalles al hover
4. **Modales Elegantes**: Radix UI Dialog
5. **Animaciones Suaves**: Framer Motion

### 3.3 **Funcionalidades UI** ✨
- Vista mensual/semanal
- Navegación por fechas
- Filtro por habitación
- Selección múltiple (drag)
- Zoom in/out
- Exportar a Excel/PDF
- Vista de impresión

---

## **FASE 4: FUNCIONALIDADES AVANZADAS** (3 horas)

### 4.1 **Modal de Bloqueo** 🚫
- Selector de razón con iconos
- Campo de notas
- Opción de recurrencia
- Vista previa del período
- Confirmación con resumen

### 4.2 **Gestión de Reglas** 📏

#### Tipos de Reglas:
1. **MIN_NIGHTS**: Mínimo de noches
2. **NO_CHECKIN_DAYS**: Días sin check-in
3. **ADVANCE_BOOKING**: Reserva anticipada
4. **MAX_STAY**: Estadía máxima
5. **SEASONAL_PRICING**: Precios por temporada

### 4.3 **Estadísticas de Ocupación** 📊
- Ocupación total (%)
- Habitaciones disponibles hoy
- Check-ins/Check-outs del día
- Ingresos proyectados
- Gráfico de tendencias

---

## **FASE 5: INTEGRACIÓN Y OPTIMIZACIÓN** (2 horas)

### 5.1 **Integración con Reservaciones** 🔗
- Verificación automática de disponibilidad
- Aplicación de reglas de negocio
- Actualización en tiempo real
- Sincronización bidireccional

### 5.2 **Sistema de Cache** ⚡
- Cache en memoria (5 minutos)
- Cache en localStorage
- Invalidación inteligente
- Precarga de datos comunes

### 5.3 **Notificaciones en Tiempo Real** 🔔
- WebSockets/SignalR
- Actualizaciones instantáneas
- Notificaciones de conflictos
- Sincronización multi-usuario

---

## **FASE 6: TESTING Y DOCUMENTACIÓN** (1 hora)

### 6.1 **Tests Unitarios** 🧪
- Lógica de disponibilidad
- Aplicación de reglas
- Cálculo de estadísticas
- Validaciones de negocio

### 6.2 **Tests E2E** 🎭
- Flujo completo de bloqueo
- Creación de reservación con verificación
- Aplicación de reglas
- Sincronización de calendario

---

## **📊 RESUMEN DE IMPLEMENTACIÓN**

### **Tiempo Estimado**: 12-15 horas

### **Stack Tecnológico**:
- **Backend**: ASP.NET Core 8, EF Core, PostgreSQL
- **Frontend**: Next.js 14, TypeScript, Tailwind CSS
- **UI Components**: Radix UI, Framer Motion
- **Estado**: Zustand
- **Fetching**: React Query
- **Validación**: Zod

### **Características Principales**:
✅ Calendario visual tipo Booking.com
✅ Gestión de bloqueos por mantenimiento
✅ Reglas de negocio configurables
✅ Integración con reservaciones
✅ Estadísticas en tiempo real
✅ Multi-idioma (ES/EN)
✅ Dark mode
✅ Responsive design
✅ Exportación de datos

### **Métricas de Éxito**:
- Reducción 80% en overbooking
- Aumento 50% en eficiencia operativa
- Visualización clara de disponibilidad
- Gestión centralizada de bloqueos
- Reportes automatizados

---

## **🚀 ORDEN DE IMPLEMENTACIÓN**

1. **Backend Models + Migration** (30 min)
2. **DTOs + Service Interface** (30 min)
3. **AvailabilityService Implementation** (1 hora)
4. **AvailabilityController** (30 min)
5. **Frontend Page Structure** (30 min)
6. **AvailabilityGrid Component** (2 horas)
7. **Block Period Modal** (1 hora)
8. **Rules Manager** (1 hora)
9. **Stats Component** (30 min)
10. **Integration + Testing** (1 hora)

---

## **📝 NOTAS IMPORTANTES**

### Siguiendo CLAUDE.md:
- ✅ Sin Repository Pattern
- ✅ Single-tenant architecture
- ✅ PowerShell para migraciones
- ✅ i18n obligatorio
- ✅ Dark mode desde inicio
- ✅ Sin comentarios en código
- ✅ Validaciones robustas
- ✅ DTOs para todo

### Siguiendo CLAUDEBK1.md:
- ✅ Separación rooms/products
- ✅ No mezclar conceptos
- ✅ Cache diferenciado
- ✅ UI consistente

### Siguiendo CLAUDEBK2.md:
- ✅ Componentes reutilizables
- ✅ Patterns establecidos
- ✅ Error handling robusto
- ✅ Performance optimizado

---

## **✅ CHECKLIST DE IMPLEMENTACIÓN**

- [ ] Crear modelos de base de datos
- [ ] Ejecutar migración
- [ ] Implementar DTOs
- [ ] Crear AvailabilityService
- [ ] Implementar AvailabilityController
- [ ] Crear estructura frontend
- [ ] Implementar AvailabilityGrid
- [ ] Crear BlockPeriodModal
- [ ] Implementar RulesManager
- [ ] Agregar estadísticas
- [ ] Integrar con reservaciones
- [ ] Implementar cache
- [ ] Agregar traducciones
- [ ] Testing
- [ ] Documentación

---

**Última actualización**: 2025-01-10
**Versión**: 1.0
**Estado**: LISTO PARA IMPLEMENTAR