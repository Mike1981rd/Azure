#!/bin/bash

# Script para iniciar el servidor MCP de Supabase
# Uso: ./start-mcp-supabase.sh

export NVM_DIR="$HOME/.nvm"
[ -s "$NVM_DIR/nvm.sh" ] && \. "$NVM_DIR/nvm.sh"

# Configurar variables de entorno para Supabase
export SUPABASE_URL="https://gvxqatvwkjmkvaslbevh.supabase.co"
export SUPABASE_ACCESS_TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Imd2eHFhdHZ3a2pta3Zhc2xiZXZoIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImlhdCI6MTc1NjM2NDk2NiwiZXhwIjoyMDcxOTQwOTY2fQ.PcpBLRo5Mf5jBC4IQO_ineBHjVIj7npK9JhW5dJlUKI"

echo "🚀 Iniciando servidor MCP de Supabase..."
echo "URL: $SUPABASE_URL"
echo "Node.js versión: $(node --version)"
echo "MCP Supabase versión: $(mcp-server-supabase --version)"

# Iniciar el servidor MCP
mcp-server-supabase
