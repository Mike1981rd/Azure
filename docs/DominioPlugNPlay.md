# Dominio Plug‑n‑Play (Onboarding de Dominios para Sitios Construidos)

## Objetivo
- Permitir que un cliente conecte su propio dominio al sitio construido con el Website Builder en minutos, sin asistencia técnica.
- El flujo debe ser 100% guiado, con instrucciones de DNS, verificación automática y conexión a Vercel (alias/SSL) hasta quedar “VERIFIED”.

## Flujo UX (Wizard de 1 pantalla con pasos)
1) Introducir dominio
- Input con validación de formato y disponibilidad interna.
- Mostrar si el dominio ya existe en el sistema (Host → CompanyId) o si hay conflicto.

2) Elegir método de conexión
- Opción recomendada: Mantener DNS en el registrador (Namecheap/GoDaddy/etc.) con:
  - A (apex @) → 76.76.21.21
  - CNAME (www) → cname.vercel-dns.com
- Opción alternativa: Delegar NS a Vercel (ns1.vercel-dns.com, ns2.vercel-dns.com) — menos recomendada para clientes sin experiencia.

3) Instrucciones de DNS (dinámicas por registrador)
- Renderizar pasos específicos según el registrador que elija el cliente (Namecheap, GoDaddy, Cloudflare, etc.).
- Mostrar los valores concretos listos para copiar:
  - A @ → 76.76.21.21 (TTL 300-3600)
  - CNAME www → cname.vercel-dns.com (TTL 300-3600)
- Si Vercel requiere TXT verification: mostrar el registro TXT esperado con botón Copy.
- Estado visual por registro (pendiente/ok/error) con “Revisar ahora”.

4) Verificación automática (polling)
- Checks en backend cada 15–30s (o botón “Revisar ahora”):
  - DNS válido (A @, CNAME www, TXT si aplica) vía DoH (Google/Cloudflare) + resolución recursiva.
  - Dominio agregado al proyecto en Vercel (domains add) y alias aplicado al último deployment prod.
  - SSL emitido por Vercel y estado “Assigned/Valid”.
- Barra de progreso + checklist de cada paso con mensajes claros y tiempos de propagación.

5) Finalizar y visitar sitio
- Marcar Primary Domain (apex o www) y activar la redirección 301 inversa.
- “Visitar sitio” abre el dominio con SSL y renderiza “/home” del sitio construido (no el login del admin).

## API (Backend ASP.NET Core)
- POST `/api/domains/company/{companyId}`
  - Crea el dominio (DomainName, preferencia de primario apex/www) y persiste DnsRecords esperados (A @, CNAME www).
- GET `/api/domains/company/{companyId}/status`
  - Devuelve pipeline y estados: DNS, Vercel, Alias, SSL, VERIFIED; últimos errores y timestamps.
- POST `/api/domains/company/{companyId}/vercel/add`
  - Llama a Vercel para agregar dominio(s) al proyecto.
- POST `/api/domains/company/{companyId}/vercel/alias`
  - Alias del último deployment prod → apex y www.
- POST `/api/domains/company/{companyId}/primary`
  - Define dominio primario (apex o www) y redirección 301.

Notas:
- Guardar pipeline de estados (y mensajes) en tabla Domains (o tabla dedicada DomainConnections) para audit/UX.
- Exponer `error_code` y `error_hint` para mensajes orientados al usuario.

## Integración Vercel (Automática)
- Secretos/ENV en backend:
  - `VERCEL_TOKEN`, `VERCEL_TEAM_ID`, `VERCEL_PROJECT_ID`.
- Acciones cuando DNS_OK = true:
  - Domains Add → adjuntar dominio(s) al proyecto.
  - Alias Set → apuntar el deployment prod actual a apex y www.
  - Definir Primary Domain y redirección 301.
  - Polling hasta SSL activo y estado “Assigned/Valid”.
- Manejo de conflictos:
  - Dominio en otro equipo/proyecto → informar y proponer “liberar y reintentar”.
  - Requerimiento de TXT → generar/mostrar y reintentar automáticamente hasta 24h.

## Verificación DNS (DoH + heurísticas)
- Resolver A @, CNAME www y TXT (si necesario) usando DNS-over-HTTPS (Google y Cloudflare) para resultados globales y evitar cache local.
- Reglas de éxito:
  - A (apex) = 76.76.21.21
  - CNAME (www) = cname.vercel-dns.com
  - TXT (si requerido por Vercel) presente y exacto.
- Manejo de Cloudflare/Proxy:
  - Detectar proxied CNAME (flattening). Guiar al cliente para desactivar proxy si bloquea la verificación.

## Pipeline de Estados
- `ADDED` → dominio creado en DB, DnsRecords esperados guardados.
- `DNS_PENDING` → aún no resuelve correctamente.
- `DNS_VALID` → A/CNAME (y TXT si aplica) correctos.
- `VERCEL_ATTACHED` → dominio agregado al proyecto en Vercel.
- `ALIASED` → alias activo (deployment prod → dominio).
- `SSL_ACTIVE` → certificado emitido.
- `VERIFIED` → dominio utilizable (IsVerified=true) y middleware sirve /home para este host.

Cada transición registra: timestamp, actor (sistema), detalle y error si falla.

## UI/UX (Friendly)
- Librería de registradores: plantilla JSON por proveedor con pasos e imágenes.
- Botones Copy, tooltips y enlaces de ayuda.
- Estado por registro con tips concretos (TTL alto, propagación, ns incorrectos, proxy activo).
- Modo “Experto”: mostrar raw DNS (dig/DoH) y respuestas JSON para soporte avanzado.

## Routing del Frontend
- Middleware de Next.js:
  - Si `hostname` pertenece al admin (localhost, websitebuilder-admin.vercel.app, previews) → `/login`.
  - Si es dominio del cliente (alias) → `/home` (o handle público configurado).
- `NEXT_PUBLIC_API_URL` apunta al backend en Render; snapshots sirviendo desde `PublishedSnapshots`.

## Observabilidad y Reintentos
- Job en background (cada 2–5 min) mientras no esté VERIFIED:
  - Re‑check DNS, domains add, alias y SSL.
- Logs estructurados (por dominio) con último error y próximo reintento.
- Notificaciones opcionales (email/toast) cuando cambie a VERIFIED o si hay bloqueo > 12h.

## Seguridad y Límites
- Rate limit para llamadas a Vercel por dominio (máx. 1/minuto).
- `VERCEL_TOKEN` en secretos del entorno (no exponer al cliente).
- Validar ownership del dominio a nivel de UI/DB (dominio por empresa).

## Plan de Implementación (3 días)
**Día 1**
- UI Wizard (pasos 1–3) con librería de registradores e “expected DNS”.
- Endpoints: crear dominio + `status` con verificación DNS (DoH) y persistencia de DnsRecords esperados.
- Ajuste de middleware host → `/home` para alias.

**Día 2**
- Integración Vercel: domains add, alias set, primary toggle, polling.
- Pipeline de estados y mensajes claros; manejo de TXT verification.

**Día 3**
- QA end‑to‑end con Namecheap.
- Copys y documentación embebida; troubleshooting integrado.
- Métricas y botón “Desconectar dominio”.

## Criterios de Aceptación
- Cliente conecta dominio sin soporte técnico.
- Estado final VERIFIED con SSL activo.
- “Visitar sitio” carga `/home` en el dominio del cliente.
- Mensajes de error accionables en cada paso.

## Errores Comunes y Mensajes
- NS incorrectos / proveedor distinto → “El dominio usa nameservers X; configura A/CNAME en tu registrador o cambia a NS de Vercel”.
- Proxy/Cloudflare → “Desactiva proxy (nube gris) para completar verificación, luego podrás reactivarlo si lo deseas”.
- CNAME flattening → “www debe apuntar a cname.vercel-dns.com; evita apuntar a IPs en www”.
- TXT faltante → “Agrega el TXT de verificación indicado y vuelve a ‘Revisar ahora’”.
- TTL alto → “Reduce TTL temporalmente a 300–600 para acelerar propagación”.

---
**Variables Relevantes**
- `NEXT_PUBLIC_API_URL` (frontend) → dominio del API en Render.
- `VERCEL_TOKEN`, `VERCEL_TEAM_ID`, `VERCEL_PROJECT_ID` (backend) → integración Vercel.

**Valores DNS (recomendados)**
- A (apex @): `76.76.21.21`
- CNAME (www): `cname.vercel-dns.com`

