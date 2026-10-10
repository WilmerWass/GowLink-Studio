# GowLink Studio — Roadmap oficial

## 1. Alcance y criterio de la documentación

Este documento es la fuente oficial de planificación del proyecto.

Su objetivo es distinguir claramente entre:

- funcionalidad ya verificada
- funcionalidad implementada con alcance parcial o pendiente de confirmación
- funcionalidad futura o experimental
- mejoras de estabilidad y consolidación
- criterios de aceptación por hito

No se tratarán como equivalentes una mejora real de producto, una etapa de consolidación y un módulo nuevo.

## 2. Estado actual y clasificación

### Estado actual verificado del proyecto

El proyecto está en la etapa denominada:

**GowLink Studio — Beta 1.4: Universal Download Engine**

Se consideran confirmadas las siguientes capacidades, como mínimo en el estado reportado por la sesión de trabajo reciente:

- branding actualizado a GowLink Studio
- ruta predeterminada de descargas en `Downloads\GowLink`
- navegación entre Downloader, Biblioteca, Buscador, Lista de Descargas y Configuración
- selección dinámica del motor activo para HTTP directo o yt-dlp
- manejo de pausa HTTP sin bloquear la interfaz
- compilación correcta con `dotnet build -c Debug`
- pruebas unitarias correctas con `dotnet test tests\WASSLink.Tests\WASSLink.Tests.csproj -c Debug --no-restore`

Estas evidencias deben entenderse como estado verificado por la sesión actual, no como promesa de compatibilidad universal ni como cierre completo de todas las capacidades futuras.

### Clasificación del trabajo

- Implementación real: nuevas capacidades funcionales con integración y validación
- Mejora y consolidación: estabilidad, UX, configuración, diagnósticos, cola, manejo de errores
- Visión futura: capacidades aún no implementadas o sin validación real

## 3. Convención de versiones

Se recomienda mantener esta convención:

- `1.4.1` para una release de consolidación o corrección dentro de la serie 1.4
- `1.5.0-beta.1`, `1.5.0-beta.2` para versiones preliminares
- `1.5.0` para la release final de una etapa
- `1.5.1`, `1.5.2` para correcciones posteriores

La etiqueta humana “Beta 1.5” debe distinguirse del identificador formal del paquete o de la release. No se asume que un número mayor automáticamente significa una versión pública ya publicada.

## 4. Roadmap oficial

### 1.4.1 — Consolidation Release

Tipo: mejora y consolidación.

Objetivo:

Consolidar la arquitectura y la experiencia actuales antes de cerrar la siguiente etapa de crecimiento funcional.

Prioridades:

- estabilidad del motor HTTP directo y del motor yt-dlp
- navegación coherente entre módulos
- multidescargas y cola fiable
- configuración persistente
- manejo de errores, pausa y cancelación
- validación del paquete portable
- pruebas de regresión y documentación de limitaciones

Dependencias:

- motor dual validado
- diseño de navegación establecido
- flujo de descargas centralizado
- default path y configuración visible

Riesgos:

- doble modelo de flujo (HTTP + yt-dlp) sin una cola común bien definida
- comportamiento inconsistente entre módulos
- falencias de UX ocultas tras una pantalla funcional
- estado incompleto de configuración persistente

Exclusiones:

- no se declaran nuevas plataformas ni proveedores externos no probados
- no se introducen módulos de reproducción, biblioteca profunda o torrents como parte de esta release

Criterios de aceptación:

- la UI principal funciona con navegación coherente y sin contradicciones funcionales
- los motores seleccionados se reflejan dinámicamente según la URL
- el default path de descarga es funcional y consistente
- la cola y la gestión de errores son estables bajo uso normal
- la compilación y las pruebas principales se mantienen verdes

### 1.5 — Search & Providers

Tipo: implementación nueva.

Objetivo:

Formalizar la detección de proveedores y la resolución del contenido.

Alcance previsto:

