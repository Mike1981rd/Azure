#!/bin/bash

# Script de validación para Snapshot Estable en Staging
# Fecha: 2025-09-03

echo "========================================"
echo "VALIDACIÓN DE SNAPSHOT ESTABLE - STAGING"
echo "========================================"
echo ""

# Variables
API_URL="https://websitebuilder-api-staging.onrender.com"
COMPANY_ID=1
PAGE_SLUG="home"

echo "1. VALIDANDO ENDPOINT DE SNAPSHOT"
echo "---------------------------------"
echo "Endpoint: $API_URL/api/website/$COMPANY_ID/snapshot/$PAGE_SLUG"
echo ""

# Test 1: Verificar endpoint de snapshot
echo "Solicitando snapshot..."
RESPONSE=$(curl -s -w "\n%{http_code}" "$API_URL/api/website/$COMPANY_ID/snapshot/$PAGE_SLUG")
HTTP_CODE=$(echo "$RESPONSE" | tail -n 1)
BODY=$(echo "$RESPONSE" | head -n -1)

if [ "$HTTP_CODE" == "200" ]; then
    echo "✅ Endpoint respondió correctamente (HTTP $HTTP_CODE)"
    echo "Primeros 200 caracteres del response:"
    echo "$BODY" | head -c 200
    echo "..."
else
    echo "❌ Error: HTTP $HTTP_CODE"
    echo "Response: $BODY"
fi

echo ""
echo "2. VERIFICANDO HEADERS DE CACHE"
echo "-------------------------------"

# Test 2: Verificar headers
HEADERS=$(curl -s -I "$API_URL/api/website/$COMPANY_ID/snapshot/$PAGE_SLUG")
echo "$HEADERS" | grep -i "cache-control"
echo "$HEADERS" | grep -i "vary"
echo "$HEADERS" | grep -i "x-snapshot"

# Validar Cache-Control específico
if echo "$HEADERS" | grep -qi "cache-control.*public.*max-age=86400"; then
    echo "✅ Cache-Control configurado correctamente (public, max-age=86400)"
else
    echo "❌ Cache-Control no está configurado correctamente"
fi

echo ""
echo "3. VERIFICANDO ENDPOINT DE PÁGINAS PUBLICADAS"
echo "---------------------------------------------"

PAGES_RESPONSE=$(curl -s "$API_URL/api/website/$COMPANY_ID/snapshot/pages")
echo "Response de páginas publicadas:"
echo "$PAGES_RESPONSE" | python3 -m json.tool 2>/dev/null || echo "$PAGES_RESPONSE"

echo ""
echo "4. PROBANDO SNAPSHOT POR VERSIÓN (si existe)"
echo "--------------------------------------------"

# Intentar obtener versión 1 de una página
VERSION_RESPONSE=$(curl -s -w "\n%{http_code}" "$API_URL/api/website/$COMPANY_ID/snapshot/page/1/version/1")
VERSION_CODE=$(echo "$VERSION_RESPONSE" | tail -n 1)

if [ "$VERSION_CODE" == "200" ]; then
    echo "✅ Endpoint de versiones funcionando"
else
    echo "ℹ️ No hay versiones disponibles aún o endpoint no accesible (HTTP $VERSION_CODE)"
fi

echo ""
echo "5. VALIDANDO FALLBACK (página inexistente)"
echo "------------------------------------------"

FALLBACK_RESPONSE=$(curl -s -w "\n%{http_code}" "$API_URL/api/website/$COMPANY_ID/snapshot/nonexistent-page")
FALLBACK_CODE=$(echo "$FALLBACK_RESPONSE" | tail -n 1)

if [ "$FALLBACK_CODE" == "404" ]; then
    echo "✅ Fallback correcto para páginas inexistentes (HTTP 404)"
else
    echo "⚠️ Respuesta inesperada para página inexistente: HTTP $FALLBACK_CODE"
fi

echo ""
echo "6. PRUEBA DE ACTUALIZACIÓN (TRIGGER SNAPSHOT)"
echo "---------------------------------------------"

# Test: Actualizar una página para trigger snapshot
echo "Actualizando página HOME para trigger snapshot..."
UPDATE_RESPONSE=$(curl -s -X PUT \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer test-token" \
    -d '{"name":"Home Updated","metaTitle":"Home - Test Update"}' \
    "$API_URL/api/websitepages/1" 2>&1)

echo "Response de actualización:"
echo "$UPDATE_RESPONSE" | head -c 500
echo ""

echo ""
echo "7. VERIFICAR LOGS DE SNAPSHOT"
echo "-----------------------------"
echo "Buscar en logs de Render por patrones:"
echo "- [SNAPSHOT]"
echo "- [DEBUG]"
echo "- GenerateSnapshotAsync"
echo ""

echo ""
echo "8. RESUMEN DE VALIDACIÓN"
echo "------------------------"
echo "Fecha: $(date)"
echo "API URL: $API_URL"
echo ""

# Resumen
if [ "$HTTP_CODE" == "200" ] && echo "$HEADERS" | grep -qi "cache-control.*public.*max-age=86400"; then
    echo "✅ VALIDACIÓN EXITOSA: El sistema de snapshots está funcionando correctamente"
else
    echo "❌ VALIDACIÓN FALLIDA: Revisar los errores anteriores"
fi

echo ""
echo "========================================"
echo "FIN DE VALIDACIÓN"
echo "========================================"