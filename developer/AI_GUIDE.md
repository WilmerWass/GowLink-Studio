# WASSLink Studio â€” AI Development Guide

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
- AÃ±adir dependencias innecesarias.
- Eliminar funcionalidades existentes sin aprobaciÃ³n.
- Marcar como implementada una capacidad que solo estÃ¡ planificada.

## Descargas

La arquitectura de descarga debe mantenerse extensible.

Las implementaciones especÃ­ficas deben depender de contratos.

Ejemplo conceptual:

IDownloadProvider
â†“
HTTP Provider
Torrent Provider
External Engine Provider

No debe acoplarse todo el sistema a un Ãºnico proveedor.

## Reproductor

El reproductor es una capacidad principal de WASSLink Studio.

No debe eliminarse simplemente para convertir el proyecto en un gestor de descargas.

El objetivo es integrar:

Descarga â†’ Biblioteca â†’ ReproducciÃ³n.

## Plugins

Las extensiones deben utilizar contratos definidos en WASSLink.Abstractions.

## Cambios

Antes de modificar arquitectura:

1. Revisar documentaciÃ³n.
2. Revisar referencias de proyectos.
3. Revisar cÃ³digo existente.
4. Implementar el cambio mÃ­nimo.
5. Compilar.
6. Ejecutar pruebas.
7. Revisar Git diff.

## ValidaciÃ³n

DespuÃ©s de cambios estructurales:

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

Planificado â‰  Implementado.

Una funcionalidad solamente se considera implementada cuando existe cÃ³digo funcional y validaciÃ³n.