- URL Analyzer
- identificación de proveedor y recurso
- metadatos y capacidades del contenido
- integración del primer proveedor prioritario
- videos individuales y playlists cuando sean compatibles
- selección de elementos y formatos
- preparación para ampliar proveedores

Dependencias:

- 1.4.1 consolidado
- un modelo de entrada URL claro
- un contrato común de contenido y capacidades

Riesgos:

- compatibilidad superficial con plataformas externas
- diferencia entre “es detectable” y “es compatible de forma fiable”
- sobreprometer la soportabilidad de providers sin pruebas documentadas

Exclusiones:

- no se promete compatibilidad universal con plataformas externas
- la reproducción en línea o la visualización avanzada no forman parte de este hitos

Criterios de aceptación:

- la URL puede resolverse sin ambigüedad en al menos un conjunto probado
- la detección de contenido se presenta con metadatos útiles
- las decisiones de proveedor se documentan y validan
- la selección de formato se integra con la cola de descargas

### 1.6 — Media Studio

Tipo: implementación nueva.

Objetivo:

Desarrollar la base funcional del entorno multimedia de GowLink.

Alcance previsto:

- biblioteca local de medios
- reproducción de audio y vídeo
- controles básicos de reproducción
- gestión de recursos y metadatos
- integración con el flujo de descargas

Dependencias:

- 1.5 resuelto con proveedores y metadatos
- librería y organización de archivos definida

Riesgos:

- compatibilidad de formatos no validada
- reproducción con dependencias externas no testadas
- mezcla de contenido descargado con contenido remoto o fraccionado

Criterios de aceptación:

- la reproducción básica funciona con formatos admitidos
- la UI de reproducción presenta controles reales y estables
- los archivos descargados se integran con una biblioteca funcional

### 1.7 — File Studio

Tipo: implementación nueva.

Objetivo:

Ampliar las capacidades del motor HTTP y del manejo de archivos.

Alcance previsto:

- reanudación de descargas
- Range Requests
- descarga segmentada compatible con servidor
- integridad de archivo
- gestión de archivos grandes
- organización y verificación del almacenamiento

Dependencias:

- motor HTTP directo ya consolidado
- cola de descargas con control de estado

Riesgos:

- servidores incompatibles con rangos parciales
- corrupción o archivos incompletos si la reanudación no está bien validada
- complejidad añadida en la cola

Criterios de aceptación:

- reanudación funcional bajo pruebas controladas
- manejo correcto de errores por red y de integridad
- validación de archivos grandes con comprobación de tamaño y finalización

### 1.8 — Torrent Studio

Tipo: implementación nueva.

Objetivo:

Introducir un motor de torrents independiente del motor HTTP.

Alcance previsto:

- metadata de torrent
- magnet links y enlaces de torrent
- selección de archivos del torrent
- progreso y paralelismo
- integración con la cola general

Dependencias:

- módulo de descarga general bien definido
- gestión de destinos y trabajo en cola

Riesgos:

- complejidad operativa superior al HTTP directo
- consumo de ancho de banda y tiempo de sincronización
- dependencia de librerías externas no verificadas

Criterios de aceptación:

- un torrent concreto puede añadirse y gestionarse sin romper la cola principal
- el progreso y la selección del contenido son visibles y consistentes
- fallos específicos del torrent no bloquean el resto del sistema

### 1.9 — Integration Studio

Tipo: integración y consolidación.

Objetivo:

Consolidar los componentes de la plataforma y su experiencia integrada.

Alcance previsto:

- interoperabilidad de motores
- pruebas de regresión
- rendimiento y estabilidad
- configuración central
- biblioteca, reproductor y cola integrados
- interoperabilidad con plugins y proveedores

Dependencias:

- 1.5–1.8 con hitos funcionales mínimos
- controles de calidad y documentación de limitaciones

Riesgos:

