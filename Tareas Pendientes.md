# Tareas Pendientes

## Estado actual (hecho)
- [x] Eliminar cron @reboot que reseteaba la clave de `postgres`.
- [x] Crear base de datos `websitebuilder` y verificar conexión local.
- [x] Backend corriendo en `:5266` detrás de Nginx; PostgreSQL en `:5432`.

## Seguridad y red
- [ ] Restringir `5432` en el NSG (solo IPs internas/VPN o cerrar públicamente).
- [ ] Restringir `22` a IP(s) de administración (o Bastion) y habilitar UFW si aplica.
- [ ] Rotar credenciales expuestas en documentación y mover secretos a variables de entorno/Key Vault.
- [ ] Habilitar HTTPS (Nginx + Let’s Encrypt):
  - Dominio y correo pendientes (A‑record al IP 20.169.209.166; e‑mail para certbot).
  - Instalar `certbot` + plugin Nginx, emitir certificado, forzar 80→443 y probar `/health`.
  - Actualizar CORS para incluir `https://<tu-dominio>`.

## Base de datos
- [ ] Crear usuario de aplicación (no superusuario), p. ej. `wb_user` con contraseña fuerte y permisos mínimos sobre `websitebuilder`.
- [ ] Actualizar la cadena de conexión en el backend (appsettings/variables) para usar `wb_user`.
- [ ] Revisar `pg_hba.conf` (mantener `scram-sha-256`) y reiniciar/reload si se cambia.

## Arranque robusto del backend
- [ ] Migrar de `screen` a servicio systemd `websitebuilder.service` con `After=postgresql.service`.
- [ ] Añadir espera con backoff/healthcheck a la DB antes de iniciar el API.
- [ ] Publicar binarios con `dotnet publish -c Release` y ruta estable para systemd.

## Observabilidad
- [ ] Habilitar logs/metrics de PostgreSQL (log de conexiones/errores) y agente de Azure Monitor si se requiere.
- [ ] Validar logs del API y rotación en `logs/`.

## Verificación
- [ ] Reiniciar la VM y validar: contraseña persiste, DB disponible y API conecta sin errores.
- [ ] Ejecutar pruebas de humo (scripts `test-*.ps1` o `WebsiteBuilderAPI.http`).

## Opcional
- [ ] Registrar provider `Microsoft.App` si se evalúa Container Apps: `az provider register -n Microsoft.App --wait`.
