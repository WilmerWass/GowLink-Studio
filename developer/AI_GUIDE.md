# GowLink Studio — Guía de desarrollo con IA

## Objetivo

Este documento define las reglas para utilizar herramientas de IA durante el desarrollo de GowLink Studio.

## Identidad del producto

La marca oficial actual es **GowLink Studio**. El producto de escritorio activo se presenta como:

**GowLink Downloader**

Las aplicaciones se presentan como GowLink Desktop y GowLink CLI. Los nombres de solución, carpetas, proyectos, ensamblados y namespaces `WASSLink.*` todavía son identificadores técnicos existentes; no deben renombrarse parcialmente. Su migración requiere un cambio coordinado y validado que aún está pendiente.

Lema oficial:

"Descarga lo que quieras, cuando quieras y como quieras."

GowLink Studio es el nombre de la suite completa.

El lema representa libertad de uso, NO una promesa técnica absoluta ni autorización para evadir restricciones técnicas, DRM o legales.

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
- Implementar mecanismos para evadir DRM, autenticación, restricciones de acceso o protecciones técnicas.

## Dependency Injection

NO crear instancias de servicios con `new` desde ViewModels cuando el servicio puede proporcionarse mediante DI.

Correcto:

    public MainViewModel(IDownloadProvider downloadProvider) { ... }

Incorrecto:

    private readonly YtDlpDownloadService _service = new();

Antes de añadir un servicio, revisar si ya existe infraestructura DI equivalente.

## Descargas

La arquitectura de descarga debe mantenerse extensible.

Las implementaciones específicas deben depender de contratos:

IDownloadProvider
↓
YtDlpDownloadProvider  (Beta 1 — implementado)
HttpDownloadProvider   (futuro)
TorrentProvider        (Beta 8 — planificado)

No debe acoplarse todo el sistema a un único proveedor.

Los proveedores se registran mediante DI.

## Reproductor

El reproductor es una capacidad futura de GowLink Studio (Beta 3).

No debe eliminarse del roadmap simplemente para convertir el proyecto en un gestor de descargas.

El objetivo es integrar:

Descarga → Biblioteca → Reproducción.

## Betas

El proyecto se desarrolla incrementalmente. El estado actual es Beta 1.3, en estabilización; no avanzar a Beta 2 hasta completar y validar Beta 1:

- Beta 1 — Downloader (en estabilización)
- Beta 2 — Search Studio
- Beta 3 — Media Player
- Beta 4 — Library Studio
- Beta 5 — Multi-Platform Providers
- Beta 6 — Plugin System
- Beta 7 — File Studio
- Beta 8 — Torrent Studio
- Beta 9 — Linux Edition
- Beta 10 — Mobile / Android
- V2 — GowLink Media Studio

NO implementar betas futuras hasta que la beta actual tenga una base sólida y funcional.

Quality Grid y selección de formatos están implementados. El progreso, la cancelación, la interfaz Desktop/CLI y las rutas de logging tienen implementación; la suite actual cuenta con 19 pruebas. Esto no equivale a validar una descarga real extremo a extremo ni a certificar todos los fallos de producción. Una sub-beta solo se marca completa cuando sus criterios funcionales están integrados y probados.

## Plugins

Las extensiones deben utilizar los contratos existentes en el proyecto técnico `WASSLink.Abstractions` hasta que se apruebe y ejecute la migración de identificadores.

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

La última ejecución registrada para Beta 1.3 pasó las 19 pruebas. Repetir ambos comandos tras cambios estructurales o de código.

## Git

Antes de commit:

git status
git diff

Los commits deben representar cambios coherentes.

NO hacer commits automáticamente sin mostrar primero los cambios realizados.

Si existen modificaciones locales del usuario, NO sobrescribirlas sin analizarlas.

## Regla principal

No confundir:

Planificado ≠ Implementado.

Una funcionalidad solamente se considera implementada cuando existe código funcional y validación.
