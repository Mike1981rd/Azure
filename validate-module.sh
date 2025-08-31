#!/bin/bash
# validate-module.sh - Script de validación automática de módulos

FILE=$1
MAX_LINES=300
WARNING_THRESHOLD=250

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

if [ -z "$FILE" ]; then
    echo -e "${RED}❌ ERROR: Debes especificar un archivo${NC}"
    echo "Uso: ./validate-module.sh [archivo]"
    exit 1
fi

if [ ! -f "$FILE" ]; then
    echo -e "${GREEN}✅ Archivo nuevo - se creará con estructura correcta${NC}"
    echo -e "${GREEN}📁 Recuerda usar: components/editor/modules/[ModuleName]/${NC}"
    exit 0
fi

LINES=$(wc -l < "$FILE")

if [ $LINES -gt $MAX_LINES ]; then
    echo -e "${RED}❌ ERROR: Archivo tiene $LINES líneas (máximo: $MAX_LINES)${NC}"
    echo -e "${RED}🛑 DETENIDO: No se puede agregar código a este archivo${NC}"
    echo -e "${GREEN}✅ SOLUCIÓN: Crear nuevo módulo en components/editor/modules/${NC}"
    echo ""
    echo "Estructura recomendada:"
    echo "  components/editor/modules/[NuevoModulo]/"
    echo "  ├── [NuevoModulo]Editor.tsx"
    echo "  ├── [NuevoModulo]Preview.tsx"
    echo "  ├── [NuevoModulo]Config.tsx"
    echo "  ├── [NuevoModulo]Types.ts"
    echo "  └── index.ts"
    exit 1
elif [ $LINES -gt $WARNING_THRESHOLD ]; then
    echo -e "${YELLOW}⚠️ ADVERTENCIA: Archivo tiene $LINES líneas${NC}"
    echo -e "${YELLOW}📊 Quedan solo $((MAX_LINES - LINES)) líneas disponibles${NC}"
    echo -e "${YELLOW}💡 Considera crear un módulo separado${NC}"
    exit 0
else
    echo -e "${GREEN}✅ Archivo tiene $LINES líneas${NC}"
    echo -e "${GREEN}📊 Puedes agregar hasta $((MAX_LINES - LINES)) líneas más${NC}"
    exit 0
fi