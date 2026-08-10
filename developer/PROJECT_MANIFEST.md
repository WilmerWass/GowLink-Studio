# WASSLink Studio

## Identidad del proyecto

WASSLink Studio es una plataforma multimedia modular orientada a la gestiÃ³n de contenido digital.

El proyecto integra progresivamente:

- Descarga de contenido.
- GestiÃ³n de biblioteca multimedia.
- ReproducciÃ³n multimedia.
- BÃºsqueda y proveedores.
- Sistema de plugins.
- ConfiguraciÃ³n.
- Interfaces Desktop y CLI.
- Futuras capacidades multiplataforma.

La aplicaciÃ³n no se limita a ser un reproductor ni Ãºnicamente un gestor de descargas.

Su objetivo es convertirse en un **Media Studio** capaz de centralizar el flujo:

Usuario
â†“
BÃºsqueda / Entrada
â†“
Descarga
â†“
Biblioteca
â†“
ReproducciÃ³n

## Capacidades principales

### Descargas

La arquitectura contempla una capa de descarga extensible.

Fuentes y mecanismos contemplados:

- HTTP/HTTPS.
- Motores externos.
- Proveedores especializados.
- Torrents mediante una futura integraciÃ³n especializada.

La compatibilidad con torrents es una capacidad arquitectÃ³nica prevista y no debe considerarse implementada hasta que exista una implementaciÃ³n funcional y pruebas correspondientes.

### Biblioteca

La biblioteca organiza el contenido descargado y sus metadatos.

### Reproductor

El reproductor multimedia es una parte fundamental de WASSLink Studio.

Permite que el usuario pueda reproducir contenido gestionado por la plataforma sin depender de una aplicaciÃ³n externa para completar el flujo principal.

### Plugins

El sistema de plugins permite ampliar las capacidades del producto sin acoplar el Core a implementaciones concretas.

## Arquitectura

La soluciÃ³n utiliza una arquitectura modular:

- Apps
- Core
- Tests

Las aplicaciones finales consumen servicios del Core.

El Core contiene contratos, servicios y lÃ³gica reutilizable.

## TecnologÃ­as

TecnologÃ­as principales previstas:

- .NET 10
- C#
- Avalonia UI
- SQLite
- FFmpeg
- yt-dlp
- LibVLC / MPV
- Microsoft.Extensions.DependencyInjection

Las tecnologÃ­as externas pueden cambiar durante la evoluciÃ³n del proyecto.

## Estado actual

Fase 1: completada.

La Fase 1 establece:

- Estructura de soluciÃ³n.
- Proyectos Core.
- Aplicaciones Desktop y CLI.
- Tests.
- Arquitectura inicial.
- Referencias entre proyectos.
- Infraestructura inicial de Dependency Injection.
- CompilaciÃ³n completa de la soluciÃ³n.

## Principio de evoluciÃ³n

WASSLink Studio se desarrolla de forma incremental.

Una capacidad se considera implementada Ãºnicamente cuando existe cÃ³digo funcional, integraciÃ³n y validaciÃ³n.

La documentaciÃ³n puede describir capacidades futuras siempre que estÃ©n claramente identificadas como planificadas.
