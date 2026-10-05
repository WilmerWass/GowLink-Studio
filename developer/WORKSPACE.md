# GowLink Studio — Workspace

## Repositorio

`WilmerWass/WASSLink-Studio` (slug histórico del repositorio).

Nombre oficial del producto: GowLink Studio.

Los nombres técnicos de solución, proyectos y namespaces siguen usando `WASSLink.*`; su migración está pendiente.

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

### GowLink Desktop (`WASSLink.Desktop`)

AplicaciÃ³n grÃ¡fica basada en Avalonia.

### GowLink CLI (`WASSLink.CLI`)

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

Fase 1 completada. Beta 1.3 está en estabilización; no iniciar Beta 2 todavía.

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

Última ejecución registrada: 20/20 tests superados. La suite automatizada no sustituye la validación de descarga real extremo a extremo.

Ejecutar Desktop:

dotnet run --project src\Apps\WASSLink.Desktop\WASSLink.Desktop.csproj

Ejecutar CLI:

dotnet run --project src\Apps\WASSLink.CLI\WASSLink.CLI.csproj

Nomenclatura de ejecución: GowLink Studio es el producto; GowLink Desktop, GowLink CLI y GowLink Downloader son los nombres visibles. Los proyectos, namespaces y ensamblados `WASSLink.*` son identificadores técnicos heredados hasta una migración coordinada.

Ver estado:

git status

Ver cambios:

git diff

Ver proyectos:

dotnet sln WASSLink-Studio.slnx list
