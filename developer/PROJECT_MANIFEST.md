# WASSLink Studio

## Identidad del proyecto

WASSLink Studio es una plataforma multimedia modular orientada a la gestiÃ³n de contenido digital.

El producto principal en desarrollo activo es:

**WASSLink Downloader**

Lema:

"Descarga lo que quieras, cuando quieras y como quieras."

El lema representa libertad de uso, no una promesa tÃ©cnica absoluta ni autorizaciÃ³n para evadir restricciones.

## Ecosistema WASSLink Studio

WASSLink Studio integra progresivamente:

WASSLink Studio
â”‚
â”œâ”€â”€ WASSLink Downloader   (Beta 1 â€” en curso)
â”œâ”€â”€ WASSLink Search       (Beta 2 â€” planificado)
â”œâ”€â”€ WASSLink Player       (Beta 3 â€” planificado)
â”œâ”€â”€ WASSLink Library      (Beta 4 â€” planificado)
â”œâ”€â”€ WASSLink Files        (Beta 7 â€” planificado)
â”œâ”€â”€ WASSLink Torrents     (Beta 8 â€” planificado)
â””â”€â”€ WASSLink Plugins      (Beta 6 â€” planificado)

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

El reproductor multimedia es una parte fundamental de WASSLink Studio.

Permite que el usuario reproduzca contenido gestionado por la plataforma sin depender de una aplicaciÃ³n externa.

### Biblioteca (Beta 4)

La biblioteca organiza el contenido descargado y sus metadatos.

### Plugins (Beta 6)

El sistema de plugins permite ampliar las capacidades del producto sin acoplar el Core a implementaciones concretas.

## Arquitectura

La soluciÃ³n utiliza una arquitectura modular:

- Apps (WASSLink.Desktop, WASSLink.CLI)
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

## Estado actual

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

Beta 1 (Download Studio): en curso.

Estado de Beta 1:

- IMPLEMENTADO: Contrato IDownloadProvider y modelos en WASSLink.Abstractions (B1.1)
- IMPLEMENTADO: Inyección de dependencias completa sin new() en ViewModels (B1.1)
- IMPLEMENTADO: UI moderna con barra lateral y secciones futuras (B1.1)
- IMPLEMENTADO: YtDlpDownloadService con IDownloadProvider y cancelación limpia (B1.1)
- IMPLEMENTADO: WassLinkLogger y DiagnosticReportGenerator
- EN CURSO: Descarga real de YouTube y validación de flujo completo (B1.2)
- PLANIFICADO: Progreso y cancelación robustos en descarga pesada (B1.3)
- PLANIFICADO: Organización de archivos y carpetas configurables (B1.4)
- PLANIFICADO: Manejo exhaustivo de errores y modo offline (B1.5)

## Principio de evoluciÃ³n

WASSLink Studio se desarrolla de forma incremental, por betas.

Una capacidad se considera implementada Ãºnicamente cuando existe cÃ³digo funcional, integraciÃ³n y validaciÃ³n.

La documentaciÃ³n puede describir capacidades futuras siempre que estÃ©n claramente identificadas como planificadas.

