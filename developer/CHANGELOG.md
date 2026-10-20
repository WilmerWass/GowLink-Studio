# GowLink Studio — Changelog

## 1. Estado del changelog

Este changelog registra hitos verificados y cambios relevantes de la arquitectura y la experiencia del producto. Mantiene la separación entre:

- cambios ya verificados
- mejoras de consolidación
- nuevas capacidades en planificación
- evolución futura del producto

## 2. Beta 1.4 — Universal Download Engine

Estado: hito previo de la arquitectura dual.

Cambios relevantes:

- branding actualizado a GowLink Studio
- ruta predeterminada de descargas en `Downloads\GowLink`
- navegación por módulos: Descargar, Biblioteca, Buscador, Historial y Configuración
- indicador dinámico del motor activo según la URL
- soporte dual de motores: HTTP directo y yt-dlp
- mejora de la pausa HTTP sin bloquear la interfaz
- validación de compilación con `dotnet build -c Debug`
- validación de pruebas con `dotnet test tests\WASSLink.Tests\WASSLink.Tests.csproj -c Debug --no-restore`

Observación:

Esta etapa debe entenderse como consolidación del producto actual y preparación para 1.5, no como cierre de todas las capacidades del proyecto.

## 3. Beta 1.4.1 — Consolidation Release

Estado: en consolidación; no publicada.

Objetivo:

Consolidar la base actual del motor dual, la navegación principal y la experiencia visible antes de abrir la siguiente etapa funcional.

Cambios implementados y pendientes de validación de publicación:

- cola FIFO común con concurrencia de 1–3 y hasta tres intentos totales (esperas de 1 y 2 segundos)
- temporales HTTP para evitar archivos finales parciales
- persistencia de preferencias e historial terminal bajo `%LOCALAPPDATA%\GowLink`
- saneamiento de query y fragmento en URLs de logs e historial
- selector de conflictos HTTP, mejoras de navegación y recursos dinámicos de acento
- pruebas de regresión y validación del paquete portable antes de publicar

## 4. 1.5 — Search & Providers

Estado: planificado como etapa funcional nueva.

Objetivo:

Formalizar la detección de proveedores y la resolución del contenido.

## 5. 1.6 — Media Studio

Estado: planificado.

Objetivo:

Desarrollar la base funcional del entorno multimedia.

## 6. 1.7 — File Studio

Estado: planificado.

Objetivo:

Extender el motor HTTP con reanudación, rangos y manejo avanzado de archivos grandes.

## 7. 1.8 — Torrent Studio

Estado: planificado.

Objetivo:

Introducir un motor independiente para torrents.

## 8. 1.9 — Integration Studio

Estado: planificado.

Objetivo:

Integrar motores, cola, biblioteca, reproducción, configuración y pruebas en una plataforma coherente.

## 9. Convención de registro

Cada entradade changelog debe distinguir entre:

- verificado
- parcialmente verificado
- planificado
- experimental

El historial completo debe reflejar la diferencia real entre una mejora de estabilidad y una nueva capacidad funcional.
