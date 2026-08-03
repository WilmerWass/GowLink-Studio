# ADR-002 - Arquitectura Modular

Estado: 🔒 Aprobado

Fecha: 2026-08-03

---

# Contexto

WASSLink está concebido como una plataforma multimedia que crecerá durante muchos años. Para garantizar su mantenibilidad, escalabilidad y facilidad de pruebas, la solución debe dividirse en módulos con responsabilidades bien definidas.

---

# Decisión

La solución se compondrá de proyectos independientes, organizados por dominio funcional.

Cada proyecto tendrá una única responsabilidad y solo podrá depender de los módulos necesarios.

La interfaz gráfica, la consola y las futuras aplicaciones móviles reutilizarán los mismos servicios y componentes.

---

# Arquitectura Inicial

- WASSLink.Abstractions
- WASSLink.Shared
- WASSLink.Configuration
- WASSLink.Download
- WASSLink.Library
- WASSLink.Player
- WASSLink.Search
- WASSLink.Plugins
- WASSLink.Studio
- WASSLink.CLI
- WASSLink.Tests

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