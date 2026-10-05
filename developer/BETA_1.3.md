# GowLink Studio — Beta 1.3

**Estado:** En curso — estabilización de Beta 1  
**Fecha de actualización:** 2026-10-05

Beta 1.3 consolida el descargador existente y la transición visible a GowLink. No inicia Beta 2 ni declara terminadas capacidades que no hayan sido verificadas.

## Estado comprobado

- La UI Desktop muestra la marca GowLink Studio / GowLink Downloader; la aplicación CLI usa el mensaje de inicio GowLink.
- Quality Grid inspecciona metadatos y formatos con yt-dlp y permite elegir una opción antes de descargar.
- `YtDlpDownloadService` informa progreso, recibe cancelación y trata de detener el proceso externo.
- `GowLinkLogger` y los informes de diagnóstico usan `%LOCALAPPDATA%\GowLink\Logs`, con fallback a `%LOCALAPPDATA%\WASSLink\Logs` cuando la ruta anterior existe y la nueva no.
- La carpeta de salida predeterminada es `Downloads\GowLink`, con fallback a `Downloads\WASSLink` cuando solo existe la carpeta heredada.
- La última ejecución registrada de `dotnet test --nologo --verbosity minimal` aprobó 19/19 pruebas.

Los tests cubren validación de URL, DI, parsing de formatos, selectores de yt-dlp y estado del ViewModel, entre otros casos. No realizan una descarga real end-to-end ni verifican automáticamente el fallback/migración de rutas.

## Trabajo de estabilización pendiente

1. Validar manualmente inspección, selección, descarga, archivo de salida, progreso y cancelación usando contenido que el usuario esté autorizado a descargar.
2. Reforzar validación automatizada de comportamiento sin depender innecesariamente de servicios externos.
3. Revisar y validar mensajes ante herramienta ausente, URL no soportada, fallos de yt-dlp/FFmpeg y errores de escritura.
4. Unificar rutas de logs. El logger compartido usa `%LOCALAPPDATA%\GowLink\Logs`; el `LoggerService` del descargador todavía escribe en `%APPDATA%\GowLink\logs` y usa `%APPDATA%\WASSLink\logs` como fallback heredado.
5. Decidir y ejecutar el rebranding técnico de namespaces, proyectos y ensamblados en un cambio coordinado. Hoy siguen con nombres `WASSLink.*`; el manifiesto Desktop y nombres visibles ya reflejan GowLink. No renombrar componentes de forma aislada.
6. Actualizar documentación y validar publicación/paquete una vez que el comportamiento se haya comprobado.

## Fuera de alcance

- Beta 2 (búsqueda integrada).
- Funciones de reproductor, biblioteca, plugins, descargas generales, torrents o Android.
- Declarar compatibilidad con proveedores no probados.

## Criterios de cierre

- La suite completa pasa con `dotnet test WASSLink-Studio.slnx`.
- El flujo funcional está probado extremo a extremo y los errores comunes tienen resultado/mensaje claro.
- Las rutas de almacenamiento y logs están coherentes, y el fallback heredado ha sido verificado.
- La estrategia para los identificadores técnicos `WASSLink.*` está completada, o el alcance acordado los mantiene sin cambios y los identifica claramente como legado.
- Changelog, manifest, roadmap y material de publicación reflejan el estado verificado; no se anuncia Beta 1 final antes de completar B1.1–B1.5.
