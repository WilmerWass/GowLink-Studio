# Changelog

Todos los cambios relevantes de WASSLink Studio se documentan aquÃ­.

## [Unreleased]

### Arquitectura

- ConsolidaciÃ³n de la arquitectura modular.
- OrganizaciÃ³n Apps / Core / Tests.
- Infraestructura inicial de Dependency Injection.
- IntegraciÃ³n inicial de WASSLink.Desktop.
- IntegraciÃ³n inicial de WASSLink.CLI.
- ValidaciÃ³n de compilaciÃ³n completa.

### Producto

WASSLink Studio mantiene el concepto de Media Studio.

El producto integra progresivamente:

- Descarga.
- Biblioteca multimedia.
- ReproducciÃ³n.
- BÃºsqueda.
- Plugins.

Se contempla la futura incorporaciÃ³n de diferentes mecanismos de descarga, incluyendo torrents.

La compatibilidad torrent permanece planificada hasta contar con una implementaciÃ³n funcional y pruebas.

## Fase 1

### Completada

La Fase 1 estableciÃ³:

- SoluciÃ³n .NET 10.
- Estructura modular.
- Proyectos Core.
- Aplicaciones Desktop y CLI.
- Proyecto de Tests.
- Referencias entre proyectos.
- Dependency Injection inicial.
- IntegraciÃ³n Avalonia.
- CompilaciÃ³n completa.

ValidaciÃ³n:

dotnet build WASSLink-Studio.slnx

Resultado:

CompilaciÃ³n realizada correctamente.

## Convenciones

Los cambios deben agruparse por funcionalidad y mantenerse coherentes con la arquitectura documentada.
