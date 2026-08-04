# WASSLink Studio - Workspace

> Documento oficial que describe la organización física del repositorio.
> Todo desarrollador, colaborador o IA deberá respetar esta estructura.

---

# Filosofía

La organización del repositorio sigue el principio:

> Cada carpeta tiene una única responsabilidad.

No se crearán carpetas nuevas sin una necesidad real y documentada.

---

# Estructura General

```text
WASSLink-Studio
│
├── .github/
├── assets/
├── developer/
├── docs/
├── examples/
├── plugins/
├── resources/
├── scripts/
├── src/
├── tests/
├── tools/
│
├── README.md
├── LICENSE
├── .gitignore
├── .editorconfig
├── .gitattributes
└── WASSLink-Studio.sln
```

---

# Descripción de Carpetas

## .github/

Configuración del repositorio GitHub.

Contendrá:

- Workflows
- Issue Templates
- Pull Request Templates
- CODEOWNERS
- Discussions (configuración)

---

## assets/

Recursos gráficos del proyecto.

Ejemplos:

- Logos
- Iconos
- Imágenes
- Branding

---

## developer/

Documentación técnica para el desarrollo.

Ejemplos:

- Manifest
- ADR
- Guías para IA
- Roadmap
- Workspace
- Changelog

---

## docs/

Documentación pública destinada a los usuarios.

No debe contener documentación interna.

---

## examples/

Ejemplos oficiales.

Ejemplos de:

- Plugins
- Configuración
- Uso de API
- Casos de integración

---

## plugins/

Plugins oficiales y de terceros.

Cada plugin será independiente del núcleo del sistema.

---

## resources/

Recursos utilizados por la aplicación.

Ejemplos:

- Traducciones
- Temas
- Plantillas
- Recursos embebidos

---

## scripts/

Scripts oficiales para desarrolladores.

Ejemplos:

- setup.ps1
- build.ps1
- clean.ps1
- doctor.ps1

Estos scripts no forman parte del producto final.

---

## src/

Código fuente.

Se divide en dos grupos.

### Apps/

Aplicaciones finales.

- WASSLink.Desktop
- WASSLink.CLI
- WASSLink.Mobile

### Core/

Bibliotecas compartidas.

Incluye toda la lógica del sistema.

---

## tests/

Pruebas automatizadas.

No debe contener código de producción.

---

## tools/

Herramientas externas.

Ejemplos:

- yt-dlp
- FFmpeg
- MPV (futuro)

---

# Reglas de Organización

1. Una carpeta = una responsabilidad.

2. No duplicar archivos.

3. No almacenar archivos temporales.

4. No guardar binarios dentro de src.

5. Toda nueva carpeta importante deberá documentarse aquí.

6. Toda modificación estructural deberá registrarse mediante un ADR o actualizar este documento.

---

# Estado

Versión: 1.0.0

Estado: Aprobado

Última actualización: 2026-08-03
