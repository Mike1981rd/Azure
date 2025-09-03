#!/bin/bash

API_URL="https://websitebuilder-api-staging.onrender.com"

echo "========================================="
echo "DIAGNÓSTICO DE SNAPSHOT SYSTEM"
echo "========================================="
echo ""

echo "1. VERIFICANDO ESTADO DE CONFIGURACIÓN Y BD"
echo "-------------------------------------------"
curl -s "$API_URL/api/diagnostic/snapshot-status" | python3 -m json.tool 2>/dev/null || curl -s "$API_URL/api/diagnostic/snapshot-status"

echo ""
echo ""
echo "2. CREANDO SNAPSHOT DE PRUEBA (Page ID 1)"
echo "-----------------------------------------"
curl -s -X POST "$API_URL/api/diagnostic/test-snapshot/1" | python3 -m json.tool 2>/dev/null || curl -s -X POST "$API_URL/api/diagnostic/test-snapshot/1"

echo ""
echo ""
echo "3. VERIFICANDO ESTADO DESPUÉS DE PRUEBA"
echo "---------------------------------------"
curl -s "$API_URL/api/diagnostic/snapshot-status" | python3 -m json.tool 2>/dev/null || curl -s "$API_URL/api/diagnostic/snapshot-status"

echo ""
echo "========================================="
echo "FIN DE DIAGNÓSTICO"
echo "========================================="