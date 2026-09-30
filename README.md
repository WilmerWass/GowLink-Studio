# 🎵 WASSLink Studio — Beta 1.2 "Quality Grid"

> Aplicación de escritorio **portable para Windows x64** que permite gestionar y descargar audio y vídeo desde fuentes compatibles, con selección de calidad, procesamiento en segundo plano y una interfaz sencilla.

[![Release](https://img.shields.io/github/v/release/WilmerWass/WASSLink-Studio?include_prereleases&label=release)](https://github.com/WilmerWass/WASSLink-Studio/releases/tag/Beta_1.2)
[![License](https://img.shields.io/badge/license-WPL%201.0-red.svg)](LICENSE)

> 🧪 **Estado:** Beta  
> 🖥️ **Plataforma:** Windows x64  
> 📦 **Versión:** Beta 1.2 — “Quality Grid”

## ✨ Características

- 🎚️ **Quality Grid:** selector organizado de formato, calidad, resolución y opciones disponibles según el contenido.
- ⚡ **Descargas en segundo plano:** mejor gestión de hilos y una interfaz más fluida mientras se procesa una descarga.
- 🗂️ **Organización automática:** guarda las descargas en la carpeta `Descargas\WASSLink` por defecto.
- 🖥️ **Aplicación portable:** no requiere instalador ni permisos administrativos.
- 📦 **Runtime incluido:** el paquete publicado incluye el runtime de .NET; no es necesario instalarlo por separado.
- 🔄 **Motor de descarga:** utiliza componentes de terceros como `yt-dlp` y `FFmpeg` para inspeccionar y procesar contenido compatible.
- 🎨 **Interfaz mejorada:** ajustes en navegación, estados de carga, insignias de versión y renderizado general.

## 🚀 Novedades de Beta 1.2

### Quality Grid

La nueva cuadrícula de calidad facilita comparar y elegir las opciones disponibles antes de iniciar una descarga:

- Formato de salida.
- Calidad de audio o vídeo.
- Resolución disponible.
- Opciones específicas según el contenido.

### Mejoras de estabilidad

- Procesamiento más fluido de las descargas.
- Mejor administración de los hilos de ejecución.
- Mejor respuesta de la interfaz durante una descarga.
- Corrección de interrupciones ocasionales al procesar determinadas URL.
- Correcciones menores de renderizado y estabilidad general.

Consulta el [changelog completo de Beta 1.2](https://github.com/WilmerWass/WASSLink-Studio/releases/tag/Beta_1.2).

## 📥 Descarga e instalación

1. Descarga [`WASSLink-Studio-win-x64.zip`](https://github.com/WilmerWass/WASSLink-Studio/releases/tag/Beta_1.2) desde la release.
2. Extrae **todo** el contenido del ZIP en una carpeta local.
3. Conserva la carpeta `tools` junto a `WASSLink.Desktop.exe`.
4. Ejecuta `WASSLink.Desktop.exe`.

La aplicación es portable: no crea accesos directos, asociaciones de archivos ni actualizaciones automáticas. Windows puede mostrar una advertencia de SmartScreen porque esta versión beta no está firmada digitalmente.

> 🌐 Se requiere conexión a Internet para inspeccionar y descargar contenido.

## 📁 Estructura del paquete publicado

```text
WASSLink-Desktop-win-x64/
├── WASSLink.Desktop.exe       # Aplicación principal
├── tools/                     # Herramientas auxiliares y motores incluidos
├── WASSLink-Desktop-README.txt
└── THIRD-PARTY-NOTICES.txt    # Avisos y licencias de terceros
```

Las descargas se guardan por defecto en:

```text
%USERPROFILE%\Downloads\WASSLink
```

## ⚖️ Uso legal y responsabilidad

WASSLink Studio es una herramienta de propósito general. Úsala únicamente con contenido que tengas derecho a descargar, guardar o procesar. El usuario es responsable de cumplir la legislación aplicable, los términos de servicio de cada plataforma y los derechos de terceros.

El programa no concede derechos sobre contenido de terceros ni está diseñado para fomentar la piratería o la infracción de derechos de autor.

## 📄 Licencia

El código y los componentes originales de WASSLink Studio se distribuyen bajo la **WASSLink Proprietary License (WPL) 1.0**. No es una licencia de código abierto: la redistribución, modificación, comercialización e ingeniería inversa están restringidas salvo autorización escrita o cuando la ley aplicable disponga lo contrario.

Consulta el texto completo en [`LICENSE`](LICENSE). Las herramientas y dependencias de terceros conservan sus propias licencias; consulta [`release/THIRD-PARTY-NOTICES.txt`](release/THIRD-PARTY-NOTICES.txt).

## 🐛 Comentarios y errores

Esta es una versión beta y puede incluir errores o cambios incompletos. Si encuentras un problema, abre un [issue](https://github.com/WilmerWass/WASSLink-Studio/issues) incluyendo:

- Versión de Windows.
- Pasos para reproducirlo.
- Mensaje de error o captura, si corresponde.
- URL de ejemplo únicamente cuando tengas derecho a compartirla.

---

**Copyright © 2026 WilmerWassPC — Todos los derechos reservados.**
