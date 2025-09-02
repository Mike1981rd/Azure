# Leer Documentos Base

Genera un resumen o responde basado en documentos clave. Uso: $ARGUMENTS

## Objetivo
Leer y contextualizar los siguientes archivos del repositorio y dar una respuesta fundamentada exclusivamente en su contenido:
- `CLAUDE.md`
- `blueprint1.md`
- `blueprint2.md`
- `blueprint3.md`
- `docs/WEBSITE-BUILDER-MODULE-GUIDE.md`
- `CLAUDEBK1.md`
- `CLAUDEBK2.md`

## Proceso
1. Cargar los archivos listados (en ese orden) y crear un contexto unificado.
2. Si $ARGUMENTS contiene una pregunta/tarea, responder utilizando solo la información de estos documentos.
3. Si $ARGUMENTS está vacío, generar un breve resumen ejecutivo con:
   - Puntos clave por documento (3–5 bullets c/u)
   - Instrucciones/mandatos operativos relevantes (si aplica)
   - Riesgos o advertencias señaladas
4. Incluir una sección final de “Fuentes consultadas” marcando los archivos realmente usados.

## Reglas
- No inventar contenido fuera de las fuentes listadas.
- Referenciar secciones usando títulos o palabras clave del documento cuando sea útil.
- Si algún archivo falta, indicarlo claramente y continuar con los disponibles.
- Mantener respuestas concisas y accionables; usar bullets cuando ayuden a la claridad.

## Formato de salida sugerido
```
📚 Resumen/Respuesta basada en documentos

🎯 Solicitud: [copiar $ARGUMENTS o “Resumen general”]

— Síntesis por documento —
- CLAUDE.md: [...]
- Blueprint 1: [...]
- Blueprint 2: [...]
- Blueprint 3: [...]
- Module Guide: [...]
- ClaudeBK1: [...]
- ClaudeBK2: [...]

— Recomendaciones/Acciones —
- [...]

— Fuentes consultadas —
- [x] CLAUDE.md
- [x] blueprint1.md
- [x] blueprint2.md
- [x] blueprint3.md
- [x] docs/WEBSITE-BUILDER-MODULE-GUIDE.md
- [x] CLAUDEBK1.md
- [x] CLAUDEBK2.md
```

