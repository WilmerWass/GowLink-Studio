# GowLink Studio

## Identidad del proyecto

GowLink Studio es una suite modular orientada a gestionar contenido digital. WASSLink Studio es el nombre anterior del producto.

El producto principal en desarrollo activo es:

**GowLink Downloader**

Lema:

"Descarga lo que quieras, cuando quieras y como quieras."

El lema representa libertad de uso, no una promesa tÃ©cnica absoluta ni autorizaciÃ³n para evadir restricciones.

## Ecosistema GowLink Studio

GowLink Studio integra progresivamente:

GowLink Studio
â”‚
â”œâ”€â”€ GowLink Downloader   (Beta 1 â€” estabilización)
â”œâ”€â”€ GowLink Search       (Beta 2 â€” planificado)
â”œâ”€â”€ GowLink Player       (Beta 3 â€” planificado)
â”œâ”€â”€ GowLink Library      (Beta 4 â€” planificado)
â”œâ”€â”€ GowLink Files        (Beta 7 â€” planificado)
â”œâ”€â”€ GowLink Torrents     (Beta 8 â€” planificado)
â””â”€â”€ GowLink Plugins      (Beta 6 â€” planificado)

## Flujo principal (visiÃ³n completa)

URL / Fuente
â†“
Source Detection
â†“
Provider
â†“
Download Engine
â†“
Processing
â†“
Organization
â†“
Library / Player

## Capacidades principales

### Descargas (Beta 1)

La arquitectura contempla una capa de descarga extensible.

Proveedor inicial:

- yt-dlp (YouTube â€” Beta 1, implementado)

Proveedores futuros contemplados:

- HTTP/HTTPS directos (Beta 1 extendido)
- MediaFire (Beta 5 â€” experimental)
- MEGA (Beta 5 â€” experimental)
- Torrents (Beta 8)
- Proveedores adicionales mediante plugins (Beta 6)

La compatibilidad con cada proveedor se considera planificada hasta que exista implementaciÃ³n funcional y pruebas.

### BÃºsqueda (Beta 2)

Motor de bÃºsqueda integrado.

### Reproductor (Beta 3)

El reproductor multimedia es una capacidad futura de GowLink Studio.

Permite que el usuario reproduzca contenido gestionado por la plataforma sin depender de una aplicaciÃ³n externa.

### Biblioteca (Beta 4)

La biblioteca organiza el contenido descargado y sus metadatos.

### Plugins (Beta 6)

El sistema de plugins permite ampliar las capacidades del producto sin acoplar el Core a implementaciones concretas.

## Arquitectura

La soluciÃ³n utiliza una arquitectura modular:

- Apps (proyectos técnicos `WASSLink.Desktop` y `WASSLink.CLI`, presentados como GowLink Desktop y GowLink CLI)
- Core (contratos, servicios, lÃ³gica reutilizable)
- Tests

Las aplicaciones finales consumen servicios del Core mediante Dependency Injection.

El Core contiene contratos, servicios y lÃ³gica reutilizable.

Los mÃ³dulos Core no deben depender de implementaciones concretas cuando exista una abstracciÃ³n adecuada.

## TecnologÃ­as

TecnologÃ­as principales:

- .NET 10
- C#
- Avalonia UI
- SQLite (futuro)
- FFmpeg
- yt-dlp
- LibVLC / MPV (futuro)
- Microsoft.Extensions.DependencyInjection

Las tecnologÃ­as externas pueden cambiar durante la evoluciÃ³n del proyecto.

## Estado actual — Beta 1.3

Fase 1: completada.

La Fase 1 estableciÃ³:

- Estructura de soluciÃ³n.
- Proyectos Core.
- Aplicaciones Desktop y CLI.
- Tests.
- Arquitectura inicial.
- Referencias entre proyectos.
- Infraestructura inicial de Dependency Injection.
- CompilaciÃ³n completa de la soluciÃ³n.

Beta 1 (GowLink Downloader): estabilización en curso. Quality Grid, selección de formatos, progreso/cancelación y UI principal están implementados; los 20 tests actuales pasan. Una ejecución runtime verificó la descarga recomendada de audio M4A y vídeo MP4 1080p, con progreso, postprocesamiento FFmpeg y archivo de salida. La validación no cierra el trabajo de estabilización ni cubre todos los fallos de producción.

Estado y pendientes:

- IMPLEMENTADO: Contrato IDownloadProvider y modelos en el proyecto técnico `WASSLink.Abstractions` (B1.1)
- IMPLEMENTADO: Inyección de dependencias completa sin new() en ViewModels (B1.1)
- IMPLEMENTADO: UI GowLink Desktop con barra lateral y secciones futuras (B1.1)
- IMPLEMENTADO: YtDlpDownloadService con IDownloadProvider y cancelación limpia (B1.1)
- IMPLEMENTADO: Quality Grid, inspección de metadatos y selección de formato (B1.2)
- IMPLEMENTADO: `GowLinkLogger` y generación de informes de diagnóstico; `WassLinkLogger` se conserva como alias
- EN CURSO: Estabilización Beta 1.3; ampliar la validación de errores y cancelación y completar el rebranding técnico de forma coordinada
- IMPLEMENTADO Y VALIDADO EN RUNTIME: `LoggerService`, logger compartido e informes de diagnóstico en `%LOCALAPPDATA%\GowLink\Logs`; Desktop creó `app.log` y `gowlink-2026-10-05.log`.
- PENDIENTE: Migración técnica coordinada de solución, proyectos y namespaces `WASSLink.*`; no se ha realizado un cambio parcial
- VALIDADO EN RUNTIME: Inspección de la URL autorizada `https://www.youtube.com/watch?v=7Ne9cbREOnk`: título “Si te tengo a ti, lo tengo todo - Marcos Brunet ｜ Adoración”, duración 5:11 y 7 formatos. Audio M4A y vídeo MP4 1080p se descargaron y decodificaron correctamente; `app.log` registró la ruta final exacta.
- PLANIFICADO: Carpeta de salida configurable y organización adicional (B1.4)
- PLANIFICADO: Robustez ampliada de errores y comportamiento offline (B1.5)

## Principio de evoluciÃ³n

GowLink Studio se desarrolla de forma incremental, por betas.

Una capacidad se considera implementada Ãºnicamente cuando existe cÃ³digo funcional, integraciÃ³n y validaciÃ³n.

La documentaciÃ³n puede describir capacidades futuras siempre que estÃ©n claramente identificadas como planificadas.