- integración prematura de componentes aún no estabilizados
- experiencia de usuario inconsistente entre motores
- regresiones que no queden cubiertas por pruebas

Criterios de aceptación:

- la plataforma funciona como un conjunto integrado
- los motores especializados cooperan con la cola general
- la regresión crítica queda cubierta por pruebas
- la documentación y la experiencia de uso están alineadas con lo verificado

## 5. Hitos históricos relevantes

### Fase 0 — Preparación

Estado: documentado como base del proyecto.

### Fase 1 — Arquitectura y estructura

Estado: completada como base del repositorio y de la solución.

### 1.4.1 — Consolidation Release

Estado: etapa actual de consolidación y mejora.

### 1.5 onwards

Estado: planificado como implementación de nuevas capacidades, no como una simple continuación visual de 1.4.1.

## 6. Política de planificación

- no se promociona una versión como “lista” sin criterios de aceptación definidos
- no se convierten mejoras puntuales en “nueva beta” sin justificación funcional
- no se declaran plataformas compatibles sin pruebas reales
- no se mezclan requisitos de producto con capacidades experimentales

## 7. Resumen ejecutivo

La estrategia correcta para GowLink Studio es separar la consolidación actual de la expansión funcional.

- 1.4.1 es una fase de estabilización y maduración
- 1.5 a 1.9 representan etapas funcionales concretas y distintas
- la documentación interna debe mantener esa distinción para evitar promesas innecesarias o confusión entre ingeniería y estrategia

Estado: planificada.

Implementar:

- Sistema modular de proveedores/plugins.
- Descubrimiento de plugins.
- Registro.
- Ciclo de vida.
- Versionado.
- Proveedores de búsqueda.
- Proveedores de descarga.
- Adaptadores de plataforma.

Los plugins deben permitir adaptar capacidades a diferentes plataformas:

Core
↓
Plugin System
├── Windows
├── Linux
└── Android

---

# Beta 7 — File Studio

Estado: planificada.

Objetivo:
Descargador de archivos generales (no solo multimedia).

Tipos contemplados:

- ZIP, RAR, 7Z
- PDF, documentos
- Imágenes
- Instaladores, EXE, ISO
- Otros archivos descargables

Separar conceptualmente:

Media Download ≠ General File Download.

Incluye:
- Descarga segmentada (si el servidor lo permite).
- Detección de archivos multipartes.
- Reanudación de descargas interrumpidas.

---

# Beta 8 — Torrent Studio

Estado: planificada.

Implementar:

- Magnet links.
- Archivos .torrent.
- Progreso.
- Pausa y reanudación.
- Velocidad.
- Destino personalizable.
- Integración con biblioteca.

Debe ser modular y respetar las capacidades de cada plataforma.

La implementación torrent debe ser desacoplada del Core.

---

# Beta 9 — Linux Edition

Estado: planificada.

Objetivo:
Adaptar el producto para Linux.

La arquitectura Core debe minimizar dependencias específicas de Windows.

---

# Beta 10 — Mobile / Android

Estado: futura.

Objetivo:
Adaptar el producto para Android.

No asumir que Android tendrá exactamente las mismas capacidades que Windows/Linux.

Usar plugins/adaptadores cuando las restricciones de plataforma lo requieran.

---

# V2 — GowLink Media Studio

Estado: futura.

Producto completo integrando:

- Downloader
- Search
- Library
- Player
- Plugins
- File Downloader
- Torrent
- Multi-platform

---

# Principios del roadmap

No se implementará una capacidad únicamente porque aparezca en el roadmap.

Cada capacidad debe pasar por:

Diseño → Implementación → Integración → Pruebas → Documentación

Una sub-beta solamente se crea cuando una funcionalidad concreta está terminada y probada.

NO implementar todas las funcionalidades simultáneamente.

Prioridad actual: terminar y validar la estabilización de Beta 1.3. No comenzar Beta 2 antes de cumplir B1.1–B1.5 y B1 FINAL.
