# Refactor Overview

Este repositorio está siendo reorganizado para mejorar claridad y alinearse con convenciones comunes.

Cambios principales realizados en esta etapa:

- Se crean carpetas raíz: `src/`, `tests/` y `docs/`.
- Se añade este documento con notas de refactor iniciales.

Próximos pasos planeados:

1. Mover proyectos existentes dentro de `src/` y renombrar carpetas en inglés (`users-service.application` -> `Application`, `users-service.domain` -> `Domain`, `users-service.infrastructure` -> `Infrastructure`, `users-service.api` -> `Api`, etc.).
2. Actualizar `*.csproj` y el `users-service.api.sln` para apuntar a las nuevas rutas.
3. Actualizar namespaces dentro de los archivos C# para reflejar los nuevos nombres.
4. Añadir proyecto de tests en `tests/` y agregarlo a la solución.

Notas de compatibilidad:

- Tras el movimiento de archivos, se ejecutará `dotnet build` para localizar y corregir referencias rotas.

Si quieres que proceda con mover los proyectos ahora, dime y avanzo con la siguiente etapa.

---
Fechado: 2025-11-15

