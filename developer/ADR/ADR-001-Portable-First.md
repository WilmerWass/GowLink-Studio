# ADR-001 - GowLink Studio: filosofía Portable First

Estado: ✅ Aprobado

Fecha: 2026-08-02

---

# Contexto

Uno de los principales objetivos de GowLink Studio es ofrecer la mayor libertad posible al usuario.

Durante el diseño del proyecto surgió la necesidad de decidir si GowLink sería únicamente una aplicación portable o una aplicación instalable.

---

# Decisión

GowLink adopta oficialmente la filosofía **Portable First**.

Esto significa que el modo de funcionamiento principal será Portable, permitiendo ejecutar la aplicación sin instalación.

Sin embargo, el proyecto también ofrecerá un modo Instalado para aquellos usuarios que deseen una mayor integración con el sistema operativo.

---

# Modos oficiales

## Portable

- No requiere instalación.
- Puede ejecutarse desde cualquier carpeta o dispositivo externo.
- Toda la configuración puede permanecer junto al programa.

## Instalado

- Integración con Windows.
- Inicio automático.
- Accesos directos.
- Asociación de archivos.
- Actualizaciones automáticas (futuro).
- Servicios en segundo plano (cuando sea necesario).

---

# Motivo

Se prioriza la libertad del usuario.

Cada usuario podrá elegir el modo que mejor se adapte a sus necesidades sin perder funcionalidades principales.

---

# Consecuencias

La arquitectura deberá diseñarse desde el inicio para soportar ambos modos sin duplicar código.

La configuración, biblioteca y datos del usuario deberán estar desacoplados del ejecutable.

---

# Estado

Esta decisión se considera Arquitectura Base del proyecto.
