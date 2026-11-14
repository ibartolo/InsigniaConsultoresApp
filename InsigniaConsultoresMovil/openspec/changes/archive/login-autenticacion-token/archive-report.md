# Informe de Archivo — `login-autenticacion-token`

**Fecha de archivo**: 2026-11-14
**Modo de almacenamiento de artefactos**: openspec
**Veredicto de verificación**: PASS WITH WARNINGS (build-only; sin proyecto de tests)

---

## Resumen

Cambio de **fundación de autenticación** para la app móvil de Insignia Consultores AXA. Sustituyó la plantilla MAUI por defecto por una base de autenticación OAuth2 (*password grant*) con MVVM simple, DI, persistencia híbrida del token (SecureStorage + SQLite), auto-login por expiración local, identidad visual de marca (ícono, splash, paleta) y Home con Shell Flyout. Es el **primer archivo** del proyecto: `openspec/specs/` estaba vacío, por lo que cada spec delta es una spec completa y pasa a ser la spec base (fuente de verdad).

## Especificaciones sincronizadas (delta → specs base)

| Dominio (capability) | Acción | Requerimientos | Escenarios |
|----------------------|--------|----------------|------------|
| `branding` | Creado (base) | 4 | 6 |
| `autenticacion-token` | Creado (base) | 10 | 16 |
| `inicio-y-navegacion` | Creado (base) | 4 | 4 |

### Rutas (fuente de verdad)

- `openspec/specs/branding/spec.md`
- `openspec/specs/autenticacion-token/spec.md`
- `openspec/specs/inicio-y-navegacion/spec.md`

## Contenido del archivo

| Artefacto | Estado |
|-----------|--------|
| `proposal.md` | ✅ |
| `specs/` (branding, autenticacion-token, inicio-y-navegacion) | ✅ |
| `design.md` | ✅ |
| `tasks.md` | ✅ (36/36 tareas + Lote 5 completo) |
| `verify-report.md` | ✅ |
| `apply-progress.md` | ✅ (Lotes 1–5) |
| `archive-report.md` | ✅ (este archivo) |

## Verificación

- **Build**: `dotnet build -f net10.0-android` → SUCCESS (0 errores; Debug y Release).
- **Tests**: no disponibles (sin proyecto de tests; `strict_tdd=false`).
- **Cumplimiento**: 16/17 requerimientos implementados; 23/24 escenarios cubiertos estáticamente; 1 diferido (AT-R8).
- **Prueba real**: login verificado en dispositivo físico **Motorola moto g56 (Android 16)** contra `https://webapi.insigniaconsultores.com.mx` — funciona.

## Problemas conocidos (deuda registrada — no corregidos en este archivo)

1. **Pendiente cosmético — doble línea en los inputs del login**: los campos muestran la línea nativa de Android + la línea de marca. Se intentó quitar con un *handler mapper* en `MauiProgram`, lo que provocó un crash de arranque (`IllegalArgumentException: This component requires that you specify a valid TextAppearance attribute` en `PlatformInterop.createNavigationBar` → `BottomNavigationView` → `ShellItemRenderer`). El mapper fue **REVERTIDO**; la app arranca bien con la doble línea. Queda como deuda.
2. **NU1903 (CVE-2025-6965)** en `SQLitePCLRaw.lib.e_sqlite3.android` 2.1.11 — sin parche upstream disponible (SQLite < 3.50.2; "Patched versions: None"). El warning no se suprime.
3. **Parpadeo breve del login** en arranque en frío con sesión válida (flash de la pantalla de login antes del auto-login a Home).
4. **`AT-R8` (401 en request protegida) diferido**: no existe request protegida en este cambio. Lo cubre el change `campanas-whatsapp`.
5. **Verificación build-only**: no hay proyecto de tests; los escenarios de runtime se validaron manualmente en hardware.

## Riesgos / observaciones

- **Discrepancia de URL del backend en la spec**: `autenticacion-token/spec.md`, `design.md` y `proposal.md` referencian `https://api.insigniaconsultores.com.mx/Token`, pero el código implementado (`AuthConfig.BaseUrl`) y la prueba real usan `https://webapi.insigniaconsultores.com.mx`. La spec base quedó con `api.` (copia verbatim del delta); se recomienda reconciliar la URL en un cambio futuro.
- **Solapamiento futuro de `inicio-y-navegacion`**: el change `campanas-whatsapp` también aporta un delta para `inicio-y-navegacion`; al archivarlo deberá **fusionarse** con la spec base creada aquí (no se tocó en este archivo).
- **Placeholders pendientes**: `ApplicationId` (`com.companyname.insigniaconsultoresapp`) y `ApplicationTitle` (`InsigniaConsultoresApp`) siguen como placeholder (fuera de alcance de este cambio).

## Conclusión

El ciclo SDD del change `login-autenticacion-token` queda **completo**: planificado, implementado, verificado (build + prueba en dispositivo real) y archivado. Las specs base quedan establecidas como fuente de verdad. Sin issues críticos pendientes.
