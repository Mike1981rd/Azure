# create-template-section.ps1 - PowerShell version
# Script para crear SECCIONES DEL TEMPLATE para el Website Builder

param(
    [Parameter(Mandatory=$true)]
    [string]$ModuleName,
    
    [switch]$WithChildren
)

# Colores para output
function Write-ColorOutput($ForegroundColor) {
    $fc = $host.UI.RawUI.ForegroundColor
    $host.UI.RawUI.ForegroundColor = $ForegroundColor
    $input | Write-Output
    $host.UI.RawUI.ForegroundColor = $fc
}

# Variables
$MODULE = $ModuleName
$MODULE_LOWER = $MODULE.ToLower()
$MODULE_UPPER = $MODULE.ToUpper()
$BASE_DIR = "websitebuilder-admin/src/components/editor/modules"
$PREVIEW_DIR = "websitebuilder-admin/src/components/preview"

# Verificar si el módulo ya existe
if (Test-Path "$BASE_DIR/$MODULE") {
    Write-Host "❌ Error: El módulo $MODULE ya existe" -ForegroundColor Red
    exit 1
}

Write-Host "🚀 Creando SECCIÓN DEL TEMPLATE: $MODULE" -ForegroundColor Green
Write-Host "📂 Ubicación: $BASE_DIR/$MODULE" -ForegroundColor Blue

if ($WithChildren) {
    Write-Host "👶 Con soporte para elementos hijos" -ForegroundColor Yellow
}

# Crear estructura de carpetas
Write-Host "`n📁 Creando estructura de carpetas..." -ForegroundColor Yellow
New-Item -ItemType Directory -Path "$BASE_DIR/$MODULE" -Force | Out-Null

# Función para crear archivo desde template
function Create-FromTemplate {
    param(
        [string]$TemplateName,
        [string]$OutputPath,
        [string]$Description
    )
    
    Write-Host "  ✅ Creando $Description" -ForegroundColor Green
    
    # Leer template (asumiendo que los templates están en una carpeta)
    $templatePath = ".templates/$TemplateName"
    
    if (-not (Test-Path $templatePath)) {
        # Si no hay template, crear archivo básico
        $content = @"
// $Description
// TODO: Implement $MODULE module

import React from 'react';

export default function ${MODULE}${TemplateName.Replace('Template', '')}() {
    return (
        <div>
            <h2>$MODULE ${TemplateName.Replace('Template', '')}</h2>
            {/* TODO: Implement */}
        </div>
    );
}
"@
    } else {
        $content = Get-Content $templatePath -Raw
    }
    
    # Reemplazar placeholders
    $content = $content -replace '\[MODULE\]', $MODULE
    $content = $content -replace '\[module\]', $MODULE_LOWER
    $content = $content -replace '\[MODULE_UPPER\]', $MODULE_UPPER
    
    # Guardar archivo
    Set-Content -Path $OutputPath -Value $content -Encoding UTF8
}

# Crear archivos principales
Write-Host "`n📝 Generando archivos del módulo..." -ForegroundColor Yellow

# types.ts
Create-FromTemplate -TemplateName "types" -OutputPath "$BASE_DIR/$MODULE/types.ts" -Description "types.ts"

# Editor principal
Create-FromTemplate -TemplateName "Editor" -OutputPath "$BASE_DIR/$MODULE/${MODULE}Editor.tsx" -Description "${MODULE}Editor.tsx"

# Si tiene hijos, crear archivos adicionales
if ($WithChildren) {
    Create-FromTemplate -TemplateName "Children" -OutputPath "$BASE_DIR/$MODULE/${MODULE}Children.tsx" -Description "${MODULE}Children.tsx"
    Create-FromTemplate -TemplateName "ItemEditor" -OutputPath "$BASE_DIR/$MODULE/${MODULE}ItemEditor.tsx" -Description "${MODULE}ItemEditor.tsx"
}

# Preview unificado
Create-FromTemplate -TemplateName "Preview" -OutputPath "$PREVIEW_DIR/Preview${MODULE}.tsx" -Description "Preview${MODULE}.tsx"

# index.ts
$indexContent = @"
export { default as ${MODULE}Editor } from './${MODULE}Editor';
"@

if ($WithChildren) {
    $indexContent += @"
export { default as ${MODULE}Children } from './${MODULE}Children';
export { default as ${MODULE}ItemEditor } from './${MODULE}ItemEditor';
"@
}

$indexContent += @"
export * from './types';
"@

Set-Content -Path "$BASE_DIR/$MODULE/index.ts" -Value $indexContent -Encoding UTF8
Write-Host "  ✅ Creando index.ts" -ForegroundColor Green

# Resumen final
Write-Host "`n✨ MÓDULO $MODULE CREADO EXITOSAMENTE!" -ForegroundColor Green
Write-Host "`n📋 Archivos generados:" -ForegroundColor Yellow
Write-Host "  ✓ $BASE_DIR/$MODULE/types.ts"
Write-Host "  ✓ $BASE_DIR/$MODULE/${MODULE}Editor.tsx"

if ($WithChildren) {
    Write-Host "  ✓ $BASE_DIR/$MODULE/${MODULE}Children.tsx"
    Write-Host "  ✓ $BASE_DIR/$MODULE/${MODULE}ItemEditor.tsx"
}

Write-Host "  ✓ $PREVIEW_DIR/Preview${MODULE}.tsx"
Write-Host "  ✓ $BASE_DIR/$MODULE/index.ts"

Write-Host "`n🎯 Próximos pasos:" -ForegroundColor Blue
Write-Host "  1. Revisar y personalizar los archivos generados"
Write-Host "  2. Integrar en EditorSidebarWithDnD.tsx"
Write-Host "  3. Integrar en ConfigPanel.tsx"
Write-Host "  4. Integrar en EditorPreview.tsx"
Write-Host "  5. Integrar en PreviewContent.tsx"
Write-Host "  6. Agregar tipo en editor.types.ts"

Write-Host "`n💡 RECORDATORIO:" -ForegroundColor Yellow
Write-Host "  • Botón 'Add block' debe ser AZUL (text-blue-600)"
Write-Host "  • Flecha de regreso va al SIDEBAR (selectSection(null))"
Write-Host "  • Drag handle solo visible on hover"
Write-Host "  • Items con chevron > icono > título"

Write-Host "`n✅ Script completado exitosamente" -ForegroundColor Green