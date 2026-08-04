# WASSLink Architecture

> Documento oficial de arquitectura del proyecto.
> Define la estructura, responsabilidades y dependencias entre módulos.
>
> Toda nueva funcionalidad debe respetar las reglas definidas en este documento.

---

# Arquitectura General

WASSLink utiliza una arquitectura modular basada en capas.

La solución está dividida en:

- Apps
- Core
- Tests

La arquitectura está diseñada para permitir crecimiento a largo plazo, mantener bajo acoplamiento y facilitar la incorporación de nuevas funcionalidades mediante módulos y plugins.

---

# Capas del Sistema

# Apps

Contienen las aplicaciones finales utilizadas por los usuarios.

Proyectos:

- WASSLink.Desktop
- WASSLink.CLI
- WASSLink.Mobile (futuro)

Responsabilidad:

- Interfaz de usuario.
- Entrada del usuario.
- Presentación de información.
- Coordinación de servicios del Core.

Las aplicaciones no contienen lógica de negocio.

---

# Core

Contiene la lógica reutilizable del ecosistema WASSLink.

El Core es independiente de las interfaces de usuario.

---

# WASSLink.Abstractions

Responsabilidad:

- Interfaces.
- Contratos.
- Definiciones compartidas.
- Modelos base de comunicación entre módulos.

Dependencias:

- Ninguna.

Regla:

Este proyecto debe permanecer independiente y estable.

---

# WASSLink.Shared

Responsabilidad:

- Utilidades comunes.
- Extensiones.
- Componentes reutilizables.
- Modelos compartidos.

Dependencias:

- WASSLink.Abstractions

Regla:

Shared no debe convertirse en un contenedor de lógica de negocio.

---

# WASSLink.Configuration

Responsabilidad:

- Configuración global.
- Preferencias del usuario.
- Lectura y escritura de configuración.
- Gestión de rutas del sistema.

Dependencias:

- WASSLink.Abstractions
- WASSLink.Shared

---

# WASSLink.Download

Responsabilidad:

- Gestión de descargas.
- Control de motores externos.
- Procesamiento inicial de contenido descargado.

Integraciones futuras:

- yt-dlp
- FFmpeg

Dependencias:

- WASSLink.Abstractions
- WASSLink.Configuration
- WASSLink.Shared

---

# WASSLink.Library

Responsabilidad:

- Administración de biblioteca multimedia.
- Organización de archivos.
- Gestión de metadatos.
- Colecciones y categorías.

Dependencias:

- WASSLink.Abstractions
- WASSLink.Shared
- WASSLink.Configuration

---

# WASSLink.Player

Responsabilidad:

- Reproducción multimedia.
- Control del reproductor.
- Gestión de sesiones de reproducción.

Integraciones futuras:

- LibVLC
- MPV

Dependencias:

- WASSLink.Abstractions
- WASSLink.Shared

---

# WASSLink.Search

Responsabilidad:

- Búsqueda de contenido.
- Gestión de resultados.
- Coordinación de proveedores de búsqueda.

Dependencias:

- WASSLink.Abstractions
- WASSLink.Shared

Regla:

Search no depende directamente de plugins.

Los proveedores externos deben comunicarse mediante contratos definidos en Abstractions.

---

# WASSLink.Plugins

Responsabilidad:

- Sistema de extensiones.
- Contratos para plugins.
- Gestión del ecosistema de plugins.

Dependencias:

- WASSLink.Abstractions
- WASSLink.Shared

---

# Sistema de Plugins

Los plugins implementan contratos definidos por:

