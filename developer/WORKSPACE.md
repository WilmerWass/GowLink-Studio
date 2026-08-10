# WASSLink Studio Workspace

## Repositorio

WASSLink-Studio

## Estructura

src/
â”œâ”€â”€ Apps/
â”‚   â”œâ”€â”€ WASSLink.CLI/
â”‚   â””â”€â”€ WASSLink.Desktop/
â”‚
â””â”€â”€ Core/
    â”œâ”€â”€ WASSLink.Abstractions/
    â”œâ”€â”€ WASSLink.Configuration/
    â”œâ”€â”€ WASSLink.Download/
    â”œâ”€â”€ WASSLink.Library/
    â”œâ”€â”€ WASSLink.Player/
    â”œâ”€â”€ WASSLink.Plugins/
    â”œâ”€â”€ WASSLink.Search/
    â””â”€â”€ WASSLink.Shared/

tests/
â””â”€â”€ WASSLink.Tests/

developer/
â””â”€â”€ DocumentaciÃ³n del proyecto

## Aplicaciones

### WASSLink.Desktop

AplicaciÃ³n grÃ¡fica basada en Avalonia.

### WASSLink.CLI

AplicaciÃ³n de lÃ­nea de comandos.

## Core

El Core contiene la lÃ³gica reutilizable.

## Flujo conceptual

BÃºsqueda
â†“
Descarga
â†“
Biblioteca
â†“
ReproducciÃ³n

## Capacidades futuras

- Descargas HTTP/HTTPS.
- Torrents.
- Proveedores adicionales.
- Plugins.
- Multiplataforma.

## Estado

Fase 1 completada.

La soluciÃ³n compila correctamente:

dotnet build WASSLink-Studio.slnx

## Regla de trabajo

Antes de modificar el proyecto:

1. Revisar arquitectura.
2. Revisar referencias.
3. Revisar documentaciÃ³n.
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
