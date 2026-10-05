# GowLink Studio — Beta 1.3

**Estado:** En curso — estabilización de Beta 1  
**Fecha de actualización:** 2026-10-05

Beta 1.3 consolida el descargador existente y la transición visible a GowLink. No inicia Beta 2 ni declara terminadas capacidades que no hayan sido verificadas.

## Estado comprobado

- La UI Desktop muestra la marca GowLink Studio / GowLink Downloader; la aplicación CLI usa el mensaje de inicio GowLink.
- Quality Grid inspecciona metadatos y formatos con yt-dlp y permite elegir una opción antes de descargar.
- `YtDlpDownloadService` informa progreso, recibe cancelación y trata de detener el proceso externo.
- `GowLinkLogger`, `LoggerService` y los informes de diagnóstico usan `%LOCALAPPDATA%\GowLink\Logs`; los loggers crean el directorio si no existe.
- Los archivos históricos que permanezcan en `%LOCALAPPDATA%\WASSLink\Logs` no se migran ni se eliminan.
- La carpeta de salida predeterminada es `Downloads\GowLink`, con fallback a `Downloads\WASSLink` cuando solo existe la carpeta heredada.
- CLI inició y mostró el mensaje GowLink. Desktop abrió con título de ventana GowLink; la automatización accesible confirmó sidebar, Quality Grid, el badge Beta 1.3, URL inválida, error de inspección localhost y acción de cancelar.
- Tras cerrar Desktop se verificó que `app.log` y `gowlink-2026-10-05.log` estaban en `%LOCALAPPDATA%\GowLink\Logs`.
- La inspección de la URL autorizada devolvió 7 formatos (3 de audio y 4 de vídeo), título `Si te tengo a ti, lo tengo todo - Marcos Brunet ｜ Adoración` y duración 5:11. La Quality Grid muestra los formatos; todavía no muestra título ni duración.
- Se completaron desde Desktop el audio M4A recomendado y el vídeo MP4 1080p recomendado. La UI mostró progreso/velocidad y el estado `Completado`; FFmpeg pudo decodificar ambos archivos.
- Los archivos M4A (5.034.356 bytes) y MP4 (69.549.423 bytes) quedaron en `Downloads\WASSLink` por fallback al directorio heredado existente.
- Tras fijar `--encoding UTF-8` en los argumentos de yt-dlp, `app.log` registró la ruta final Unicode exacta del MP4 en `%LOCALAPPDATA%\GowLink\Logs\app.log`.
- `dotnet test --nologo --verbosity minimal` aprobó 20/20 pruebas después de los cambios.

Los tests cubren validación de URL, DI, parsing de formatos, selectores de yt-dlp y estado del ViewModel, entre otros casos. El runtime valida los formatos indicados, no todos los escenarios de error o combinaciones; los tests tampoco verifican automáticamente la creación/fallback de rutas.

## Trabajo de estabilización pendiente

1. Ampliar las pruebas runtime con cancelación contra el proceso externo, fallos de yt-dlp/FFmpeg, errores de escritura y otros formatos con contenido autorizado.
2. Reforzar validación automatizada de comportamiento sin depender innecesariamente de servicios externos.
3. Revisar y validar mensajes ante herramienta ausente, URL no soportada, fallos de yt-dlp/FFmpeg y errores de escritura.
4. Decidir si la Quality Grid debe presentar título y duración; yt-dlp los extrae, pero la UI actual solo muestra los formatos.
5. Decidir y ejecutar el rebranding técnico de namespaces, proyectos y ensamblados en un cambio coordinado. Hoy siguen con nombres `WASSLink.*`; el manifiesto Desktop y nombres visibles ya reflejan GowLink. No renombrar componentes de forma aislada.
6. Actualizar documentación y validar publicación/paquete una vez que el comportamiento se haya comprobado.

## Fuera de alcance

- Beta 2 (búsqueda integrada).
- Funciones de reproductor, biblioteca, plugins, descargas generales, torrents o Android.
- Declarar compatibilidad con proveedores no probados.

## Criterios de cierre

- La suite completa pasa con `dotnet test WASSLink-Studio.slnx`.
- Audio M4A y vídeo MP4 1080p se validaron extremo a extremo, incluida la telemetría de progreso, FFmpeg y la ruta final. La cobertura automatizada de errores y otros escenarios runtime sigue pendiente.
- Las rutas de logs están unificadas en `%LOCALAPPDATA%\GowLink\Logs`; conservar los archivos heredados sin borrarlos.
- La estrategia para los identificadores técnicos `WASSLink.*` está completada, o el alcance acordado los mantiene sin cambios y los identifica claramente como legado.
- Changelog, manifest, roadmap y material de publicación reflejan el estado verificado; no se anuncia Beta 1 final antes de completar B1.1–B1.5.
