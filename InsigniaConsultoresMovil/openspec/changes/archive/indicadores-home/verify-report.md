# Verification Report: indicadores-home

**Change**: `indicadores-home`
**Version**: N/A (delta specs, no explicit version field)
**Mode**: Standard (`strict_tdd: false`, no test runner)
**Date**: 2026-11-17
**Verifier**: sdd-verify (deepseek-v4-pro)

---

## Executive Summary

The Home KPI section (`indicadores-home`) is implemented and compiles cleanly against `net10.0-android` (0 errors). Static review confirms every requirement IH-R1…IH-R6 and the MODIFIED `inicio-y-navegacion` requirement are structurally implemented, and the existing flyout/logout/campaigns/login behavior is preserved (constructor dependencies resolve in `MauiProgram`, `Username`/`LogoutCommand` remain functional). Runtime/device-dependent scenarios could not be visually confirmed because the physical device (Motorola moto g56) was unavailable (locked/black screen) — these are marked `NOT EXECUTED`, not failures. No CRITICAL issues found.

---

## Completeness

| Metric | Value |
|--------|-------|
| Tasks total | 8 |
| Tasks complete | 8 |
| Tasks incomplete | 0 |

All tasks `[x]` in `tasks.md` (1.1–1.4 infra, 2.1–2.3 presentation, 3.1 build). ✅

---

## Build & Tests Execution

**Build**: ✅ Passed

```
dotnet build InsigniaConsultoresApp\InsigniaConsultoresApp.csproj -f net10.0-android -v q --nologo
→ Compilación correcta.
  2 Advertencia(s)
  0 Errores
```

The 2 warnings are `NU1903` (SQLitePCLRaw.lib.e_sqlite3.android 2.1.11 has a known high-severity vulnerability) — pre-existing, unrelated to this change, already documented as expected in `apply-progress.md`.

**Tests**: ➖ None — no test project exists (`config.yaml`: `test_runner.command = ""`, `framework: none`, `strict_tdd: false`). Standard mode → no TDD checks apply.

**Coverage**: ➖ Not available (no test project; `coverage_threshold: 0`).

---

## Spec Compliance Matrix

Because there is no test runner, there are no runtime test results. Behavioral validation is **static** (code inspection + successful build). Device-dependent scenarios are additionally marked `NOT EXECUTED (device unavailable)`.

| Requirement | Scenario | Evidence (file:line) | Result |
|-------------|----------|----------------------|--------|
| IH-R1 Carga al entrar | Carga inicial | `HomePage.xaml.cs:22-33` (`OnAppearing` → `LoadAsync` once), `HomeViewModel.cs:145-181`, `HomePage.xaml:87-92` (spinner) | ✅ STATIC (visual: NOT EXECUTED) |
| IH-R1 Carga al entrar | Flyout no bloqueado | async non-blocking, no modal overlay; `AppShell.xaml:8` (`FlyoutBehavior`) | ✅ STATIC (visual: NOT EXECUTED) |
| IH-R2 Las 5 tarjetas | Tarjetas etiqueta+valor | `HomePage.xaml:43-80` (5 `Border` cards), `HomeViewModel.cs:130-138` (es-MX bindings) | ✅ STATIC (visual: NOT EXECUTED) |
| IH-R2 Las 5 tarjetas | Sin detalle ni tap-through | no `TapGestureRecognizer`/`GestureRecognizer` anywhere in `HomePage.xaml` | ✅ STATIC |
| IH-R3 Período | Etiqueta de periodo | `HomeViewModel.cs:217-222` (`BuildPeriodoTexto` uses API `FechaInicio`/`FechaFin`) | ✅ STATIC (visual: NOT EXECUTED) |
| IH-R4 Selector de fechas | Valores por defecto | `HomeViewModel.cs:40-43` (first day of month / today) | ✅ STATIC |
| IH-R4 Selector de fechas | Cambio recarga | `HomeViewModel.cs:57-87` (setters → `OnRangoCambiadoAsync`) | ✅ STATIC (visual: NOT EXECUTED) |
| IH-R4 Selector de fechas | Hasta futura | `HomeViewModel.cs:160-164` + `HomePage.xaml:38` (`MaximumDate="{Binding Hoy}"`) | ✅ STATIC |
| IH-R4 Selector de fechas | Desde posterior a Hasta | `HomeViewModel.cs:154-158` (no API call, Spanish message) | ✅ STATIC |
| IH-R5 Estados | Carga y error | `HomePage.xaml:87-112` (spinner + error/retry), `CampaignViewModelBase.cs:67-88` | ✅ STATIC (visual: NOT EXECUTED) |
| IH-R5 Estados | Valor cero | `HomeViewModel.cs:227-228` (`Formatear` → `(valor ?? 0).ToString("N0", EsMx)`) | ✅ STATIC |
| IH-R6 Refresco y sesión | Pull-to-refresh | `HomePage.xaml:11-13` (`RefreshView` + `RefreshCommand`), `HomeViewModel.cs:183-194` | ✅ STATIC (visual: NOT EXECUTED) |
| IH-R6 Refresco y sesión | Sesión expirada (401) | `HomeViewModel.cs:167` → `CampaignViewModelBase.cs:94-109` (guard `_redirectingToLogin`) | ✅ STATIC |
| IH-R6 Refresco y sesión | Error red/servidor | `ReportsService.cs:86-98` + `CampaignResult.cs:67-77` (Spanish msgs); `CampaignViewModelBase.cs:80-83` (no crash) | ✅ STATIC |
| inicio-y-navegacion (MODIFIED) | Home muestra sección indicadores | `HomePage.xaml` full KPI layout; old placeholder removed (`git show HEAD` confirms placeholder was "Inicio / Sin contenido por ahora") | ✅ STATIC (visual: NOT EXECUTED) |

