# GowLink Studio — Arquitectura

## 1. Propósito del documento

Este documento distingue entre:

- arquitectura actual verificada
- arquitectura objetivo
- capacidades ya implementadas
- capacidades futuras o experimentales

Esto es necesario porque el proyecto ya ha evolucionado más allá de la etapa de “descargador básico” y necesita separar claramente la base funcional de la visión de producto.

## 2. Arquitectura actual verificada

La arquitectura actual del proyecto se puede resumir así:

- Apps: entrada del usuario y composición
- Core: contratos y lógica reutilizable
- Tests: validación funcional

### Capa Apps

Responsables de: UI, entrada de usuario, composición de servicios, presentación.

Proyectos actuales:

- `src/Apps/WASSLink.Desktop`
- `src/Apps/WASSLink.CLI`

Reglas:

- no deben contener lógica de negocio central
- deben consumir servicios del Core mediante inyección de dependencias
- no deben crear servicios críticos con `new()` cuando exista una abstracción

### Capa Core

Responsables de: modelos compartidos, contratos, servicios reutilizables, motores, diagnóstico y lógica ejecutiva.

Proyectos principales:

- `WASSLink.Abstractions`
- `WASSLink.Shared`
- `WASSLink.Download`
- `WASSLink.Search`
- `WASSLink.Library`
- `WASSLink.Player`
- `WASSLink.Plugins`
- `WASSLink.Configuration`

### Capa Tests

- validan la lógica del Core y la integración con los motores
- no deben depender innecesariamente de la capa de UI
- se usan como mecanismo de regresión para cambios funcionales

## 3. Estado real de los módulos principales

### WASSLink.Abstractions

Responsabilidad:

- contratos de descarga
- modelos compartidos
- definición del flujo de trabajo básico

Estado:

- implementado y usado por la arquitectura actual

### WASSLink.Download

Responsabilidad:

- resolución de proveedores
- ejecución del motor de descarga
- progresos y cancelación
- enrutamiento entre motores

Estado actual:

- incluye el diseño dual basado en HTTP directo y yt-dlp
- la lógica de proveedor y enrutamiento es parte del estado actual objetivo de la beta 1.4

### WASSLink.Search

Responsabilidad:

- búsqueda de contenidos, resultados y consulta por URL

Estado:

- parte del desarrollo futuro y no debe tratarse como completamente integrada si no ha sido verificada como tal

### WASSLink.Library

Responsabilidad:

- organización, listado y consulta del contenido descargado

Estado:

- presente como estructura y flujo de UI, pero no como módulo completamente validado en profundidad

### WASSLink.Player

Responsabilidad:

- reproducción y control multimedia

Estado:

- futuro funcional, no cierre de implementación actual

### WASSLink.Plugins

Responsabilidad:

- extensibilidad y Providers/engines externos

Estado:

- futuro arquitectónico, no equivalente a integración comprobada

### WASSLink.Configuration

Responsabilidad:

- preferencias, rutas y configuración de la aplicación

Estado:

- servicio JSON local implementado para preferencias e historial terminal; la publicación portable y el manejo de fallos siguen sujetos a validación

## 4. Arquitectura objetivo prevista

La arquitectura objetivo del producto debería organizarse alrededor de un flujo común:

1. Entrada URL universal
2. URL Analyzer
3. Provider Resolver
4. Content Resolver
5. Definición de capacidades del contenido
6. DownloadJob
7. Cola común de descargas
8. Motores especializados y desacoplados
9. Biblioteca y reproductor integrados

Este flujo es una visión de arquitectura, no una afirmación de que todas esas capas ya existan completamente.

## 5. Motor dual actual

El proyecto actual ya presenta una separación funcional clara entre dos modos de descarga:

- HTTP Directo (Streaming)
  - para archivos directos y enlaces de descarga
- yt-dlp & FFmpeg
  - para multimedia y contenido externalizado

Esta separación es parte del estado actual del proyecto, pero todavía requiere consolidación en cola, configuración y manejo de fallos para alcanzar estabilidad de producto.

## 6. Dependencias y reglas de diseño

Reglas que deben mantenerse:

- Apps → Core
- Core → Core inferior
- Tests → módulo que valida
- la capa de UI no debe absorber lógica de negocio central
- los motores especializados deben ser reemplazables y descubiertos sin acoplar el resto del sistema a implementaciones concretas

## 7. Deuda técnica y riesgos

La arquitectura actual tiene puntos que requieren atención:

- unificar la cola de trabajo para todos los motores
- consolidar la configuración persistente
- hacer explícitas las limitaciones por proveedor
- definir con claridad la diferencia entre “detectable” y “compatible”
- documentar qué partes están ya validadas y cuáles solo están planificadas

## 8. Resumen arquitectónico

La arquitectura correcta para GowLink Studio es modular, dual-motor y orientada al contenido, pero no debe confundirse con una implementación completa ya cerrada.

La arquitectura actual tiene una base sólida para:

- descargar contenido directo
- resolver contenido multimedia
- gestionar módulos de UI y navegación
- preparar la expansión a búsqueda, biblioteca, reproductor y motores especializados

La evolución futura debe ocurrir por etapas verificadas, no por promesas amplias de compatibilidad.

No permitido:

Core → Apps

Core → UI

Core → implementaciones externas concretas cuando exista una abstracción adecuada

---

# Flujo principal (Beta 1)

Usuario
↓
GowLink Desktop (`WASSLink.Desktop`, App)
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

Para archivos grandes, GowLink debe detectar las capacidades del servidor:

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

El cambio de marca en la UI y ciertos datos de ejecución ya está iniciado. La migración de namespaces y nombres de proyecto todavía no se realizó y deberá abordarse en un cambio separado, coordinando referencias, XAML, ensamblados, solución, pruebas y distribución.

---

# Estado

Estado documental: Beta 1.4.1 — consolidación en curso; no publicada.

La cola común, la persistencia local y las protecciones de temporales HTTP están implementadas. La validación automatizada y un build Release se ejecutaron durante esta consolidación; el paquete portable limpio y los proveedores externos todavía requieren validación y no se consideran confirmados por la arquitectura.

El logger compartido, el logger interno de descargas y los informes de diagnóstico usan `%LOCALAPPDATA%\GowLink\Logs`. Los loggers crean el directorio si no existe. Los archivos históricos que permanezcan bajo `%LOCALAPPDATA%\WASSLink\Logs` se conservan; no se migran ni se eliminan automáticamente.

Fase 1: completada. Beta 1.4.1: en consolidación.
