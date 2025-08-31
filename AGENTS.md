# AGENTS.md — Reglas de sesión para asistentes (Codex/Claude)

Este documento define cómo debe iniciar y comportarse el asistente en este repo para evitar fricción (especialmente con Azure CLI). Mantiene el flujo corto, en modo chat y sin pedirle tareas al usuario.

## Objetivo
- Conectar a Azure CLI en segundos, sin preguntas innecesarias.
- Usar credenciales seguras si no hay sesión previa.
- Evitar instrucciones al usuario; solo pedir permiso de red si el sandbox lo exige.

## Flujo de Arranque (Fast Path)
1. Comprobar sesión: `az account show`.
   - Si responde: informar "Conectado a Azure CLI: <suscripción>" y continuar.
2. Si no hay sesión, intentar login con Service Principal (SP) de forma silenciosa, en este orden:
   - `.azure/sp.env` (ver formato abajo).
   - Bloque parseable en `claude.md` (ver formato abajo).
3. Si el sandbox requiere red, hacer una única solicitud breve: permiso para usar red y ejecutar `az login` (sin mostrar secretos).
4. Si hay `AZURE_SUBSCRIPTION_ID`, fijar suscripción: `az account set --subscription <id>`.
5. Confirmar: `az account show` y reportar estado.

## Credenciales — Orden de Preferencia
1. Archivo `.azure/sp.env` (no versionado). Variables soportadas:
   ```bash
   AZURE_TENANT_ID=00000000-0000-0000-0000-000000000000
   AZURE_CLIENT_ID=00000000-0000-0000-0000-000000000000
   AZURE_CLIENT_SECRET=REDACTED
   AZURE_SUBSCRIPTION_ID=00000000-0000-0000-0000-000000000000  # opcional
   ```
   - El asistente cargará estas variables y ejecutará:
     `az login --service-principal -u $AZURE_CLIENT_ID -p $AZURE_CLIENT_SECRET --tenant $AZURE_TENANT_ID`
   - Si `AZURE_SUBSCRIPTION_ID` existe: `az account set --subscription $AZURE_SUBSCRIPTION_ID`.

2. Bloque parseable en `claude.md` (si el punto 1 no existe). Formato esperado en texto plano:
   ```
   ### Azure Service Principal (parseable)
   TenantId: 00000000-0000-0000-0000-000000000000
   ClientId: 00000000-0000-0000-0000-000000000000
   ClientSecret: <SECRETO>
   SubscriptionId: 00000000-0000-0000-0000-000000000000  # opcional
   ```
   - El asistente extraerá esos valores sin imprimirlos y hará el login SP.

3. Si no hay SP disponible y se permite interacción con navegador/Device Code:
   - Con permiso de red del sandbox, ejecutar `az login --use-device-code` y completar el flujo.

## Reglas de Interacción
- Modo chat, respuestas naturales y cortas.
- No pedir al usuario que ejecute comandos.
- Solo una pregunta breve si falta permiso de red: “¿Autorizas uso de red para `az login`?”
- Nunca imprimir secretos. Si hace falta persistirlos para la sesión, usar archivos temporales fuera del repo.

## Comandos Base que usa el asistente
- Ver sesión actual: `az account show --output jsonc`
- Login SP: `az login --service-principal -u <ClientId> -p <ClientSecret> --tenant <TenantId>`
- Fijar suscripción: `az account set --subscription <SubscriptionId>`
- Listar RGs (sanity): `az group list --output table`

## Manejo de Errores Comunes
- "az no instalado": reportar y continuar con el resto del trabajo sin bloquear.
- "Sin red/sandbox": solicitar una sola vez permiso de red; si se niega, continuar sin Azure.
- "Credenciales no parseables": intentar `.azure/sp.env`; si no está, intentar bloque parseable en `claude.md`; si falla, reportar “faltan credenciales SP válidas”.

## Seguridad
- No versionar secretos. Preferir `.azure/sp.env`. Si existen en `claude.md`, no copiarlos ni mostrarlos.
- Los tokens de Azure CLI permanecen en cache local (`~/.azure`). No mover ni exponer ese directorio.

---

Con este AGENTS.md, el asistente arrancará siempre comprobando sesión de Azure y hará login con SP sin molestar, dejando como única interacción posible la confirmación de red si el sandbox lo exige.

## Vercel CLI — Flujo rápido
1. Verificar CLI: `vercel --version` (reportar versión instalada).
2. Comprobar sesión sin exponer secretos: `vercel whoami`.
   - Si no hay sesión, usar comando con token efímero sin persistir: `vercel whoami --token <TOKEN>`.
   - Evitar `vercel login` interactivo salvo que el usuario lo pida explícitamente.
3. Nunca imprimir ni guardar el token en archivos del repo. No exportar variables persistentes.
4. Desconexión: si fue necesario un login persistente, ejecutar `vercel logout`. Si se usó `--token`, no queda sesión local.
5. Operaciones típicas (tras sesión válida):
   - Link: `vercel link` en `websitebuilder-admin`.
   - Ver envs: `vercel env ls`.
   - Deploy prod: `vercel --prod --yes` (o con `--token` efímero si no hay sesión persistente).

## MCP Supabase — Flujo rápido
1. Detectar configuración: leer `.mcp.json -> mcpServers.supabase.env` para `SUPABASE_URL` y `SUPABASE_ACCESS_TOKEN` (sin imprimirlos).
2. Verificar binario: confirmar `mcp-server-supabase --version` para validar disponibilidad.
3. Conexión (cliente MCP): asumir que el cliente (p. ej. Claude con MCP) iniciará el servidor; no pedir acciones al usuario.
4. Confirmación: reportar en una sola línea “Conectado a Supabase (MCP) con config de .mcp.json”.
5. Si el sandbox requiere red: hacer una única solicitud breve para uso de red.

### Orden de credenciales (Supabase)
- Preferido: valores en `.mcp.json` bajo `mcpServers.supabase.env`.
- Alternativo: bloque parseable en `CLAUDE.md` con el formato:
  ```
  ### Supabase (parseable)
  SUPABASE_URL: https://xxxxx.supabase.co
  SUPABASE_ACCESS_TOKEN: <TOKEN>
  ```
- Nunca imprimir ni copiar el token. Solo usarlo para establecer la sesión MCP.

### Reglas de interacción (Supabase)
- Responder en modo chat, corto y claro.
- No pedir que el usuario ejecute comandos ni scripts.
- Solo una pregunta si la red del sandbox está bloqueada.
- Si `.mcp.json` falta o está incompleto: informar “falta config MCP de Supabase” en una línea, sin más vueltas.

### Comprobaciones rápidas (internas)
- `mcp-server-supabase --version` para confirmar instalación.
- Handshake MCP (opcional, interno): inicializar y listar herramientas; si falla por protocolo/red, reportar “config presente; pendiente cliente MCP”.