**Compliance summary**: 16/16 scenarios structurally implemented (static). 0 failing, 0 missing. 10 scenarios pending on-device visual confirmation (NOT EXECUTED due to device unavailability).

---

## Correctness (Static — Structural Evidence)

| Requirement | Status | Notes |
|------------|--------|-------|
| IH-R1 Carga al entrar | ✅ Implemented | `LoadAsync` on `OnAppearing`, guarded by `_loaded` (once), spinner bound to `IsBusy`, non-blocking flyout |
| IH-R2 Las 5 tarjetas | ✅ Implemented | Exactly 5 cards, correct Spanish labels, es-MX values, no tap-through |
| IH-R3 Período | ✅ Implemented | Uses API-returned dates, falls back to selected range when API omits them (documented deviation) |
| IH-R4 Selector de fechas | ✅ Implemented | Defaults, reload-on-change, both validations (Hasta future, Desde>Hasta) short-circuit before API call, Spanish messages |
| IH-R5 Estados | ✅ Implemented | Spinner, error+retry (Spanish), 0 renders as "0" |
| IH-R6 Refresco y sesión | ✅ Implemented | Pull-to-refresh, 401 → clear session + `//login` exactly once, network/5xx Spanish messages, no crash |
| inicio-y-navegacion (MODIFIED) | ✅ Implemented | Placeholder replaced by KPI section; no leftover placeholder text |

---

## Coherence (Design / Approach)

No `design.md` exists for this change (workflow went propose → spec → tasks → apply). Coherence was checked against the `proposal.md` Approach and the `apply-progress.md` deviations.

| Decision (from proposal/apply) | Followed? | Notes |
|----------|-----------|-------|
| DTO `IndicadoresPeriodo` with 5 ints + `FechaInicio`/`FechaFin`, `[JsonPropertyName]`, tolerant deserialization | ✅ Yes | `IndicadoresPeriodo.cs:14-26`; missing fields → `0`/`DateTime.MinValue` |
| `IReportsService`/`ReportsService` reusing `CampaignService` pattern (Bearer per request, `CampaignResult<T>`, no DelegatingHandler) | ✅ Yes | `ReportsService.cs` mirrors `CampaignService.SendAndReadAsync` exactly |
| `HomeViewModel` inherits `CampaignViewModelBase` for 401 + load/error state | ✅ Yes | `HomeViewModel.cs:18,33-34`; deviation noted as intended |
| `MaximumDate` via `Hoy` property (not `x:Static`) to avoid XAML source-gen ambiguity | ✅ Yes | `HomePage.xaml:38` + `HomeViewModel.cs:90` |
| DI registration `AddSingleton<IReportsService, ReportsService>()` | ✅ Yes | `MauiProgram.cs:44` |

---

## Issues Found

### CRITICAL
None.

