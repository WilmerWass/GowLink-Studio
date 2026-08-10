$ErrorActionPreference = "Stop"

$Root = Split-Path -Parent $PSScriptRoot
$Developer = Join-Path $Root "developer"

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host " WASSLink Studio - Developer Docs Updater" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

if (-not (Test-Path $Developer)) {
    throw "No existe la carpeta developer: $Developer"
}

function Write-Doc {
    param(
        [string]$Name,
        [string]$Content
    )

    $Path = Join-Path $Developer $Name

    Write-Host "Actualizando $Name..." -ForegroundColor Yellow

    Set-Content `
        -Path $Path `
        -Value $Content `
        -Encoding UTF8

    Write-Host "  OK" -ForegroundColor Green
}

Write-Doc "PROJECT_MANIFEST.md" @"
# WASSLink Studio

## Identidad del proyecto

WASSLink Studio es una plataforma multimedia modular orientada a la gestión de contenido digital.

El proyecto integra progresivamente:

- Descarga de contenido.
- Gestión de biblioteca multimedia.
- Reproducción multimedia.
- Búsqueda y proveedores.
- Sistema de plugins.
- Configuración.
- Interfaces Desktop y CLI.
- Futuras capacidades multiplataforma.

La aplicación no se limita a ser un reproductor ni únicamente un gestor de descargas.

Su objetivo es convertirse en un **Media Studio** capaz de centralizar el flujo:

Usuario
↓
Búsqueda / Entrada
↓
Descarga
↓
Biblioteca
↓
Reproducción

## Capacidades principales

### Descargas

La arquitectura contempla una capa de descarga extensible.

Fuentes y mecanismos contemplados:

- HTTP/HTTPS.
- Motores externos.
- Proveedores especializados.
- Torrents mediante una futura integración especializada.

La compatibilidad con torrents es una capacidad arquitectónica prevista y no debe considerarse implementada hasta que exista una implementación funcional y pruebas correspondientes.

### Biblioteca

La biblioteca organiza el contenido descargado y sus metadatos.

### Reproductor

El reproductor multimedia es una parte fundamental de WASSLink Studio.

Permite que el usuario pueda reproducir contenido gestionado por la plataforma sin depender de una aplicación externa para completar el flujo principal.

### Plugins

El sistema de plugins permite ampliar las capacidades del producto sin acoplar el Core a implementaciones concretas.

## Arquitectura

La solución utiliza una arquitectura modular:

- Apps
- Core
- Tests

Las aplicaciones finales consumen servicios del Core.

El Core contiene contratos, servicios y lógica reutilizable.

## Tecnologías

Tecnologías principales previstas:

- .NET 10
- C#
- Avalonia UI
- SQLite
- FFmpeg
- yt-dlp
- LibVLC / MPV
- Microsoft.Extensions.DependencyInjection

Las tecnologías externas pueden cambiar durante la evolución del proyecto.

## Estado actual

Fase 1: completada.

La Fase 1 establece:

- Estructura de solución.
- Proyectos Core.
- Aplicaciones Desktop y CLI.
- Tests.
- Arquitectura inicial.
- Referencias entre proyectos.
- Infraestructura inicial de Dependency Injection.
- Compilación completa de la solución.

## Principio de evolución

WASSLink Studio se desarrolla de forma incremental.

Una capacidad se considera implementada únicamente cuando existe código funcional, integración y validación.

La documentación puede describir capacidades futuras siempre que estén claramente identificadas como planificadas.
"@

Write-Doc "ARCHITECTURE.md" @"
# WASSLink Architecture

> Documento oficial de arquitectura del proyecto.

## Arquitectura general

WASSLink Studio utiliza una arquitectura modular basada en capas.

La solución se divide en:

- Apps
- Core
- Tests

---

# Apps

Las Apps contienen los puntos de entrada de usuario.

Proyectos actuales:

- WASSLink.Desktop
- WASSLink.CLI

Proyecto futuro:

- WASSLink.Mobile

Las Apps son responsables de:

- Interfaz de usuario.
- Entrada del usuario.
- Presentación.
- Composición de servicios.

Las Apps no deben contener lógica de negocio central.

---

# Core

El Core contiene la lógica reutilizable del ecosistema.

## WASSLink.Abstractions

Responsabilidad:

- Interfaces.
- Contratos.
- Definiciones compartidas.

Dependencias:

- Ninguna.

---

## WASSLink.Shared

Responsabilidad:

- Utilidades comunes.
- Extensiones.
- Modelos base.

Dependencias:

- WASSLink.Abstractions

---

## WASSLink.Configuration

Responsabilidad:

- Configuración.
- Preferencias.
- Carga y almacenamiento de configuración.

Dependencias:

- WASSLink.Abstractions
- WASSLink.Shared

---

## WASSLink.Download

Responsabilidad:

- Gestión de descargas.
- Abstracción de motores de descarga.
- Integración con proveedores y motores externos.

Dependencias:

- WASSLink.Abstractions
- WASSLink.Configuration
- WASSLink.Shared

Capacidades futuras contempladas:

- HTTP/HTTPS.
- Integraciones especializadas.
- Torrents mediante una implementación independiente.

La arquitectura no debe acoplar el módulo directamente a un motor torrent específico.

---

## WASSLink.Library

Responsabilidad:

- Biblioteca multimedia.
- Organización.
- Metadatos.
- Gestión del contenido descargado.

Dependencias:

- WASSLink.Abstractions
- WASSLink.Shared
- WASSLink.Configuration

---

## WASSLink.Player

Responsabilidad:

- Reproducción multimedia.
- Control del reproductor.
- Integración con motores multimedia.

Dependencias:

- WASSLink.Abstractions
- WASSLink.Shared

---

## WASSLink.Search

Responsabilidad:

- Búsqueda.
- Proveedores.
- Resultados.

Dependencias:

- WASSLink.Abstractions
- WASSLink.Shared
- WASSLink.Plugins

---

## WASSLink.Plugins

Responsabilidad:

- Sistema de extensiones.
- Contratos de plugins.
- Registro y descubrimiento de extensiones.

Dependencias:

- WASSLink.Abstractions
- WASSLink.Shared

---

# Tests

Los proyectos de Tests validan el comportamiento del Core y sus integraciones.

Las pruebas no deben introducir dependencias innecesarias hacia las Apps.

---

# Regla de dependencias

Permitido:

Apps → Core

Core → Core inferior

Tests → proyectos que prueban

No permitido:

Core → Apps

Core → UI

Core → implementaciones externas concretas cuando exista una abstracción adecuada

---

# Flujo principal

Usuario

↓

App

↓

Servicios Core

↓

Proveedor / motor externo

↓

Biblioteca

↓

Player

---

# Plugins

Los plugins implementan contratos definidos en WASSLink.Abstractions.

Los módulos internos no deben depender directamente de plugins concretos.

Esto permite:

- Extensibilidad.
- Bajo acoplamiento.
- Sustitución de proveedores.
- Integración futura de nuevos mecanismos de descarga.

---

# Principio fundamental

El Core define **qué** debe hacerse.

Las implementaciones externas definen **cómo** se realiza.

Esto permite evolucionar WASSLink Studio sin reconstruir la arquitectura alrededor de una tecnología específica.

---

# Estado

Versión: 1.1

Fecha: 2026-08-03

Fase 1: completada.
"@

Write-Doc "ROADMAP.md" @"
# WASSLink Studio Roadmap

## Fase 0 — Preparación

Estado: completada.

Objetivos:

- Definir identidad del proyecto.
- Establecer documentación inicial.
- Preparar repositorio.
- Definir principios de desarrollo.

---

# Fase 1 — Arquitectura y estructura

Estado: COMPLETADA

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

La solución compila correctamente mediante:

dotnet build WASSLink-Studio.slnx

---

# Fase 2 — Core funcional

Estado: planificada.

Objetivos:

- Definir contratos reales.
- Implementar configuración.
- Implementar modelos multimedia.
- Implementar biblioteca.
- Definir abstracciones de descarga.
- Definir abstracciones del reproductor.
- Definir contratos de búsqueda.
- Mejorar Dependency Injection.

---

# Fase 3 — Descargas

Estado: planificada.

Objetivo:

Construir un sistema de descargas extensible.

Capacidades previstas:

- HTTP.
- HTTPS.
- Descargas mediante motores externos.
- Gestión de progreso.
- Cancelación.
- Reintentos.
- Cola de descargas.
- Historial.

Capacidad futura:

- Torrent mediante integración especializada.

La compatibilidad torrent deberá desarrollarse como una implementación desacoplada del Core.

---

# Fase 4 — Biblioteca multimedia

Estado: planificada.

Objetivos:

- Importación.
- Indexación.
- Metadatos.
- Organización.
- Búsqueda local.
- Historial.
- Favoritos.

---

# Fase 5 — Reproductor

Estado: planificada.

Objetivos:

- Reproducción de audio.
- Reproducción de vídeo.
- Controles.
- Cola.
- Posición de reproducción.
- Volumen.
- Integración con biblioteca.

El reproductor permanece como componente central de WASSLink Studio.

---

# Fase 6 — Sistema de Plugins

Estado: planificada.

Objetivos:

- Descubrimiento de plugins.
- Registro.
- Ciclo de vida.
- Versionado.
- Proveedores de búsqueda.
- Proveedores de descarga.
- Extensiones multimedia.

---

# Fase 7 — Integración

Estado: futura.

Objetivo:

Unificar:

Búsqueda
↓
Descarga
↓
Biblioteca
↓
Reproducción

---

# Fase 8 — Multiplataforma

Estado: futura.

Objetivos:

- Windows.
- Linux.
- macOS.
- Posible Android.
- Posible Mobile.

---

# Principio

No se implementará una capacidad únicamente porque aparezca en el roadmap.

Cada capacidad deberá pasar por:

Diseño → Implementación → Integración → Pruebas → Documentación
"@

Write-Doc "GLOSSARY.md" @"
# WASSLink Studio Glossary

## App

Aplicación final utilizada por el usuario.

Ejemplos:

- WASSLink.Desktop
- WASSLink.CLI

## Core

Conjunto de proyectos que contienen contratos, servicios y lógica reutilizable.

## Download

Sistema responsable de gestionar descargas.

Puede utilizar diferentes motores y proveedores.

## Torrent

Mecanismo de distribución de archivos basado en BitTorrent.

En WASSLink Studio se considera una capacidad futura del sistema de descargas.

## Library

Biblioteca multimedia donde se organiza el contenido gestionado por WASSLink.

## Player

Componente encargado de reproducir contenido multimedia.

## Plugin

Extensión que implementa contratos definidos por WASSLink.

## Provider

Componente que proporciona una capacidad concreta, como búsqueda, descarga o metadatos.

## Abstraction

Contrato que define una capacidad sin depender de una implementación concreta.

## Dependency Injection

Patrón utilizado para proporcionar dependencias a los componentes en lugar de crearlas directamente.

## Media

Contenido multimedia como:

- Audio.
- Vídeo.
- Otros formatos compatibles.

## Metadata

Información asociada al contenido:

- Título.
- Artista.
- Álbum.
- Duración.
- Formato.
- Tamaño.
- Fecha.

## External Engine

Software externo utilizado para realizar una capacidad especializada.

Ejemplos:

- FFmpeg.
- yt-dlp.
- LibVLC.
- MPV.

## Media Studio

Concepto de producto de WASSLink Studio.

Representa la integración de:

Descarga + Biblioteca + Reproducción + Gestión multimedia.

## CLI

Command Line Interface.

Interfaz de línea de comandos.

## Desktop

Aplicación gráfica de escritorio basada en Avalonia.

## DTO

Data Transfer Object.

Objeto utilizado para transportar información entre componentes.

## DI

Dependency Injection.

Inyección de dependencias.

## F1

Fase 1 del desarrollo del proyecto.

En esta fase se estableció la arquitectura inicial y estructura de solución.
"@

Write-Doc "LEGAL_PRINCIPLES.md" @"
# WASSLink Studio — Legal Principles

## Propósito

WASSLink Studio es un software destinado a gestionar contenido multimedia y proporcionar herramientas técnicas para búsqueda, descarga, organización y reproducción.

## Responsabilidad del usuario

El usuario es responsable de utilizar WASSLink Studio de acuerdo con:

- Legislación aplicable.
- Derechos de autor.
- Licencias.
- Términos de servicio de los proveedores.
- Derechos de terceros.

## Descargas

La existencia de una capacidad técnica para descargar contenido no implica autorización legal para descargar cualquier contenido.

WASSLink Studio no debe presentar una capacidad técnica como permiso legal.

## Torrents

El protocolo BitTorrent tiene usos legítimos y no legítimos.

La integración de torrents, si se implementa, debe considerarse una tecnología de transferencia de datos y no una autorización para obtener contenido protegido.

## Contenido multimedia

El usuario debe asegurarse de poseer los derechos necesarios o contar con autorización para almacenar, modificar, distribuir o reproducir el contenido gestionado.

## Servicios externos

WASSLink puede utilizar servicios y herramientas externas.

Su utilización debe respetar las licencias correspondientes y los términos aplicables.

Ejemplos:

- FFmpeg.
- yt-dlp.
- LibVLC.
- MPV.

## Plugins

Los plugins de terceros pueden tener sus propias licencias y condiciones.

El sistema de plugins no convierte automáticamente sus componentes en parte del software propietario del proyecto.

## Principio de neutralidad tecnológica

WASSLink Studio proporciona infraestructura técnica.

La responsabilidad sobre el contenido utilizado mediante dicha infraestructura corresponde al usuario y al contexto legal aplicable.

## Documentación

Estos principios son documentación técnica y de producto.

No constituyen asesoramiento jurídico.
"@

Write-Doc "AI_GUIDE.md" @"
# WASSLink Studio — AI Development Guide

## Objetivo

Este documento define las reglas para utilizar herramientas de IA durante el desarrollo de WASSLink Studio.

## Prioridades

La IA debe priorizar:

1. Arquitectura.
2. Correctitud.
3. Seguridad.
4. Mantenibilidad.
5. Simplicidad.
6. Rendimiento.
7. Velocidad de desarrollo.

## Reglas

La IA no debe:

- Inventar APIs.
- Inventar archivos existentes.
- Cambiar arquitectura sin justificarlo.
- Añadir dependencias innecesarias.
- Eliminar funcionalidades existentes sin aprobación.
- Marcar como implementada una capacidad que solo está planificada.

## Descargas

La arquitectura de descarga debe mantenerse extensible.

Las implementaciones específicas deben depender de contratos.

Ejemplo conceptual:

IDownloadProvider
↓
HTTP Provider
Torrent Provider
External Engine Provider

No debe acoplarse todo el sistema a un único proveedor.

## Reproductor

El reproductor es una capacidad principal de WASSLink Studio.

No debe eliminarse simplemente para convertir el proyecto en un gestor de descargas.

El objetivo es integrar:

Descarga → Biblioteca → Reproducción.

## Plugins

Las extensiones deben utilizar contratos definidos en WASSLink.Abstractions.

## Cambios

Antes de modificar arquitectura:

1. Revisar documentación.
2. Revisar referencias de proyectos.
3. Revisar código existente.
4. Implementar el cambio mínimo.
5. Compilar.
6. Ejecutar pruebas.
7. Revisar Git diff.

## Validación

Después de cambios estructurales:

dotnet build WASSLink-Studio.slnx

Y cuando existan pruebas:

dotnet test WASSLink-Studio.slnx

## Git

Antes de commit:

git status
git diff

Los commits deben representar cambios coherentes.

## Regla principal

No confundir:

Planificado ≠ Implementado.

Una funcionalidad solamente se considera implementada cuando existe código funcional y validación.
"@

Write-Doc "WORKSPACE.md" @"
# WASSLink Studio Workspace

## Repositorio

WASSLink-Studio

## Estructura

src/
├── Apps/
│   ├── WASSLink.CLI/
│   └── WASSLink.Desktop/
│
└── Core/
    ├── WASSLink.Abstractions/
    ├── WASSLink.Configuration/
    ├── WASSLink.Download/
    ├── WASSLink.Library/
    ├── WASSLink.Player/
    ├── WASSLink.Plugins/
    ├── WASSLink.Search/
    └── WASSLink.Shared/

tests/
└── WASSLink.Tests/

developer/
└── Documentación del proyecto

## Aplicaciones

### WASSLink.Desktop

Aplicación gráfica basada en Avalonia.

### WASSLink.CLI

Aplicación de línea de comandos.

## Core

El Core contiene la lógica reutilizable.

## Flujo conceptual

Búsqueda
↓
Descarga
↓
Biblioteca
↓
Reproducción

## Capacidades futuras

- Descargas HTTP/HTTPS.
- Torrents.
- Proveedores adicionales.
- Plugins.
- Multiplataforma.

## Estado

Fase 1 completada.

La solución compila correctamente:

dotnet build WASSLink-Studio.slnx

## Regla de trabajo

Antes de modificar el proyecto:

1. Revisar arquitectura.
2. Revisar referencias.
3. Revisar documentación.
4. Implementar.
5. Compilar.
6. Probar.
7. Revisar Git.

## Comandos principales

Compilar:

dotnet build WASSLink-Studio.slnx

Probar:

dotnet test WASSLink-Studio.slnx

Ver estado:

git status

Ver cambios:

git diff

Ver proyectos:

dotnet sln WASSLink-Studio.slnx list
"@

Write-Doc "CHANGELOG.md" @"
# Changelog

Todos los cambios relevantes de WASSLink Studio se documentan aquí.

## [Unreleased]

### Arquitectura

- Consolidación de la arquitectura modular.
- Organización Apps / Core / Tests.
- Infraestructura inicial de Dependency Injection.
- Integración inicial de WASSLink.Desktop.
- Integración inicial de WASSLink.CLI.
- Validación de compilación completa.

### Producto

WASSLink Studio mantiene el concepto de Media Studio.

El producto integra progresivamente:

- Descarga.
- Biblioteca multimedia.
- Reproducción.
- Búsqueda.
- Plugins.

Se contempla la futura incorporación de diferentes mecanismos de descarga, incluyendo torrents.

La compatibilidad torrent permanece planificada hasta contar con una implementación funcional y pruebas.

## Fase 1

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

Resultado:

Compilación realizada correctamente.

## Convenciones

Los cambios deben agruparse por funcionalidad y mantenerse coherentes con la arquitectura documentada.
"@

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host " Documentos actualizados correctamente" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Archivos actualizados:" -ForegroundColor White
Write-Host "  AI_GUIDE.md"
Write-Host "  ARCHITECTURE.md"
Write-Host "  CHANGELOG.md"
Write-Host "  GLOSSARY.md"
Write-Host "  LEGAL_PRINCIPLES.md"
Write-Host "  PROJECT_MANIFEST.md"
Write-Host "  ROADMAP.md"
Write-Host "  WORKSPACE.md"
Write-Host ""
Write-Host "No se realizó ningún commit ni push." -ForegroundColor Yellow
Write-Host ""