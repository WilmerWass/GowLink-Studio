# Changelog

Todos los cambios relevantes de WASSLink Studio se documentan aquí.

## Beta 1 — Download Studio (en curso)

### B1.1 — Base e interfaz (en curso)

#### Arquitectura

- Definición de IDownloadProvider en WASSLink.Abstractions.
- DownloadRequest y DownloadProgress como contratos de datos.
- YtDlpDownloadService implementa IDownloadProvider.
- MainViewModel recibe IDownloadProvider mediante DI.
- UI renovada con barra lateral y secciones "Próximamente".

#### Pendiente para B1.1

- Compilación verificada con cambios de DI.
- Tests básicos de compilación.

### Funcionalidades implementadas (acumuladas desde Fase 1)

#### Arquitectura

- Consolidación de la arquitectura modular.
- Organización Apps / Core / Tests.
- Infraestructura inicial de Dependency Injection.
- Integración inicial de WASSLink.Desktop.
- Integración inicial de WASSLink.CLI.
- Validación de compilación completa.

#### Logging y diagnóstico

- WassLinkLogger: escritura de logs a archivo con rotación diaria y sincronización.
- DiagnosticReportGenerator: reportes de diagnóstico (OS, .NET, logs recientes).

#### Descarga

- YtDlpDownloadService: ejecución de yt-dlp.exe como proceso externo.
- Resolución de ruta portable (busca herramientas relativas a la carpeta de instalación).
- Parsing de progreso mediante regex.
- Soporte de CancellationToken para cancelación de descarga.
- UI básica: campo URL, selección de tipo (Audio/Vídeo) y formato (MP3/MP4), botón descargar, barra de progreso, estado.

---

## Fase 1 — Arquitectura

### Completada

La Fase 1 estableció:

- Solución .NET 10.
- Estructura modular.
- Proyectos Core.
- Aplicaciones Desktop y CLI.
- Proyecto de Tests.
- Referencias entre proyectos.
- Dependency Injection inicial.
- Integración Avalonia.
- Compilación completa.

Validación:

    dotnet build WASSLink-Studio.slnx
    // Compilación realizada correctamente.

---

## Convenciones

Los cambios deben agruparse por funcionalidad y mantenerse coherentes con la arquitectura documentada.

Diferenciar siempre:

IMPLEMENTADO / EN CURSO / PLANIFICADO / EXPERIMENTAL / FUTURO
