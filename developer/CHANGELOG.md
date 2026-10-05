# Changelog

Todos los cambios relevantes de GowLink Studio se documentan aquí.

## Beta 1.3 — Estabilización y transición de marca (en curso)

Fecha de registro: 2026-10-05.

- Actualización de la marca visible en Desktop a GowLink Studio / GowLink Downloader y del mensaje de inicio de GowLink CLI.
- Actualización del nombre de identidad del manifiesto de Desktop. Los nombres técnicos de solución, proyectos y namespaces `WASSLink.*` permanecen sin renombrar para evitar una migración parcial.
- Se incorporó `GowLinkLogger`; `WassLinkLogger` permanece como alias de compatibilidad.
- Los loggers y los informes de diagnóstico escriben en `%LOCALAPPDATA%\GowLink\Logs`. Los archivos históricos en `%LOCALAPPDATA%\WASSLink\Logs` se conservan, pero no se migran ni se usan como destino alternativo de escritura.
- La ruta predeterminada de descargas ahora es `Downloads\GowLink`, con fallback al directorio `Downloads\WASSLink` si existe y el nuevo todavía no.
- En la primera revisión se detectó que `LoggerService` aún escribía en `%APPDATA%\GowLink\logs`; esa discrepancia se corrigió y validó durante esta sesión.
- Validación inicial: `dotnet test --nologo --verbosity minimal` — 19/19 pruebas aprobadas en esa revisión. La suite no representaba una prueba real de descarga extremo a extremo ni prueba específica de migración de rutas.
- En ese punto seguía pendiente validar el flujo completo con contenido autorizado; la validación runtime posterior queda registrada al final de esta sección.
- Unificación de logs: `LoggerService`, `GowLinkLogger` y los informes de diagnóstico escriben ahora en `%LOCALAPPDATA%\GowLink\Logs`. El directorio se crea al inicializar los loggers o al escribir un log; los archivos históricos en `%LOCALAPPDATA%\WASSLink\Logs` no se mueven ni se eliminan.
- Validación tras el cambio de rutas en esa revisión: `dotnet test --nologo --verbosity minimal` — 19/19 pruebas aprobadas.
- Ejecución runtime: CLI mostró “GowLink CLI iniciado.”; Desktop abrió con título y marca GowLink, la UI reportó correctamente URL inválida y fallo de inspección contra localhost sin servicio, y el botón Cancelar produjo “Descarga cancelada por el usuario.” No se completó una descarga real ni se midió progreso de transferencia.
- Tras cerrar Desktop se comprobaron `app.log` y `gowlink-2026-10-05.log` bajo `%LOCALAPPDATA%\GowLink\Logs`; `app.log` registró la URL localhost y el comando de yt-dlp usado en la prueba cancelada.
- Validación runtime posterior con `https://www.youtube.com/watch?v=7Ne9cbREOnk`: yt-dlp detectó el título `Si te tengo a ti, lo tengo todo - Marcos Brunet ｜ Adoración`, duración 5:11 y 7 formatos (3 de audio y 4 de vídeo). La Quality Grid pobló los formatos; aún no representa título ni duración.
- Se completaron el audio recomendado M4A (5.034.356 bytes) y el vídeo recomendado MP4 1080p (69.549.423 bytes), guardados en `Downloads\WASSLink` por fallback al directorio heredado existente. La UI reportó progreso/velocidad, procesamiento FFmpeg y `Completado`; ambos medios se decodificaron correctamente con FFmpeg.
- Una ejecución de vídeo encontró un HTTP 403 transitorio; tras inspeccionar de nuevo y reintentar, la descarga recomendada terminó correctamente. Para conservar nombres Unicode se añadió `--encoding UTF-8` a yt-dlp; la ejecución posterior registró la ruta MP4 exacta en `%LOCALAPPDATA%\GowLink\Logs\app.log`.
- Validación actualizada: `dotnet test --nologo --verbosity minimal` — 20/20 pruebas aprobadas.

## Beta 1 — GowLink Downloader (en estabilización)

### B1.1 — Base e interfaz (completada)

#### Arquitectura

- Definición de contratos en WASSLink.Abstractions: IDownloadProvider, DownloadRequest, DownloadProgress y DownloadResult.
- Implementación de IDownloadProvider en YtDlpDownloadService con manejo asíncrono y resolución dinámica de herramientas.
- Registro en Dependency Injection mediante AddDownloadServices() y consumo vía constructor en MainViewModel (sin new()).
- UI de GowLink Desktop (`WASSLink.Desktop`) con barra lateral funcional, secciones "Próximamente" para futuras betas (Búsqueda, Reproductor, Biblioteca, Archivos) e indicador de estado del motor yt-dlp/FFmpeg.
- Resolución de advertencias de compilación (CA2024 y CS0105).
- Suite de pruebas inicial de 11 casos superados; la suite actual alcanza 20/20 (validada en Beta 1.3).

### B1.2 — Quality Grid (implementado; validado en runtime)

- Inspección asíncrona de formatos con yt-dlp, presentación de opciones de audio/vídeo y selección de formato.
- El usuario puede solicitar una descarga con el formato elegido.
- La inspección runtime devolvió 7 formatos y permitió descargar audio M4A y vídeo MP4 1080p. El título y la duración se obtuvieron de los metadatos, aunque todavía no se muestran en la Quality Grid.

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
