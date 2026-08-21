# WASSLink Architecture

> Documento oficial de arquitectura del proyecto.

## Arquitectura general

WASSLink Studio utiliza una arquitectura modular basada en capas.

La solución se divide en:

- Apps
- Core
- Tests

---

# Apps

Las Apps contienen los puntos de entrada de usuario.

Proyectos actuales:

- WASSLink.Desktop (Avalonia UI)
- WASSLink.CLI (consola)

Proyecto futuro:

- WASSLink.Mobile (Android / futuro)

Las Apps son responsables de:

- Interfaz de usuario.
- Entrada del usuario.
- Presentación.
- Composición de servicios.

Las Apps no deben contener lógica de negocio central.

Las Apps inyectan dependencias del Core mediante DI. No instancian servicios Core con new().

---

# Core

El Core contiene la lógica reutilizable del ecosistema.

## WASSLink.Abstractions

Responsabilidad:

- Interfaces.
- Contratos.
- Definiciones compartidas.

Dependencias:

- Ninguna.

Contratos implementados (Beta 1):

- IDownloadProvider
- DownloadRequest
- DownloadProgress

---

## WASSLink.Shared

Responsabilidad:

- Utilidades comunes.
- Extensiones.
- Modelos base.
- Logging (WassLinkLogger).
- Diagnósticos (DiagnosticReportGenerator).

Dependencias:

- WASSLink.Abstractions

---

## WASSLink.Configuration

Responsabilidad:

- Configuración.
- Preferencias.
- Carga y almacenamiento de configuración.

Dependencias:

- WASSLink.Abstractions
- WASSLink.Shared

Estado: scaffold (Beta 2).

---

## WASSLink.Download

Responsabilidad:

- Gestión de descargas.
- Abstracción de motores de descarga.
- Integración con proveedores y motores externos.

Dependencias:

- WASSLink.Abstractions
- WASSLink.Configuration
- WASSLink.Shared

Implementaciones actuales (Beta 1):

- YtDlpDownloadService — implementa IDownloadProvider.

Capacidades futuras contempladas:

- HTTP/HTTPS (Beta 1 extendido).
- MediaFire, MEGA (Beta 5 — experimental).
- Torrents (Beta 8).

La arquitectura no debe acoplar el módulo directamente a un motor externo concreto.

---

## WASSLink.Library

Responsabilidad:

- Biblioteca multimedia.
- Organización.
- Metadatos.
- Gestión del contenido descargado.

Dependencias:

- WASSLink.Abstractions
- WASSLink.Shared
- WASSLink.Configuration

Estado: scaffold (Beta 4).

---

## WASSLink.Player

Responsabilidad:

- Reproducción multimedia.
- Control del reproductor.
- Integración con motores multimedia.

Dependencias:

- WASSLink.Abstractions
- WASSLink.Shared

Estado: scaffold (Beta 3).

---

## WASSLink.Search

Responsabilidad:

- Búsqueda.
- Proveedores de búsqueda.
- Resultados.

Dependencias:

- WASSLink.Abstractions
- WASSLink.Shared
- WASSLink.Plugins

Estado: scaffold (Beta 2).

---

## WASSLink.Plugins

Responsabilidad:

- Sistema de extensiones.
- Contratos de plugins.
- Registro y descubrimiento de extensiones.

Dependencias:

- WASSLink.Abstractions
- WASSLink.Shared

Estado: scaffold (Beta 6).

---

# Tests

Los proyectos de Tests validan el comportamiento del Core y sus integraciones.

Las pruebas no deben introducir dependencias innecesarias hacia las Apps.

Framework: xUnit.

---

# Regla de dependencias

Permitido:

Apps → Core

Core → Core inferior

Tests → proyectos que prueban

No permitido:

Core → Apps

Core → UI

Core → implementaciones externas concretas cuando exista una abstracción adecuada

---

# Flujo principal (Beta 1)

Usuario
↓
WASSLink.Desktop (App)
↓
MainViewModel (IDownloadProvider via DI)
↓
YtDlpDownloadService (Core/Download)
↓
yt-dlp.exe (motor externo)
↓
Archivo de salida (carpeta local)

---

# Flujo principal (visión completa)

URL / Fuente
↓
Source Detection
↓
Provider (IDownloadProvider)
↓
Download Engine (yt-dlp, HTTP, Torrent...)
↓
Processing (FFmpeg)
↓
Organization (carpetas, metadatos)
↓
Library / Player

---

# Providers

Los proveedores implementan IDownloadProvider.

Un provider puede informar sobre sus capacidades:

- CanHandle(url) — si puede gestionar la URL dada.
- (futuro) CanResume — si soporta reanudación.
- (futuro) CanSegment — si soporta descarga segmentada.
- (futuro) CanExtractMetadata — si puede extraer metadatos.

La arquitectura no obliga a implementar todas las capacidades desde el principio.

---

# Descarga segmentada (futuro — Beta 7)

Para archivos grandes, WASSLink debe detectar las capacidades del servidor:

URL
↓
HEAD / metadata
↓
¿Acepta Range Requests?
├── Sí → descarga segmentada posible
└── No → descarga convencional

NO asumir que todo archivo puede dividirse.

La arquitectura de Beta 1 no debe impedir esta implementación futura.

---

# Plugins

Los plugins implementan contratos definidos en WASSLink.Abstractions.

Los módulos internos no deben depender directamente de plugins concretos.

Esto permite:

- Extensibilidad.
- Bajo acoplamiento.
- Sustitución de proveedores.
- Integración futura de nuevos mecanismos de descarga.

---

# Principio fundamental

El Core define QUÉ debe hacerse.

Las implementaciones externas definen CÓMO se realiza.

Esto permite evolucionar WASSLink Studio sin reconstruir la arquitectura alrededor de una tecnología específica.

---

# Estado

Version: 1.2

Fecha: 2026-08-21

Fase 1: completada.

Beta 1: en curso.
