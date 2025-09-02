# 🗄️ MIGRACIÓN REQUERIDA - Sistema de Notificaciones

## ⚠️ IMPORTANTE: Ejecutar esta migración para actualizar el modelo de notificaciones

### Cambios realizados:
1. **Modelo CustomerNotificationPreference** actualizado con campos específicos en lugar de genéricos
2. **DTOs** actualizados para coincidir con el frontend
3. **Relación** cambiada de uno-a-muchos a uno-a-uno (un registro de preferencias por cliente)

### Campos nuevos en CustomerNotificationPreference:
- ✅ EmailOrderUpdates, EmailPromotions, EmailNewsletter, EmailProductReviews, EmailPriceAlerts
- ✅ SmsOrderUpdates, SmsDeliveryAlerts, SmsPromotions  
- ✅ PushEnabled, PushSound, PushVibration
- ✅ DoNotDisturbStart, DoNotDisturbEnd, Timezone
- ✅ CreatedAt, UpdatedAt

### Comandos a ejecutar:

#### Opción 1: Package Manager Console (Visual Studio)
```powershell
# 1. Crear la migración
Add-Migration UpdateNotificationPreferencesModel -Context ApplicationDbContext

# 2. Aplicar la migración
Update-Database -Context ApplicationDbContext
```

#### Opción 2: CLI de .NET
```bash
# 1. Crear la migración
dotnet ef migrations add UpdateNotificationPreferencesModel --context ApplicationDbContext

# 2. Aplicar la migración
dotnet ef database update --context ApplicationDbContext
```

### ⚠️ NOTA IMPORTANTE:
Esta migración:
1. **ELIMINARÁ** los registros existentes de CustomerNotificationPreferences (si hay)
2. **CREARÁ** nuevas columnas con los campos específicos
3. **CAMBIARÁ** la relación de muchos-a-uno a uno-a-uno

### Después de la migración:
- Cada cliente tendrá UN SOLO registro de preferencias de notificación
- Los valores por defecto están configurados en el modelo
- El frontend ya está preparado para trabajar con este formato

### Si hay errores:
1. Verificar que no haya registros duplicados en CustomerNotificationPreferences
2. Eliminar registros antiguos si es necesario: `DELETE FROM "CustomerNotificationPreferences";`
3. Volver a ejecutar la migración