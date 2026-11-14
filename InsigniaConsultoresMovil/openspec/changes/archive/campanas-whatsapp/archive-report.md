# Informe de Archivo — `campanas-whatsapp`

**Fecha de archivo**: 2026-11-14
**Modo de almacenamiento de artefactos**: openspec
**Veredicto de verificación**: PASS WITH WARNINGS (build-only; sin proyecto de tests; runtime en dispositivo físico)

---

## Resumen

Cambio que lleva el outreach de WhatsApp del web MVC al teléfono: un asesor autenticado ve sus campañas WhatsApp (`Canal == 2`), abre su detalle, prepara y lee destinatarios, y dispara envíos 1 a 1 por WhatsApp registrando el estado (Pendiente/Enviado) contra `https://webapi.insigniaconsultores.com.mx`. Introduce una nueva capability `campanas-whatsapp` (8 requerimientos, CW-R1..CW-R8) y modifica `inicio-y-navegacion` (ítem "Campañas" en el Flyout + rutas Shell de las vistas de campañas).

## Especificaciones sincronizadas (delta → specs base)

| Dominio (capability) | Acción | Requerimientos antes | Requerimientos después | Escenarios antes | Escenarios después |
|----------------------|--------|----------------------|------------------------|------------------|--------------------|
| `campanas-whatsapp` | Creado (base) | — | 8 (CW-R1..CW-R8) | — | 23 |
| `inicio-y-navegacion` | Fusionado (merge) | 4 | 5 (IN-R1..IN-R5) | 4 | 6 |

Detalle del merge de `inicio-y-navegacion`:

- **MODIFIED** `IN-R2` (Contenido del Flyout): ahora incluye el ítem "Campañas" (visible solo con sesión activa).
- **ADDED** `IN-R5` (Navegación a la lista de campañas): rutas Shell de las vistas de campañas + back sin regreso al login.
- Requerimientos `IN-R1` (Home), `IN-R3` (Área placeholder) e `IN-R4` (Navegación sin regreso al login) **intactos**.

### Rutas (fuente de verdad)

- `openspec/specs/campanas-whatsapp/spec.md` (nueva)
- `openspec/specs/inicio-y-navegacion/spec.md` (fusionada)

## Contenido del archivo

| Artefacto | Estado |
|-----------|--------|
| `proposal.md` | ✅ |
| `specs/` (campanas-whatsapp, inicio-y-navegacion) | ✅ |
| `design.md` | ✅ |
| `tasks.md` | ✅ (17/17 tareas de implementación [1.1–7.1]; Fase 8 completada vía build + prueba manual en dispositivo) |
| `verify-report.md` | ✅ |
| `apply-progress.md` | ✅ (Lotes 1–5, Etapas 5–7) |
| `archive-report.md` | ✅ (este archivo) |

## Verificación

- **Build**: `dotnet build -f net10.0-android` → SUCCESS (0 errores, 2 advertencias `NU1903` aceptadas).
- **Tests**: no disponibles (sin proyecto de tests; `strict_tdd=false`).
- **Cumplimiento**: 20/26 escenarios ✅ con evidencia runtime; 4 ⚠️ static-only (CW-R8 ×2, IN-R2, más no-ejecutados documentados); 0 ❌.
- **Prueba real**: walkthrough completo en **Motorola moto g56 5G (Android 16)** contra `https://webapi.insigniaconsultores.com.mx` — lista filtrada, detalle, preparar/leer destinatarios, búsqueda local, envío por `wa.me/52…`, deshacer y navegación Flyout.

## Correcciones aplicadas durante la prueba en dispositivo (resueltas)

1. **Parsing tolerante de `Preparar` y `Destinatarios`**: la respuesta de `Preparar` puede ser un arreglo (no la envoltura esperada) y `Destinatarios` puede variar; el servicio parsea el JSON directamente (`FindArray`/`TryGetProperty`).
2. **`Launcher.Default.OpenAsync` + `<queries>`**: por la visibilidad de paquetes de Android 11+, se usa `OpenAsync` (no `TryOpenAsync`) y se añadió `<queries>` al manifest para poder abrir `wa.me`.
3. **Texto de WhatsApp = `Descripcion`**: el deep link ahora precarga `Descripcion` (con fallback a `Asunto`).
4. **Guard de carga única**: `_loaded`/`OnAppearing` evita la carrera de recarga obsoleta al volver a las pantallas.
5. **Username del flyout ya no se limpia en pantallas de campaña**: la condición de refresco del usuario se extendió a las rutas de campaña (W-1 del verify-report queda resuelto).

## Problemas conocidos (deuda registrada — no corregidos en este archivo)

1. **NU1903 (CVE-2025-6965)** en `SQLitePCLRaw.lib.e_sqlite3.android` 2.1.11 — sin parche upstream ("Patched versions: None"). Aceptado.
2. **Doble línea inferior en los inputs del login** — pre-existente (del change `login-autenticacion-token`), fuera del alcance de este cambio.
3. **`"0000000000"` pasa la regla de 10 dígitos** — cualquier cadena de 10 dígitos se considera enviable. El usuario ya fue informado; decisión pendiente (validación fuera del alcance "más allá de 10 dígitos" del proposal).
4. **CW-R8 (401 → limpiar sesión → login) implementado pero no probado en runtime** — no se reprodujo la expiración de token en dispositivo (verificado solo estáticamente).
5. **Sugerencias del verify-report dejadas como están**:
   - `CanSend` omite `!IsSent` (inofensivo: el botón "Enviar" se oculta cuando está enviado).
   - `?text=` siempre se agrega al URI `wa.me` (cosmético).
   - `FechaEnvio` optimista usa UTC (`DateTime.UtcNow`) en vez de hora local.
   - `ModelResponseDto` sin uso (código muerto).
   - Deshacer no es optimista (más simple y seguro, sin regresión funcional).
   - Se eliminó el error-kind `Business` (`isSuccess=false` y 4xx no-401 se clasifican `Server`/`Malformed`).

## Riesgos / observaciones

- **Verificación build-only**: no hay proyecto de tests; los escenarios de runtime se validaron manualmente en hardware.
- **Sin confirmación real de envío dentro de WhatsApp**: el envío se marca Enviado tras lanzar el deep link (aceptado por el usuario en el proposal).
- **Backlog para la próxima sesión**: envío masivo secuencial con intervalo configurable (~3 min por cliente). **NO implementar ahora** (fuera del alcance de este cambio).

## Conclusión

El ciclo SDD del change `campanas-whatsapp` queda **completo**: planificado, implementado, verificado (build + walkthrough en dispositivo real) y archivado. Las specs base quedan actualizadas como fuente de verdad. Sin issues críticos pendientes.
