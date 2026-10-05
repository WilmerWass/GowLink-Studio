# Changelog

Todos los cambios relevantes de GowLink Studio se documentan aquí.

## Beta 1.3 — Estabilización y transición de marca (en curso)

Fecha de registro: 2026-10-05.

- Actualización de la marca visible en Desktop a GowLink Studio / GowLink Downloader y del mensaje de inicio de GowLink CLI.
- Actualización del nombre de identidad del manifiesto de Desktop. Los nombres técnicos de solución, proyectos y namespaces `WASSLink.*` permanecen sin renombrar para evitar una migración parcial.
- Se incorporó `GowLinkLogger`; `WassLinkLogger` permanece como alias de compatibilidad.
- El logger compartido y los informes de diagnóstico prefieren `%LOCALAPPDATA%\GowLink\Logs` y usan `%LOCALAPPDATA%\WASSLink\Logs` como fallback si la ruta antigua existe y la nueva todavía no.
- La ruta predeterminada de descargas ahora es `Downloads\GowLink`, con fallback al directorio `Downloads\WASSLink` si existe y el nuevo todavía no.
- Pendiente de estabilización: `LoggerService`, usado por las descargas, aún guarda en `%APPDATA%\GowLink\logs` y conserva fallback en `%APPDATA%\WASSLink\logs`; sus rutas deben unificarse y validarse antes de declarar completa la transición de logging.
- Validación ejecutada: `dotnet test --nologo --verbosity minimal` — 19/19 pruebas aprobadas. La suite no representa una prueba real de descarga extremo a extremo ni prueba específica de migración de rutas.
- Pendiente: validar manualmente el flujo completo con una URL y contenido que el usuario tenga derecho a descargar; completar estabilización y decidir/migrar namespaces y nombres técnicos en un cambio coordinado.

## Beta 1 — GowLink Downloader (en estabilización)

### B1.1 — Base e interfaz (completada)

#### Arquitectura

- Definición de contratos en WASSLink.Abstractions: IDownloadProvider, DownloadRequest, DownloadProgress y DownloadResult.
- Implementación de IDownloadProvider en YtDlpDownloadService con manejo asíncrono y resolución dinámica de herramientas.
- Registro en Dependency Injection mediante AddDownloadServices() y consumo vía constructor en MainViewModel (sin new()).
- UI de GowLink Desktop (`WASSLink.Desktop`) con barra lateral funcional, secciones "Próximamente" para futuras betas (Búsqueda, Reproductor, Biblioteca, Archivos) e indicador de estado del motor yt-dlp/FFmpeg.
- Resolución de advertencias de compilación (CA2024 y CS0105).
- Suite de pruebas inicial de 11 casos superados; la suite actual alcanza 19/19 (validada en Beta 1.3).

### B1.2 — Quality Grid (implementado; validación extremo a extremo pendiente)

- Inspección asíncrona de formatos con yt-dlp, presentación de opciones de audio/vídeo y selección de formato.
- El usuario puede solicitar una descarga con el formato elegido.
- Los tests cubren parsing de formatos y composición de selectores; no se considera una prueba en vivo de descarga.

### Funcionalidades implementadas (acumuladas desde Fase 1)

#### Arquitectura

- Consolidación de la arquitectura modular.
- Organización Apps / Core / Tests.
- Infraestructura inicial de Dependency Injection.
- Integración inicial de GowLink Desktop (`WASSLink.Desktop`).
- Integración inicial de GowLink CLI (`WASSLink.CLI`).
- Validación de compilación completa.

#### Logging y diagnóstico

- `GowLinkLogger` (con alias `WassLinkLogger`): escritura sincronizada de logs diarios.
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
