# Informe de Archivo — `indicadores-home`

**Fecha de archivo**: 2026-11-17
**Modo de almacenamiento de artefactos**: openspec
**Veredicto de verificación**: PASS WITH WARNINGS (0 CRITICAL; build verde; verificación visual en dispositivo NO posible)

---

## Resumen

Cambio que reemplaza el placeholder del Home por una sección de 5 indicadores (KPIs) del negocio con selector de rango de fechas (Desde/Hasta), etiqueta de periodo, pull-to-refresh y manejo de estados (carga/error/401). Introduce la capability `indicadores-home` (6 requerimientos, IH-R1..IH-R6) y modifica `inicio-y-navegacion` (el requisito "Área de contenido placeholder" pasa a mostrar los KPIs).

## Especificaciones sincronizadas (delta → specs base)

| Dominio (capability) | Acción | Requerimientos antes | Requerimientos después | Escenarios antes | Escenarios después |
|----------------------|--------|----------------------|------------------------|------------------|--------------------|
| `indicadores-home` | Creado (base) | — | 6 (IH-R1..IH-R6) | — | 14 |
| `inicio-y-navegacion` | Fusionado (merge) | 5 | 5 | 6 | 6 |

Detalle del merge de `inicio-y-navegacion`:

- **MODIFIED** "Área de contenido placeholder" (se conserva el nombre exacto del requisito): ahora DEBE mostrar la sección de indicadores (KPIs) de la spec `indicadores-home` en lugar del área en blanco.
- Requerimientos restantes intactos: "Home con menú hamburguesa", "Contenido del Flyout", "Navegación sin regreso al login" y "Navegación a la lista de campañas".

### Rutas (fuente de verdad)

- `openspec/specs/indicadores-home/spec.md` (nueva)
- `openspec/specs/inicio-y-navegacion/spec.md` (fusionada)

## Contenido del archivo

| Artefacto | Estado |
|-----------|--------|
| `proposal.md` | ✅ |
| `specs/` (indicadores-home, inicio-y-navegacion) | ✅ |
| `design.md` | ➖ No existe (flujo: propose → spec → tasks → apply) |
| `tasks.md` | ✅ (8/8 tareas completadas: 1.1–1.4, 2.1–2.3, 3.1) |
| `verify-report.md` | ✅ |
| `apply-progress.md` | ✅ |
| `archive-report.md` | ✅ (este archivo) |

## Verificación

- **Build**: `dotnet build -f net10.0-android` → **SUCCESS** (0 errores; 2 advertencias `NU1903` pre-existentes por `SQLitePCLRaw.lib.e_sqlite3.android` 2.1.11).
- **Tests**: no disponibles (sin proyecto de tests; `strict_tdd=false`).
- **Cumplimiento**: 16/16 escenarios con evidencia estática (código + build); 0 fallidos, 0 faltantes.
- **Verificación visual en dispositivo: NO posible** — el dispositivo físico (Motorola moto g56) quedó bloqueado/pantalla en negro durante el intento. **10 escenarios NO EJECUTADOS** en runtime, pendientes de smoke test por el usuario.

## Correcciones aplicadas durante esta fase (resueltas)

1. **Fechas del query-string ahora usan `CultureInfo.InvariantCulture`** — `ReportsService.cs:36` ahora formatea con `System.Globalization.CultureInfo.InvariantCulture` (`desde.ToString("yyyy-MM-dd", ...)` / `hasta.ToString("yyyy-MM-dd", ...)`). Resuelve el WARNING 1 del verify-report (formato no explícitamente invariante).

## Advertencias/sugerencias aceptadas (dejadas como están — no corregidas)

1. **Overlay de error sin fondo** — `HomePage.xaml:95-112` (el `VerticalStackLayout` de error/retry no tiene `BackgroundColor`; cosmético).
2. **`HomeViewModel.Username` efectivamente sin uso** — se asigna pero nada lo enlaza (el header del Flyout lee `ITokenStore` vía `AppShell.UpdateUsernameAsync`).
3. **Duplicación de helper HTTP** entre `ReportsService.SendAndReadAsync` y `CampaignService.SendAndReadAsync` — podría extraerse a una base/helper compartido. No bloqueante.
4. **Las tarjetas muestran "0" mientras cargan** — `Formatear` mapea `null` → `"0"`; aceptable según IH-R5, pero podría existir un estado de carga distinto.
5. **El SQL de indicadores del backend no filtra por compañía** (conteos globales) — aceptado tal cual (documentado en spec y proposal como out-of-scope).

## Riesgos / observaciones

- **Verificación visual pendiente en dispositivo**: 10 escenarios `NOT EXECUTED`; se requiere smoke test en hardware por el usuario antes de considerar la validación runtime completa.
- **Árbol de trabajo con cambios no relacionados**: `git status` muestra, junto a los archivos de `indicadores-home`, modificaciones del lote UI previo (subrayado del login, rediseño del detalle de campaña: `CampaignDetailPage.xaml`, `BrandStyles.xaml`, y specs `autenticacion-token`/`campanas-whatsapp`). **Este archivo solo toca archivos `openspec/`**; los archivos de código de ese lote previo NO se incluyen ni se alteran aquí.
- **NU1903 (CVE-2025-6965)** en `SQLitePCLRaw.lib.e_sqlite3.android` 2.1.11 — pre-existente, sin parche upstream. Aceptado.

## Conclusión

El ciclo SDD del change `indicadores-home` queda **completo**: planificado, implementado, verificado (build verde + revisión estática) y archivado. Las specs base quedan actualizadas como fuente de verdad. Sin issues CRÍTICOS. Queda un smoke test visual en dispositivo como único pendiente no bloqueante.
