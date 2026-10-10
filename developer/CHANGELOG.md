# GowLink Studio — Changelog

## 1. Estado del changelog

Este changelog registra hitos verificados y cambios relevantes de la arquitectura y la experiencia del producto. Mantiene la separación entre:

- cambios ya verificados
- mejoras de consolidación
- nuevas capacidades en planificación
- evolución futura del producto

## 2. Beta 1.4 — Universal Download Engine

Estado: consolidación y mejora de la arquitectura actual.

Cambios relevantes:

- branding actualizado a GowLink Studio
- ruta predeterminada de descargas en `Downloads\GowLink`
- navegación por módulos: Downloader, Biblioteca, Buscador, Descargas y Configuración
- indicador dinámico del motor activo según la URL
- soporte dual de motores: HTTP directo y yt-dlp
- mejora de la pausa HTTP sin bloquear la interfaz
- validación de compilación con `dotnet build -c Debug`
- validación de pruebas con `dotnet test tests\WASSLink.Tests\WASSLink.Tests.csproj -c Debug --no-restore`

Observación:

Esta etapa debe entenderse como consolidación del producto actual y preparación para 1.5, no como cierre de todas las capacidades del proyecto.

## 3. 1.4.1 — Consolidation Release

Estado: fase de estabilidad y refinamiento.

Objetivo:

Consolidar la base actual del motor dual, la navegación principal y la experiencia visible antes de abrir la siguiente etapa funcional.

Cambios esperados en esta fase:

- estabilidad del motor HTTP directo
- estabilidad del motor yt-dlp
- configuración más coherente y persistente
- manejo mejorado de errores, pausa y cancelación
- regresiones y validación de UX
- documentación interna y roadmap oficial coherentes

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