### WARNING
1. **Query-string date formatting not explicitly culture-invariant** — `ReportsService.cs:35`
   `$"api/Reports/Indicadores?fechaInicio={desde:yyyy-MM-dd}&fechaFin={hasta:yyyy-MM-dd}"` uses `IFormattable.ToString(format, CultureInfo.CurrentCulture)`. In practice this is **safe**: `-` is a literal in .NET custom date formats (not the culture-sensitive `/` separator), and `yyyy/MM/dd` are Gregorian numeric components, so output is `YYYY-MM-DD` under `es-MX` (and any Gregorian culture). It is only theoretically vulnerable to a culture whose *default calendar* is non-Gregorian. Recommended (non-blocking): `desde.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)` for explicit hardening.
2. **Unrelated uncommitted changes in the working tree** — `git status` shows, alongside the indicadores-home files, modifications that belong to a *prior* change (campanas-whatsapp detail read-only form): `Views/CampaignDetailPage.xaml`, `BrandStyles.xaml` (`BrandReadOnlyField`/`BrandReadOnlyText`), `openspec/specs/autenticacion-token/spec.md`, `openspec/specs/campanas-whatsapp/spec.md`. These are NOT regressions from indicadores-home, but they must be excluded when this change is committed/archived. The archive step must stage only the indicadores-home files.

### SUGGESTION
1. **Error overlay has no background** — `HomePage.xaml:95-112`: the centered error/retry `VerticalStackLayout` has no `BackgroundColor`; KPI content behind it may bleed through visually. Cosmetic only.
2. **`HomeViewModel.Username` is effectively dead code** — `HomeViewModel.cs:51-55` sets it, but nothing binds it (the flyout header reads `ITokenStore` directly via `AppShell.UpdateUsernameAsync`, `AppShell.xaml.cs:77-88`). Harmless, but could be removed or documented to avoid confusion.
3. **`ReportsService.SendAndReadAsync` duplicates `CampaignService`'s** — `ReportsService.cs:60-99` vs `CampaignService.cs:173-206`. A shared private helper/base could be extracted. Non-blocking.
4. **Cards show "0" before first load completes** — `Formatear` maps `null` → `"0"` (`HomeViewModel.cs:227-228`), so during initial load the cards briefly render "0" instead of empty/placeholder. Acceptable per IH-R5, but a distinct loading state could be considered.

---

## Regression Check (flyout / logout / campaigns / login)

| Area | Result | Evidence |
|------|--------|----------|
| Constructor deps resolve | ✅ | `MauiProgram.cs:41` (`ITokenStore`), `:44` (`IReportsService`), `:48` (`HomeViewModel` transient); `HomeViewModel` ctor `(IReportsService, ITokenStore)` → `base(tokenStore)` (`HomeViewModel.cs:33-34`) |
| `Username` still works | ✅ | Property retained and set in `LoadAsync` (`HomeViewModel.cs:51-55,148-149`) |
| `LogoutCommand` still works | ✅ | Retained (`HomeViewModel.cs:47,127,205-211`); absolute nav `//login`; also `AppShell.OnLogoutClicked` unchanged |
| Flyout / hamburger | ✅ | `AppShell.xaml` unchanged; load is async, no modal blocking |
| Login route | ✅ | `AppShell.xaml:13` `Route="login"` matches `//login` used in `HomeViewModel`/`CampaignViewModelBase`/`AppShell` |
| Campaigns module | ✅ | No behavioral change; the only campaign-detail diff in the tree is from a prior change, not this one |

---

## Verdict

**PASS WITH WARNINGS**

Implementation is complete and builds cleanly. All requirements are structurally implemented and the 401/validation/formatting behaviors are correct by inspection. The only device-dependent evidence (visual KPIs, pull-to-refresh, spinner, error UI) could not be confirmed because the physical device was unavailable — pending on-device smoke test. One non-blocking hardening recommendation (explicit `InvariantCulture` on the query string) and a working-tree hygiene note for archive.

---

## Risks

- On-device visual confirmation outstanding (device unavailable) — 10 scenarios remain `NOT EXECUTED`.
- Working tree contains interleaved uncommitted changes from a prior change; archive must stage only indicadores-home files.
- Query-string formatting relies on literal `-` (safe on Gregorian/es-MX) rather than explicit `InvariantCulture`.
- `SQLitePCLRaw.lib.e_sqlite3.android` 2.1.11 has a known high-severity advisory (pre-existing, unrelated).
