# WASSLink Studio — Roadmap Oficial

## Historial

### Fase 0 — Preparación

Estado: completada.

Objetivos cumplidos:

- Definir identidad del proyecto.
- Establecer documentación inicial.
- Preparar repositorio.
- Definir principios de desarrollo.

---

### Fase 1 — Arquitectura y estructura

Estado: COMPLETADA.

Objetivos alcanzados:

- Crear solución .NET 10.
- Crear estructura Apps/Core/Tests.
- Crear proyectos Core.
- Crear WASSLink.Desktop.
- Crear WASSLink.CLI.
- Crear proyecto de Tests.
- Definir referencias entre proyectos.
- Validar mapa de dependencias.
- Crear infraestructura inicial de Dependency Injection.
- Integrar Avalonia.
- Validar compilación completa.

Resultado:

    dotnet build WASSLink-Studio.slnx
    // Compilación realizada correctamente.

---

## Roadmap actual

---

# Beta 1 — Download Studio

Estado: EN CURSO.

Objetivo:

Crear un descargador multimedia funcional.

Proveedor inicial: YouTube (yt-dlp).

Capacidades de Beta 1:

- Descarga de audio (MP3) y vídeo (MP4).
- Entrada por URL.
- Progreso de descarga en tiempo real.
- Cancelación de descarga.
- Manejo básico de errores.
- Organización básica de archivos descargados.
- Logs y diagnóstico.
- Interfaz moderna con barra lateral.

## Sub-betas de Beta 1

Las sub-betas se crean únicamente cuando una funcionalidad concreta está terminada y probada.

NO utilizar fechas artificiales.

### B1.1 — Base e interfaz

Estado: COMPLETADA.

Objetivo:
Infraestructura DI correcta + UI con barra lateral + contrato IDownloadProvider.

Criterios cumplidos:
- IDownloadProvider definido en WASSLink.Abstractions.
- YtDlpDownloadService implementa IDownloadProvider.
- MainViewModel recibe IDownloadProvider por DI (sin new()).
- UI con barra lateral funcional y secciones Próximamente.
- Compilación y pruebas unitarias correctas (0 errores, 0 advertencias, 11 pruebas superadas).

### B1.2 — Descarga real de YouTube

Objetivo:
Descarga de audio y vídeo desde YouTube.

Criterios:
- URL de YouTube → descarga → archivo en carpeta de salida.
- Sin errores inesperados en flujo normal.

### B1.3 — Progreso y cancelación robustos

Objetivo:
Progreso en tiempo real, cancelación limpia.

Criterios:
- Barra de progreso actualizada durante descarga.
- Cancelación detiene el proceso de yt-dlp correctamente.
- Estado visual claro (descargando / completado / cancelado / error).

### B1.4 — Organización

Objetivo:
Organización automática de archivos descargados.

Criterios:
- Carpeta de destino configurable.
- Subcarpetas por plataforma/creador cuando sea posible.

### B1.5 — Robustez y errores

Objetivo:
Manejo de casos de error comunes.

Criterios:
- URL inválida → mensaje claro.
- yt-dlp no encontrado → mensaje claro con instrucciones.
- Descarga fallida → mensaje de error legible.

### B1 FINAL — Primera beta pública

Objetivo:
Beta 1 completa, funcional y documentada.

Criterios:
- Todos los objetivos de B1.1 a B1.5 cumplidos.
- Changelog actualizado.
- Release en GitHub.
- Documentación actualizada.

---

# Beta 2 — Search Studio

Estado: planificada.

Implementar:

- Búsqueda integrada de contenido.
- Resultados de búsqueda.
- Selección de resultado para descarga.
- Conexión: búsqueda → descarga.

---

# Beta 3 — Media Player

Estado: planificada.

Implementar:

- Reproducción de audio.
- Reproducción de vídeo.
- Controles: play, pause, stop, seek.
- Volumen.
- Duración y posición.
- Siguiente / anterior.
- Integración con biblioteca.

El reproductor permanece como componente central de WASSLink Studio.

---

# Beta 4 — Library Studio

Estado: planificada.

Implementar:

- Biblioteca local de contenido descargado.
- Detección e importación.
- Organización.
- Metadatos.
- Búsqueda local.
- Filtros.
- Historial.
- Favoritos.

---

# Beta 5 — Multi-Platform Providers

Estado: planificada.

Objetivo:
Incorporar proveedores de descarga adicionales.

Proveedores experimentales candidatos:

- MediaFire
- MEGA
- Servidores HTTP/HTTPS directos
- Otros proveedores según compatibilidad real

IMPORTANTE:
No afirmar compatibilidad con ningún proveedor hasta probarlo.
La compatibilidad depende de las capacidades y restricciones de cada plataforma.

---

# Beta 6 — Plugin System

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

# V2 — WASSLink Media Studio

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

Prioridad actual: Beta 1 funcional y sólida antes de avanzar a Beta 2.
