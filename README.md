# GowLink Studio — Beta 1.4.1: Universal Download Engine

> Aplicación de escritorio **portable para Windows x64** que permite gestionar y descargar audio y vídeo desde fuentes compatibles, con selección de calidad, procesamiento en segundo plano y una interfaz intuitiva multilenguaje.
>
> **Descarga. Conecta. Organiza. Reproduce.**

[![Release](https://img.shields.io/github/v/release/WilmerWass/GowLink-Studio?include_prereleases&label=latest%20public%20release)](https://github.com/WilmerWass/GowLink-Studio/releases)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)

> 🧪 **Estado:** Beta  
> 🖥️ **Plataforma:** Windows x64  
> 📦 **Versión de trabajo:** Beta 1.4.1 — Universal Download Engine<br>
> La Beta 1.4.1 aún no está publicada como release.

## ✨ Características

- 🔗 **Motor dual:** descarga archivos directos por HTTP y contenido multimedia mediante yt-dlp y FFmpeg.
- 📥 **Cola común:** controla las descargas activas y configura entre una y tres tareas simultáneas.
- 🎚️ **Quality Grid:** selecciona entre formatos de audio y vídeo que yt-dlp detecta para el enlace.
- 📂 **Biblioteca local:** consulta los archivos guardados en `Downloads\GowLink` y ábrelos desde la aplicación.
- 🔎 **Búsqueda local:** filtra archivos por nombre y categoría; la búsqueda externa se abre en Bing solo cuando la eliges.
- ⚙️ **Preferencias persistentes:** conserva carpeta, límite de velocidad, concurrencia, color de acento e historial terminal en `%LOCALAPPDATA%\GowLink`.
- 🖥️ **Aplicación portable:** no requiere instalador ni permisos administrativos.
- 📦 **Runtime incluido:** el paquete publicado incluye el runtime de .NET; no es necesario instalarlo por separado.
- 📝 **Logs centralizados:** todos los registros se guardan en `%LOCALAPPDATA%\GowLink\Logs` para facilitar diagnósticos.

## Novedades de Beta 1.4.1

- Cola FIFO en memoria con límite configurable de concurrencia y cancelación.
- Reintentos acotados para fallos transitorios; los errores permanentes y de almacenamiento no se reintentan.
- Archivos HTTP escritos primero a temporales para no dejar un parcial con el nombre final.
- Historial y preferencias terminales guardados localmente; las URL persistidas y los logs omiten query y fragmento.
- Controles explícitos para resolver archivos HTTP duplicados, cierre de paneles y color de acento consistente.
- Tamaños de formatos multimedia etiquetados como estimados cuando yt-dlp aporta una cifra, o desconocidos si no.

La compatibilidad depende del extractor instalado y no se garantiza para todas las URL aceptadas por yt-dlp.

## 📥 Descarga e instalación

### Opción 1: Descarga portable (recomendado)

1. La Beta 1.4.1 aún no tiene un paquete publicado. Consulta [Releases](https://github.com/WilmerWass/GowLink-Studio/releases) para la última versión pública.
2. Extrae **todo** el contenido del ZIP en una carpeta local.
3. Conserva la carpeta `tools` junto a `WASSLink.Desktop.exe`.
4. Ejecuta `WASSLink.Desktop.exe`.

### Opción 2: Desde el repositorio (para desarrolladores)

```bash
git clone https://github.com/WilmerWass/GowLink-Studio.git
cd GowLink-Studio
# Sigue las instrucciones en CONTRIBUTING.md para compilar
```

Para publicar una rama de trabajo por primera vez, usa `git push -u origin nombre-de-tu-rama`.

La aplicación es portable: no crea accesos directos, asociaciones de archivos ni actualizaciones automáticas. Windows puede mostrar una advertencia de SmartScreen porque esta versión beta no es certificada; puedes ignorarla si descargas desde el repositorio oficial.

> 🌐 Se requiere conexión a Internet para inspeccionar y descargar contenido.

## 📁 Estructura del paquete publicado

```text
GowLink-Desktop-win-B1.4.1/
├── WASSLink.Desktop.exe       # Ejecutable; nombre técnico heredado
├── LEEME_README.txt           # Instrucciones portables (ES / EN / JA)
├── THIRD-PARTY-NOTICES.txt    # Avisos y licencias de terceros
├── NOVEDADES_NEWS.txt         # Novedades de Beta 1.4.1 (ES / EN / JA)
└── tools/
    ├── ffmpeg/ffmpeg.exe
    └── yt-dlp/yt-dlp.exe
```

El artefacto portable de Windows x64 se genera como
`GowLink-Desktop-win-B1.4.1.zip`. Los tres documentos de texto de la raíz están
organizados en español, inglés y japonés.

Las descargas se guardan por defecto en:

```text
%USERPROFILE%\Downloads\GowLink
```
Para cambiar la ruta donde se guardaras tus archivos descargados dale click donde dice examinar y seleciona o crea una nueva carpeta

Los logs se guardan en:

```text
%LOCALAPPDATA%\GowLink\Logs
```

## 🔒 Seguridad y confiabilidad

✅ **Fuente oficial:**  
Descarga siempre desde https://github.com/WilmerWass/GowLink-Studio/releases

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

Esta es una versión beta y puede incluir errores o cambios incompletos. Si encuentras un problema, abre un [issue](https://github.com/WilmerWass/GowLink-Studio/issues) incluyendo:

- Versión de Windows.
- Pasos para reproducirlo.
- Mensaje de error o captura, si corresponde.
- URL de ejemplo únicamente cuando tengas derecho a compartirla.

---

**GowLink Studio** © 2026 WilmerWassPC  
Distribuido bajo [GPL-3.0](LICENSE)
