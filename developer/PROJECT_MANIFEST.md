# GowLink Studio — Manifiesto del proyecto

## 1. Identidad oficial

**Nombre oficial del producto:** GowLink Studio

**Nombre histórico del repositorio:** WASSLink-Studio

**Declaración de marca:**

La aplicación y la documentación deben referirse formalmente a GowLink Studio. Los nombres técnicos heredados `WASSLink.*` permanecen como identificadores de infraestructura o compatibilidad hasta una migración coordinada, no como nombre visible del producto.

## 2. Propósito y usuarios objetivo

GowLink Studio es un gestor de descargas universal orientado a:

- descargas de archivos directos por HTTP/HTTPS
- descargas multimedia mediante motores especializados
- organización y consulta de archivos descargados
- gestión centralizada de cola, progreso y configuración

El producto se orienta a usuarios que necesitan descargar contenido de forma flexible, con un flujo visible y con la posibilidad de separar motores por tipo de contenido.

## 3. Filosofía Portable First

La filosofía del proyecto es priorizar una experiencia portable, modular y adaptable, con menor dependencia de un único entorno de escritorio o una única plataforma de hosting.

Esto implica:

- mantener el núcleo y las abstracciones desacopladas de la interfaz
- favorecer la portabilidad del experimento funcional y del entorno de ejecución
- permitir que la aplicación evolucione sin depender de una sola integración visual o de una sola plataforma soporte

Portable First no equivale a declarar compatibilidad universal ni a asumir que una plataforma es compatible sin verificación real.

## 4. Estado actual verificado

El estado verificado del proyecto se describe mejor como:

**GowLink Studio — Beta 1.4.1: Universal Download Engine (consolidation in progress)**

Se consideran confirmadas las siguientes capacidades según el último estado de trabajo reportado:

- branding actualizado a GowLink Studio
- ruta predeterminada en `Downloads\GowLink`
- navegación entre Descargar, Biblioteca, Buscador, Historial y Configuración
- selección dinámica del motor según la URL: HTTP directo o yt-dlp
- corrección de la pausa HTTP sin bloquear la interfaz
- compilación correcta con `dotnet build -c Debug`
- validación de pruebas con `dotnet test tests\WASSLink.Tests\WASSLink.Tests.csproj -c Debug --no-restore`

Estas capacidades deben reconocerse como verificado en esta fase actual, no como cierre total del producto ni como compatibilidad universal.

## 5. Stack tecnológico real

Los componentes actuales y verificados del proyecto son:

- .NET 10
- C#
- Avalonia UI
- Microsoft.Extensions.DependencyInjection
- yt-dlp
- FFmpeg
- motor HTTP directo basado en `HttpClient`
- xUnit para pruebas

Tecnologías futuras o experimentales se documentan como tales y no se presentan como implementadas.

## 6. Estructura y arquitectura del repositorio

El repositorio se organiza en tres grandes bloques:

- `src/Apps` — puntos de entrada de usuario
- `src/Core` — lógica reutilizable y contratos
- `tests` — validación funcional y de integración

Estructura real:

- `src/Apps/WASSLink.Desktop`
- `src/Apps/WASSLink.CLI`
- `src/Core/WASSLink.Abstractions`
- `src/Core/WASSLink.Download`
- `src/Core/WASSLink.Search`
- `src/Core/WASSLink.Library`
- `src/Core/WASSLink.Player`
- `src/Core/WASSLink.Plugins`
- `src/Core/WASSLink.Configuration`
- `src/Core/WASSLink.Shared`
- `tests/WASSLink.Tests`

## 7. Arquitectura general

La solución usa una arquitectura modular con separación entre:

- capa de presentación
- capa de contratos y servicios reutilizables
- capa de motores especializados
- capa de pruebas

La lógica central debe implementarse en Core y consumirse desde Apps mediante inyección de dependencias.

## 8. Estado de la versión actual

La versión activa a documentar para este punto es:

**GowLink Studio — Beta 1.4.1**

La siguiente etapa oficial de consolidación es:

**1.4.1 — Consolidation Release (not yet published)**

Esto marca la diferencia entre:

- la fase de producto ya desarrollada y verificada en ciertos aspectos
- la fase de estabilización y mejora del producto que ya existe; no se declara lista hasta superar sus criterios de aceptación
- la fase funcional futura (búsqueda, biblioteca, reproductor, torrents, integración)

## 9. Limitaciones y riesgos conocidos

- No se declara compatibilidad universal con proveedores externos sin verificación real.
- No se asume que una pantalla funcional equivalga a una capacidad completamente integrada.
- La arquitectura dual HTTP + yt-dlp requiere una cola y una capa de decisión central bien definidas.
- La configuración persistente, la cola y la gestión de errores siguen siendo áreas de riesgo si no se validan de forma continua.
- La biblioteca, el reproductor y los motores especializados siguen siendo objetivos funcionales, no estado actual ya cerrado.

## 10. Visión a largo plazo

GowLink Studio se concibe como una plataforma modular de gestión de descargas y medios, con un motor principal común y varios motores especializados:

- descargas HTTP directas
- multimedia y extracción con yt-dlp
- biblioteca de archivos y medios
- reproducción
- búsqueda integrada
- extensibilidad mediante plugins
- motores futuros adicionales, como torrents

La visión no reemplaza el estado real. La documentación debe mantener ambas capas separadas.
