# WASSLink Architecture

> Documento oficial de arquitectura del proyecto.

## Arquitectura general

WASSLink Studio utiliza una arquitectura modular basada en capas.

La soluciÃ³n se divide en:

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
- PresentaciÃ³n.
- ComposiciÃ³n de servicios.

Las Apps no deben contener lÃ³gica de negocio central.

---

# Core

El Core contiene la lÃ³gica reutilizable del ecosistema.

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

- ConfiguraciÃ³n.
- Preferencias.
- Carga y almacenamiento de configuraciÃ³n.

Dependencias:

- WASSLink.Abstractions
- WASSLink.Shared

---

## WASSLink.Download

Responsabilidad:

- GestiÃ³n de descargas.
- AbstracciÃ³n de motores de descarga.
- IntegraciÃ³n con proveedores y motores externos.

Dependencias:

- WASSLink.Abstractions
- WASSLink.Configuration
- WASSLink.Shared

Capacidades futuras contempladas:

- HTTP/HTTPS.
- Integraciones especializadas.
- Torrents mediante una implementaciÃ³n independiente.

La arquitectura no debe acoplar el mÃ³dulo directamente a un motor torrent especÃ­fico.

---

## WASSLink.Library

Responsabilidad:

- Biblioteca multimedia.
- OrganizaciÃ³n.
- Metadatos.
- GestiÃ³n del contenido descargado.

Dependencias:

- WASSLink.Abstractions
- WASSLink.Shared
- WASSLink.Configuration

---

## WASSLink.Player

Responsabilidad:

- ReproducciÃ³n multimedia.
- Control del reproductor.
- IntegraciÃ³n con motores multimedia.

Dependencias:

- WASSLink.Abstractions
- WASSLink.Shared

---

## WASSLink.Search

Responsabilidad:

- BÃºsqueda.
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

Apps â†’ Core

Core â†’ Core inferior

Tests â†’ proyectos que prueban

No permitido:

Core â†’ Apps

Core â†’ UI

Core â†’ implementaciones externas concretas cuando exista una abstracciÃ³n adecuada

---

# Flujo principal

Usuario

â†“

App

â†“

Servicios Core

â†“

Proveedor / motor externo

â†“

Biblioteca

â†“

Player

---

# Plugins

Los plugins implementan contratos definidos en WASSLink.Abstractions.

Los mÃ³dulos internos no deben depender directamente de plugins concretos.

Esto permite:

- Extensibilidad.
- Bajo acoplamiento.
- SustituciÃ³n de proveedores.
- IntegraciÃ³n futura de nuevos mecanismos de descarga.

---

# Principio fundamental

El Core define **quÃ©** debe hacerse.

Las implementaciones externas definen **cÃ³mo** se realiza.

Esto permite evolucionar WASSLink Studio sin reconstruir la arquitectura alrededor de una tecnologÃ­a especÃ­fica.

---

# Estado

VersiÃ³n: 1.1

Fecha: 2026-08-03

Fase 1: completada.
