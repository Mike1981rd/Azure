#!/bin/bash

API_URL="https://websitebuilder-api-staging.onrender.com"

echo "Generando snapshots para todas las páginas..."
echo ""

for pageId in 2 3 4 5 6 7 8; do
    echo "Página ID $pageId:"
    curl -s -X POST "$API_URL/api/diagnostic/test-snapshot/$pageId" | jq '.success' 2>/dev/null || echo "Failed"
    sleep 1
done

echo ""
echo "Verificando estado final:"
curl -s "$API_URL/api/diagnostic/snapshot-status" | jq '.database' 2>/dev/null || echo "Error"