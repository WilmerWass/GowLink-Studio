# GowLink Studio — Estado y deuda técnica

## 1. Funcionalidades implementadas y verificadas

El estado de trabajo de Beta 1.4.1 incluye:

- branding actualizado a GowLink Studio
- ruta de destino por defecto en `Downloads\GowLink`
- navegación principal entre módulos
- motor activo dinámico según la URL
- descarga HTTP directa con manejo básico de progreso
- integración del motor yt-dlp con flujo de descarga
- cola FIFO con concurrencia de una a tres tareas y reintentos transitorios acotados
- temporales HTTP y opciones explícitas de conflicto de archivo directo
- persistencia local de ajustes e historial terminal; URLs guardadas sin query
- build Release y 45 pruebas automatizadas pasando en la revisión actual

La publicación portable no está confirmada y la Beta 1.4.1 no se ha publicado.
Tampoco se declara compatibilidad universal con URLs o proveedores externos.

## 2. Funcionalidades parcialmente confirmadas

- pausa/reanudación del flujo HTTP en todos los escenarios
- integración entre módulos de biblioteca, buscador y descarga
- compatibilidad con plataformas externas por URL
- gestión de duplicados y nombres de salida multimedia de yt-dlp
- prueba de instalación portable fuera del repositorio

## 3. Problemas conocidos

- completar la gestión uniforme de conflictos entre HTTP y yt-dlp
- algunas pantallas o módulos se comportan bien como UI, pero aún no tienen validación profunda de integración
- la compatibilidad con proveedores externos no debe asumirse a partir de la detección superficial
- la documentación interna aún necesita distinguir claramente entre “estado real” y “visión futura”

## 4. Deuda técnica

- definir criterios reales para “compatible” frente a “detectable”
- separar claramente UI/UX de comportamiento funcional real
- mantener versiones y hitos con criterios objetivos de aceptación

## 5. Riesgos de mantenimiento

- sobreprometer compatibilidad por una sola funcionalidad verificada
- mezclar mejoras de UX con nuevas capacidades sin separar hitos
- dejar la documentación en un estado histórico pero no operativo
- aceptar una integración prematura de motores sin pruebas de regresión

## 6. Pruebas ejecutadas

- `dotnet build WASSLink-Studio.slnx -c Release --no-restore`: compilación correcta, sin warnings
- `dotnet test tests\WASSLink.Tests\WASSLink.Tests.csproj -c Release`: 45 pruebas correctas
- `dotnet build WASSLink-Studio.slnx -c Debug --no-restore` en salida aislada: compilación correcta, sin warnings
- `dotnet test tests\WASSLink.Tests\WASSLink.Tests.csproj -c Debug --no-build --no-restore` en salida aislada: 45 pruebas correctas

Estos resultados no sustituyen la validación del paquete portable ni las pruebas de compatibilidad de proveedores externos.

## 7. Siguientes acciones recomendadas

1. validar el build Debug y el paquete portable en staging limpio
2. mantener la separación entre mejora y nueva funcionalidad
3. fijar criterios de aceptación para 1.5
4. no anunciar compatibilidad universal a proveedores externos sin pruebas
5. mantener la documentación interna alineada con la realidad del proyecto
