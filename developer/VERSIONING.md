# GowLink Studio — Convención de versiones

## 1. Principio

El proyecto necesita distinguir claramente entre:

- versión preliminar
- versión beta funcional
- release final de una etapa
- corrección puntual de una etapa

Esto evita que una mejora local se convierta en una promesa de producto mayor.

## 2. Convención recomendada

- `1.5.0-beta.1` — primera pre-release de una etapa
- `1.5.0-beta.2` — segunda pre-release
- `1.5.0` — release final de la etapa
- `1.5.1` — corrección de la etapa
- `1.5.2` — corrección posterior

## 3. Relación con la etiqueta humana

La etiqueta humana “Beta 1.5” puede usarse para comunicación interna, pero debe distinguirse del identificador formal del paquete o release.

## 4. Regla de uso

- no se asume que 1.4.1 es un número “automático” siguiente sin verificar el historial real
- no se debe convertir una preparación mensual en una fecha que obligue a publicar antes de cumplir criterios
- no se deben anunciar versiones finales si aún no se han verificado los criterios de salida

## 5. Política de publicación

El objetivo organizativo es una publicación grande aproximadamente mensual, pero la fecha no debe imponerse a la calidad.

Cada release debe incluir:

- alcance definido
- criterios de aceptación
- pruebas ejecutadas
- limitaciones documentadas
- notas de release verificadas

## 6. Recomendación para la etapa actual

La etapa actual se debe manejar como:

- 1.4.1 — Consolidation Release

No como una promesa de nueva funcionalidad, sino como consolidación de la etapa existente antes de abrir la siguiente fase de crecimiento funcional.
