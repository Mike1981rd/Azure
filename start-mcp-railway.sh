#!/bin/bash

# Script para iniciar el servidor MCP de Railway leyendo el token del entorno
# Uso:
#   export RAILWAY_API_TOKEN="<TU_TOKEN>"
#   ./start-mcp-railway.sh

set -euo pipefail

export NVM_DIR="$HOME/.nvm"
[ -s "$NVM_DIR/nvm.sh" ] && . "$NVM_DIR/nvm.sh"

if [ -z "${RAILWAY_API_TOKEN:-}" ]; then
  echo "[ERROR] RAILWAY_API_TOKEN no está definido en el entorno." >&2
  echo "        Genera un token en https://railway.com/account/tokens y exporta la variable." >&2
  exit 1
fi

echo "🚂 Iniciando servidor MCP de Railway..."
echo "Node.js versión: $(node --version 2>/dev/null || echo 'desconocida')"
echo "Token Railway configurado: (oculto)"

# Iniciar el servidor MCP (usa stdio)
exec npx -y @jasontanswe/railway-mcp

