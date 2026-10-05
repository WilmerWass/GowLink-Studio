# 🎵 GowLink Studio — Beta 1.2 "Quality Grid"

> Aplicación de escritorio **portable para Windows x64** que permite gestionar y descargar audio y vídeo desde fuentes compatibles, con selección de calidad, procesamiento en segundo plano y una interfaz sencilla.

[![Release](https://img.shields.io/github/v/release/WilmerWass/WASSLink-Studio?include_prereleases&label=release)](https://github.com/WilmerWass/WASSLink-Studio/releases/tag/Beta_1.2)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)

> 🧪 **Estado:** Beta  
> 🖥️ **Plataforma:** Windows x64  
> 📦 **Versión:** Beta 1.2 — «Quality Grid»

## ✨ Características

- 🎚️ **Quality Grid:** selector organizado de formato, calidad, resolución y opciones disponibles según el contenido.
- ⚡ **Descargas en segundo plano:** mejor gestión de hilos y una interfaz más fluida mientras se procesa una descarga.
- 📂 **Organización automática:** guarda las descargas en la carpeta `Descargas\GowLink` por defecto.
- 🖥️ **Aplicación portable:** no requiere instalador ni permisos administrativos.
- 📦 **Runtime incluido:** el paquete publicado incluye el runtime de .NET; no es necesario instalarlo por separado.
- 🔧 **Motor de descarga:** utiliza componentes de terceros como `yt-dlp` y `FFmpeg` para inspeccionar y procesar contenido compatible.
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
GowLink-Desktop-win-x64/
├── GowLink.Desktop.exe       # Aplicación principal
├── tools/                     # Herramientas auxiliares y motores incluidos
├── GowLink-Desktop-README.txt
└── THIRD-PARTY-NOTICES.txt    # Avisos y licencias de terceros
```

Las descargas se guardan por defecto en:

```text
%USERPROFILE%\Downloads\GowLink
```

## 🔒 Seguridad y confiabilidad

✅ **Fuente oficial:**  
Descarga siempre desde https://github.com/WilmerWass/WASSLink-Studio/releases

⚠️ **Advertencia:**  
Descargas de sitios de terceros no verificados pueden incluir virus o modificaciones maliciosas. Verifica la integridad del archivo descargado.

🔍 **Verificación:**  
- Descarga únicamente desde el repositorio oficial.
- Compara el hash SHA-256 del ZIP con el publicado en la release.

## ⚖️ Uso legal y responsabilidad

GowLink Studio es una herramienta tecnológica de propósito general. No promueve, autoriza ni garantiza la descarga, copia o distribución no autorizada de contenido.

El usuario es el único responsable de:

- El contenido que descargue, almacene, reproduzca o comparta.
- Contar con los permisos o derechos necesarios.
- Cumplir la legislación aplicable y los términos de servicio de las plataformas utilizadas.

GowLink Studio no concede derechos de propiedad intelectual sobre contenido perteneciente a terceros.

## 📄 Licencia

GowLink Studio se distribuye bajo la **[Licencia Pública General GNU v3.0 (GPL-3.0)](LICENSE)**.

**Resumido:**
- ✅ Eres libre de usar, modificar y distribuir el software.
- ✅ Las versiones derivadas también deben ser GPL-3.0.
- ❌ No puedes venderlo ni ocultarlo como propietario sin abrir el código.
- ℹ️ Se proporciona sin garantías. Lee la licencia completa para detalles.

Las herramientas y dependencias de terceros (`yt-dlp`, FFmpeg, Avalonia, .NET) conservan sus propias licencias.  
Consulta [`release/THIRD-PARTY-NOTICES.txt`](release/THIRD-PARTY-NOTICES.txt) para más información.

## 🐛 Comentarios y errores

Esta es una versión beta y puede incluir errores o cambios incompletos. Si encuentras un problema, abre un [issue](https://github.com/WilmerWass/WASSLink-Studio/issues) incluyendo:

- Versión de Windows.
- Pasos para reproducirlo.
- Mensaje de error o captura, si corresponde.
- URL de ejemplo únicamente cuando tengas derecho a compartirla.

---

**GowLink Studio** © 2026 WilmerWassPC
Distribuido bajo [GPL-3.0](LICENSE)
