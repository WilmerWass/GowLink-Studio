# WASSLink Project Manifest

> Documento fundacional del proyecto.
> Este documento define la identidad, visión, misión, principios y decisiones arquitectónicas de WASSLink.
> Ninguna decisión importante deberá contradecir este documento sin una revisión formal.

---

# Información General

| Campo | Valor |
|--------|-------|
| Proyecto | WASSLink |
| Nombre Comercial | WASSLink Studio |
| Ediciones | Studio, CLI y Mobile (futuro) |
| Slogan Oficial | Portable Media Center |
| Estado | En desarrollo |
| Versión del Manifest | 1.0.1 |
| Versión del Proyecto | 0.1.0-alpha |
| Repositorio Oficial | https://github.com/WilmerWass/WASSLink_MPDL |
| Licencia | MIT (Pendiente de confirmar) |
| Product Owner | WilmerWass |
| Arquitecto de Software (Asistencia IA) | ChatGPT GPT-5.5

---

# Identidad

WASSLink es una plataforma multimedia modular, diseñada para centralizar la búsqueda, descarga, organización, reproducción y administración de contenido multimedia desde una única aplicación.

El proyecto adopta la filosofía Portable First y está pensado para evolucionar hacia un ecosistema multiplataforma compuesto por Studio, CLI y futuras aplicaciones móviles.

---

# Visión

Convertir a WASSLink en el Centro Multimedia más completo, intuitivo y extensible para Windows, diseñado bajo una filosofía **Portable First**, donde el usuario tiene el control para utilizarlo como aplicación portable o instalada, permitiéndole descubrir, descargar, organizar, reproducir y administrar su biblioteca multimedia desde un único lugar mediante una arquitectura moderna, modular y preparada para evolucionar hacia un ecosistema multiplataforma.

---

# Misión

Desarrollar una plataforma multimedia moderna, abierta y extensible que permita a cualquier usuario descubrir, descargar, organizar, reproducir y administrar contenido multimedia de forma sencilla, segura y eficiente, priorizando la portabilidad, la automatización y el control total del usuario sobre su biblioteca.

---

# Propuesta de Valor

WASSLink busca integrar en una única plataforma las funciones que normalmente requieren múltiples aplicaciones.

El usuario podrá:

- Buscar contenido multimedia.
- Descargar desde diferentes plataformas mediante plugins.
- Organizar automáticamente su biblioteca.
- Reproducir audio y video.
- Gestionar metadatos.
- Crear colecciones y playlists.
- Trabajar tanto en modo portable como instalado.

Todo ello bajo una arquitectura modular preparada para crecer durante muchos años.

---

# Principios Fundamentales

1. El usuario siempre tiene el control.

2. Portable First.

3. La simplicidad prevalece sobre la complejidad.

4. Modular antes que monolítico.

5. La documentación es parte del software.

6. La automatización debe ahorrar tiempo al usuario.

7. La privacidad del usuario es prioritaria.

8. La compatibilidad es una responsabilidad, no una opción.

9. Cada módulo debe tener una única responsabilidad.

10. El proyecto debe poder mantenerse durante muchos años.

11. Las decisiones importantes deben registrarse mediante ADR.

12. La colaboración entre personas e Inteligencia Artificial debe ser transparente.

13. WASSLink será una plataforma multimedia abierta, modular y neutral.

14. Las funcionalidades específicas de plataformas externas deberán implementarse mediante plugins siempre que sea técnicamente posible.

15. Todo cambio importante deberá estar documentado antes de implementarse.
---

| Categoría | Tecnología |
|-----------|------------|
| Lenguaje | C# |
| Framework | .NET 10 LTS (o .NET 9) |
| UI | Avalonia UI |
| Base de Datos | SQLite |
| Descargas | yt-dlp |
| Multimedia | FFmpeg |
| Reproductor | LibVLC |
| Arquitectura | Dependency Injection |
| MVVM | CommunityToolkit.Mvvm |
| Logging | Serilog |
| Control de Versiones | Git |
| Repositorio | GitHub |

---

# Arquitectura General

La solución se construirá mediante módulos independientes organizados por responsabilidad.

Los proyectos oficiales serán:

- WASSLink.Abstractions
- WASSLink.Shared
- WASSLink.Configuration
- WASSLink.Download
- WASSLink.Library
- WASSLink.Player
- WASSLink.Search
- WASSLink.Plugins
- WASSLink.Studio
- WASSLink.CLI
- WASSLink.Tests

La arquitectura completa se documenta en ARCHITECTURE.md.

---

# Objetivos a Corto plazo

- Crear la solución .NET.
- Implementar Studio y CLI.
- Integrar yt-dlp.
- Integrar FFmpeg.
- Crear Biblioteca Multimedia.
- Publicar Alpha.
---

# Objetivos a Mediano Plazo

Sistema de Plugins.
Reproductor Multimedia.
Organización automática.
Búsqueda integrada.
Beta pública.

---

# Objetivos a Largo Plazo

Aplicación Android.
Sincronización.
IA para organización multimedia.
Versión 1.0.
Ecosistema multiplataforma.

---

# Filosofía del Proyecto

WASSLink no es un simple descargador.

Es una plataforma multimedia modular, abierta y extensible, diseñada para simplificar la gestión de contenido multimedia respetando siempre el control del usuario, la transparencia y una arquitectura limpia.

---

# Decisiones Aprobadas

| ADR | Estado |
|------|--------|
| ADR-001 Portable First | ✅ |
| ADR-002 Arquitectura Modular | ✅ |
| ADR-003 Neutralidad de Plataforma | ✅ |
| ADR-004 Colaboración con IA | ⏳ |

---

# Historial de Revisiones

| Versión | Fecha | Cambios |
|----------|--------|---------|
| 1.0.0 | 2026-08-03 | Creación del Manifest |
| 1.0.1 | 2026-08-03 | Actualización de principios y estructura |
