# 🎵 GowLink Studio — Beta 1.3 "Estabilización"

> Aplicación de escritorio **portable para Windows x64** que permite gestionar y descargar audio y vídeo desde fuentes compatibles, con selección de calidad, procesamiento en segundo plano y una interfaz intuitiva multilenguaje.
>
> **Descarga. Conecta. Organiza. Reproduce.**

[![Release](https://img.shields.io/github/v/release/WilmerWass/GowLink-Studio?include_prereleases&label=release)](https://github.com/WilmerWass/GowLink-Studio/releases/tag/v1.3.0-beta)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)

> 🧪 **Estado:** Beta  
> 🖥️ **Plataforma:** Windows x64  
> 📦 **Versión:** Beta 1.3 — «Estabilización»

## ✨ Características

- 🎚️ **Quality Grid:** selector organizado de formato, calidad, resolución y opciones disponibles según el contenido.
- ⚡ **Descargas en segundo plano:** mejor gestión de hilos y una interfaz más fluida mientras se procesa una descarga.
- 📂 **Organización automática:** guarda las descargas en la carpeta `%LOCALAPPDATA%\GowLink\Descargas` por defecto.
- 🖥️ **Aplicación portable:** no requiere instalador ni permisos administrativos.
- 📦 **Runtime incluido:** el paquete publicado incluye el runtime de .NET; no es necesario instalarlo por separado.
- 🔧 **Motor de descarga:** utiliza componentes de terceros como `yt-dlp` y `FFmpeg` con soporte completo para UTF-8 en rutas y metadatos.
- 📝 **Logs centralizados:** todos los registros se guardan en `%LOCALAPPDATA%\GowLink\Logs` para facilitar diagnósticos.
- 🌐 **Interfaz multilenguaje:** soporte para Español, Inglés y Japonés.
- 🎨 **Interfaz mejorada:** ajustes en navegación, estados de carga, insignias de versión y renderizado general.

## 🚀 Novedades de Beta 1.3 "Estabilización"

### 🏢 Rebranding completo a GowLink Studio

La aplicación se redenomina como **GowLink Studio** para reflejar una nueva identidad visual y corporativa:

- Nuevo nombre oficial y tagline: "Descarga. Conecta. Organiza. Reproduce."
- Actualización de logos, iconos e identidad visual.
- URLs actualizadas a `github.com/WilmerWass/GowLink-Studio`.

### ⚙️ Estabilización del Engine

El motor de descarga basado en `yt-dlp` y `FFmpeg` alcanza madurez con:

- Soporte completo para UTF-8 en rutas de archivo y metadatos.
- Manejo mejorado de caracteres especiales y nombres no latinos.
- Compatibilidad extendida con formatos y plataformas.

### 📝 Centralización de Logs

Los registros de aplicación se unifican en una ubicación estándar:

- Ubicación centralizada: `%LOCALAPPDATA%\GowLink\Logs`
- Mejor formato de registro para diagnósticos y debugging.
- Rotación automática de archivos de log antiguos.

### 🌐 Interfaz y Documentación Trilingüe

Expansión del alcance lingüístico:

- **Español:** interfaz y documentación completa.
- **Inglés:** soporte para usuarios anglófonos.
- **Japonés:** localización para mercados de Asia Oriental.
- Badges de estado en múltiples idiomas.

### Mejoras de estabilidad

- Correcciones de interrupciones ocasionales al procesar determinadas URL.
- Mejor administración de los hilos de ejecución.
- Mejor respuesta de la interfaz durante una descarga.
- Correcciones menores de renderizado y estabilidad general.

Consulta el [changelog completo de Beta 1.3](https://github.com/WilmerWass/GowLink-Studio/releases/tag/v1.3.0-beta).

## 📥 Descarga e instalación

### Opción 1: Descarga portable (recomendado)

1. Descarga [`GowLink-Desktop-win-B1.3.zip`](https://github.com/WilmerWass/GowLink-Studio/releases/tag/v1.3.0-beta) desde la release (≈106 MB).
2. Extrae **todo** el contenido del ZIP en una carpeta local.
3. Conserva la carpeta `tools` junto a `GowLink.Desktop.exe`.
4. Ejecuta `GowLink.Desktop.exe`.

### Opción 2: Desde el repositorio (para desarrolladores)

```bash
git clone https://github.com/WilmerWass/GowLink-Studio.git
cd GowLink-Studio
# Sigue las instrucciones en CONTRIBUTING.md para compilar
```

La aplicación es portable: no crea accesos directos, asociaciones de archivos ni actualizaciones automáticas. Windows puede mostrar una advertencia de SmartScreen porque esta versión beta no es certificada; puedes ignorarla si descargas desde el repositorio oficial.

> 🌐 Se requiere conexión a Internet para inspeccionar y descargar contenido.

## 📁 Estructura del paquete publicado

```text
GowLink-Desktop-win-B1.3/
├── GowLink.Desktop.exe       # Aplicación principal
├── tools/                    # Herramientas auxiliares y motores incluidos
│   ├── yt-dlp.exe
│   ├── ffmpeg.exe
│   └── [otros binarios]
├── GowLink-Desktop-README.txt
└── THIRD-PARTY-NOTICES.txt   # Avisos y licencias de terceros
```

Las descargas se guardan por defecto en:

```text
%LOCALAPPDATA%\GowLink\Descargas
```

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
