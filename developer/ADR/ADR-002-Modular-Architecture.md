# ADR-002 - Arquitectura Modular

Estado: 🔒 Aprobado

Fecha: 2026-08-03

---

# Contexto

GowLink Studio está concebido como una plataforma multimedia modular. Para garantizar su mantenibilidad, escalabilidad y facilidad de pruebas, la solución debe dividirse en módulos con responsabilidades bien definidas.

---

# Decisión

La solución se compondrá de proyectos independientes, organizados por dominio funcional.

Cada proyecto tendrá una única responsabilidad y solo podrá depender de los módulos necesarios.

La interfaz gráfica, la consola y las futuras aplicaciones móviles reutilizarán los mismos servicios y componentes.

---

# Arquitectura Inicial

- `WASSLink.Abstractions` (nombre técnico actual)
- `WASSLink.Shared` (nombre técnico actual)
- `WASSLink.Configuration` (nombre técnico actual)
- `WASSLink.Download` (nombre técnico actual)
- `WASSLink.Library` (nombre técnico actual)
- `WASSLink.Player` (nombre técnico actual)
- `WASSLink.Search` (nombre técnico actual)
- `WASSLink.Plugins` (nombre técnico actual)
- `WASSLink.Studio` (nombre técnico histórico/previsto)
- `WASSLink.CLI` (nombre técnico actual)
- `WASSLink.Tests` (nombre técnico actual)

La marca de producto vigente es GowLink Studio. Esta ADR conserva los identificadores que describen la solución técnica actual; su rebranding requiere una migración coordinada posterior.

---

# Principios

- Responsabilidad única.
- Bajo acoplamiento.
- Alta cohesión.
- Reutilización.
- Arquitectura desacoplada.
- Interfaces antes que implementaciones.
- Preparado para pruebas unitarias.

---

# Consecuencias

Cada nuevo módulo deberá justificar su existencia.

No se permitirá crear proyectos "cajón de sastre".

Toda dependencia entre módulos deberá quedar documentada en ARCHITECTURE.md.

---

# Estado

Esta decisión forma parte de la arquitectura base del proyecto.