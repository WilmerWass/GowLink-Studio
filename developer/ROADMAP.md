# WASSLink Studio Roadmap

## Fase 0 â€” PreparaciÃ³n

Estado: completada.

Objetivos:

- Definir identidad del proyecto.
- Establecer documentaciÃ³n inicial.
- Preparar repositorio.
- Definir principios de desarrollo.

---

# Fase 1 â€” Arquitectura y estructura

Estado: COMPLETADA

Objetivos alcanzados:

- Crear soluciÃ³n .NET 10.
- Crear estructura Apps/Core/Tests.
- Crear proyectos Core.
- Crear WASSLink.Desktop.
- Crear WASSLink.CLI.
- Crear proyecto de Tests.
- Definir referencias entre proyectos.
- Validar mapa de dependencias.
- Crear infraestructura inicial de Dependency Injection.
- Integrar Avalonia.
- Validar compilaciÃ³n completa.

Resultado:

La soluciÃ³n compila correctamente mediante:

dotnet build WASSLink-Studio.slnx

---

# Fase 2 â€” Core funcional

Estado: planificada.

Objetivos:

- Definir contratos reales.
- Implementar configuraciÃ³n.
- Implementar modelos multimedia.
- Implementar biblioteca.
- Definir abstracciones de descarga.
- Definir abstracciones del reproductor.
- Definir contratos de bÃºsqueda.
- Mejorar Dependency Injection.

---

# Fase 3 â€” Descargas

Estado: planificada.

Objetivo:

Construir un sistema de descargas extensible.

Capacidades previstas:

- HTTP.
- HTTPS.
- Descargas mediante motores externos.
- GestiÃ³n de progreso.
- CancelaciÃ³n.
- Reintentos.
- Cola de descargas.
- Historial.

Capacidad futura:

- Torrent mediante integraciÃ³n especializada.

La compatibilidad torrent deberÃ¡ desarrollarse como una implementaciÃ³n desacoplada del Core.

---

# Fase 4 â€” Biblioteca multimedia

Estado: planificada.

Objetivos:

- ImportaciÃ³n.
- IndexaciÃ³n.
- Metadatos.
- OrganizaciÃ³n.
- BÃºsqueda local.
- Historial.
- Favoritos.

---

# Fase 5 â€” Reproductor

Estado: planificada.

Objetivos:

- ReproducciÃ³n de audio.
- ReproducciÃ³n de vÃ­deo.
- Controles.
- Cola.
- PosiciÃ³n de reproducciÃ³n.
- Volumen.
- IntegraciÃ³n con biblioteca.

El reproductor permanece como componente central de WASSLink Studio.

---

# Fase 6 â€” Sistema de Plugins

Estado: planificada.

Objetivos:

- Descubrimiento de plugins.
- Registro.
- Ciclo de vida.
- Versionado.
- Proveedores de bÃºsqueda.
- Proveedores de descarga.
- Extensiones multimedia.

---

# Fase 7 â€” IntegraciÃ³n

Estado: futura.

Objetivo:

Unificar:

BÃºsqueda
â†“
Descarga
â†“
Biblioteca
â†“
ReproducciÃ³n

---

# Fase 8 â€” Multiplataforma

Estado: futura.

Objetivos:

- Windows.
- Linux.
- macOS.
- Posible Android.
- Posible Mobile.

---

# Principio

No se implementarÃ¡ una capacidad Ãºnicamente porque aparezca en el roadmap.

Cada capacidad deberÃ¡ pasar por:

DiseÃ±o â†’ ImplementaciÃ³n â†’ IntegraciÃ³n â†’ Pruebas â†’ DocumentaciÃ³n
