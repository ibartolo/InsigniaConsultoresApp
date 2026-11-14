# Verification Report

**Change**: `login-autenticacion-token`
**Version**: N/A (first iteration — no prior specs; `openspec/specs/` is empty)
**Mode**: Standard (strict_tdd = false, no test project, no device/emulator)

> Verification method: static code review + real build execution. Runtime scenarios are **NOT EXECUTED** (no connected device / `adb` unavailable) and are reported as coverage gaps, not failures.

---

## Completeness

| Metric | Value |
|--------|-------|
| Tasks total | 36 |
| Tasks complete | 36 |
| Tasks incomplete | 0 |

All tasks `[x]`. No incomplete tasks. Completeness = 100%.

---

## Build & Tests Execution

**Build**: ✅ Passed — `dotnet build InsigniaConsultoresApp\InsigniaConsultoresApp.csproj -f net10.0-android`

```
Compilación correcta.
    3 Advertencia(s)
    0 Errores
Tiempo transcurrido 00:00:14.93
```

Warnings (all **NU1903**, same advisory ×3 at restore/build steps):

```
warning NU1903: El paquete "SQLitePCLRaw.lib.e_sqlite3.android" 2.1.11 tiene una
vulnerabilidad de gravedad alta conocida, https://github.com/advisories/GHSA-2m69-gcr7-jv3q
```

This matches the apply-progress record (B1). It is a **high-severity** advisory (CVE-2025-6965, SQLite < 3.50.2) on a native transitive dependency with **"Patched versions: None"** upstream. It is documented, not suppressed, and not fixable by version bump at this time.

**Tests**: ➖ Not available (no test project exists — `strict_tdd: false`, `test_runner.command: ""`).

**Coverage**: ➖ Not available (`coverage_threshold: 0`, no coverage tooling).

---

## Spec Compliance Matrix

> Compliance status legend:
> - ✅ COMPLIANT (static) — code present and structurally correct; no runtime test possible.
> - ⚠️ NOT EXECUTED (device) — code present/correct, but behavior requires a device/emulator.
> - ❌ NOT IMPLEMENTED — deferred (trigger does not exist in this change's scope).

| Req | Scenario | Evidence (file:line) | Status |
|-----|----------|----------------------|--------|
| BR-R1 | Ícono de app usa el logo | `InsigniaConsultoresApp.csproj:36` (`MauiIcon logo.png`), `Resources/AppIcon/logo.png` present, `AndroidManifest.xml:3` (`@mipmap/logo`) | ⚠️ NOT EXECUTED (device) |
| BR-R1 | Splash usa el mismo logo | `InsigniaConsultoresApp.csproj:39` (`MauiSplashScreen logo.png #0F4C81`), `Resources/Splash/logo.png` present | ⚠️ NOT EXECUTED (device) |
| BR-R1 | Recorte del adaptive icon (riesgo) | Documented risk (spec scenario 3 / design open question) | ⚠️ NOT EXECUTED (device) |
| BR-R2 | Colores disponibles en toda la app | `Resources/Styles/Colors.xaml:47-59` — all 11 brand tokens defined | ✅ COMPLIANT (static) |
| BR-R3 | Componentes usan los valores de marca | `Resources/Styles/BrandStyles.xaml` (gradient CTA, radius 999/10, shadow) | ✅ COMPLIANT (static) |
| AT-R1 | Elementos de la pantalla de login | `Views/LoginPage.xaml` (logo 56×56 radio 12, título, Entry usuario, Entry `IsPassword=True`, botón "Iniciar sesión", spinner overlay; no "Registrar"/"Recuperar") | ✅ COMPLIANT (static) |
| AT-R2 | Campos vacíos | `Validations/LoginValidator.cs:19-27` + `LoginViewModel.cs:81-86` (no request; `ErrorMessage` inline) | ✅ COMPLIANT (static) |
| AT-R3 | Respuesta exitosa | `AuthService.cs:33-54` (parses `access_token`/`token_type`/`expires_in`, `PropertyNameCaseInsensitive=true`) | ✅ COMPLIANT (static) |
| AT-R3 | Respuesta malformada | `AuthService.cs:36-41` (empty body / null DTO / null access_token → `MalformedResponse`) | ✅ COMPLIANT (static) |
| AT-R4 | Carga en curso | `LoginViewModel.cs:77` (`if (IsBusy) return`), `:89,120` (`IsBusy=true`/`finally IsBusy=false`), `LoginPage.xaml:66-72` (`IsEnabled={IsNotBusy}`, spinner overlay) | ✅ COMPLIANT (static) |
| AT-R5 | Token persistido tras login exitoso | `TokenStore.cs:54-71` (SecureStorage + SQLite), `LoginViewModel.cs:108` (`Password=""`) | ⚠️ NOT EXECUTED (device — real keystore encryption) |
| AT-R6 | Cálculo con margen | `TokenExpiry.cs:10-17` → 3600 − 1200 = 2400s | ✅ COMPLIANT (static) |
| AT-R6 | Fallback sin margen | `TokenExpiry.cs:12-15` (`null→0`, `≤margen→lifetime`, never negative) | ✅ COMPLIANT (static) |
| AT-R7 | Token válido al arrancar | `AppShell.xaml.cs:32-33` (`HasValidSessionAsync()` → `GoToAsync("//home")`) | ⚠️ NOT EXECUTED (device) |
| AT-R7 | Token expirado o ausente | `AppShell.xaml.cs:33-34` (stays on `//login`), `TokenStore.cs:24-28` | ✅ COMPLIANT (static) |
| AT-R8 | 401 en solicitud protegida | **No protected request exists** (business functionality out of scope) | ❌ NOT IMPLEMENTED (deferred) |
| AT-R9 | Credenciales inválidas | `AuthService.cs:57-61` → "Usuario o contraseña incorrectos" | ✅ COMPLIANT (static) |
| AT-R9 | Error de red | `AuthService.cs:69-77` (`TaskCanceledException`/`HttpRequestException`) → "No se pudo conectar con el servidor" | ✅ COMPLIANT (static) |
| AT-R9 | Error del servidor | `AuthService.cs:63-64` (≥500) → "Ocurrió un error en el servidor, intenta de nuevo" | ✅ COMPLIANT (static) |
| AT-R10 | Logout | `AppShell.xaml.cs:61-65` (`ClearAsync()` → `//login`), `TokenStore.cs:73-86` | ⚠️ NOT EXECUTED (device) |
| IN-R1 | Home tras login exitoso | `AppShell.xaml:20-25` (FlyoutItem `home`), `Views/HomePage.xaml` | ✅ COMPLIANT (static) |
| IN-R2 | Flyout muestra logo, usuario y acciones | `AppShell.xaml:28-50` (FlyoutHeader logo + `UsernameLabel` + "Sesión activa" + MenuItem "Cerrar sesión") | ✅ COMPLIANT (static) |
| IN-R3 | Home sin contenido de negocio | `Views/HomePage.xaml:8-15` (placeholder "Sin contenido por ahora") | ✅ COMPLIANT (static) |
| IN-R4 | Back no regresa al login | `AppShell.xaml:15` (`FlyoutItemIsVisible=False`), absolute `//home`/`//login` only (`AppShell.xaml.cs:33,64`, `LoginViewModel.cs:111`, `HomeViewModel.cs:48`), no `PushAsync` | ⚠️ NOT EXECUTED (device — actual back-press) |

**Compliance summary**: 16/17 requirements implemented · 23/24 scenarios covered statically · 1 deferred (AT-R8) · 0 runtime scenarios executed (no device).

---

## Correctness (Static — Structural Evidence)

| Requirement | Status | Notes |
|------------|--------|-------|
| BR-R1 Icono/splash | ✅ Implemented | `logo.png` byte-identical (SHA256 `B4176C97…`) in AppIcon/Splash/Images; manifest mipmap names consistent; template svgs + `dotnet_bot.png` removed |
| BR-R2 Paleta | ✅ Implemented | All 11 tokens exact-match spec values; `Primary`/`PrimaryDark` repointed |
| BR-R3 Sistema visual | ✅ Implemented | Radius 10/999, CTA gradient `#E8952B→#C97A18`, hero gradient 120° approx, shadow |
| AT-R1 Login UI | ✅ Implemented | All elements present; no Registrar/Recuperar (grep = 0 hits) |
| AT-R2 Validación | ✅ Implemented | Both fields validated, inline ES message, no request |
| AT-R3 Solicitud token | ✅ Implemented | `FormUrlEncodedContent`, `POST /Token`, case-insensitive deserialization |
| AT-R4 Anti doble envío | ✅ Implemented | `IsBusy` guard + `IsEnabled`/`IsNotBusy` + spinner overlay |
| AT-R5 Persistencia híbrida | ✅ Implemented | SecureStorage (`access_token`) + SQLite (`Session` table); password never persisted, cleared after login |
| AT-R6 Expiración | ✅ Implemented | Margin 1200s, never negative, fallback correct |
| AT-R7 Arranque | ✅ Implemented | `AppShell.Loaded` routes to `//home` or stays login |
| AT-R8 401 en uso | ❌ Deferred | No protected requests in scope (see WARNING) |
| AT-R9 Errores | ✅ Implemented | Exact ES strings, all 4 conditions mapped |
| AT-R10 Logout | ✅ Implemented | Clears SecureStorage + SQLite, absolute `//login` |
| IN-R1 Home Flyout | ✅ Implemented | `FlyoutBehavior="Flyout"` on root, blank page |
| IN-R2 Contenido Flyout | ✅ Implemented | Logo + username + "Inicio" + "Cerrar sesión" |
| IN-R3 Placeholder | ✅ Implemented | Blank placeholder |
| IN-R4 Sin regreso | ✅ Implemented | Absolute navigation only; login hidden from flyout |

---

## Coherence (Design)

| Decision | Followed? | Notes |
|----------|-----------|-------|
| D1 DI/composición | ✅ Yes | All lifetimes match (HttpClient singleton, services singleton, VMs/pages/Shell transient) |
| D2 Layering | ✅ Yes | All classes/interfaces present as specified |
| D3 Token request | ✅ Yes | `FormUrlEncodedContent` + `PropertyNameCaseInsensitive` |
| D4 Error model | ✅ Yes | `LoginResult` + `AuthErrorKind` + `AuthMessages` (exact strings) |
| D5 Validación | ✅ Yes | Static `LoginValidator` + `ValidationResult` |
| D6 Persistencia híbrida | ✅ Yes | SecureStorage + SQLite, defensive keystore fallback with warning |
| D7 Expiración | ✅ Yes | `TokenExpiry.CalculateUtc` matches design formula exactly |
| D8 Ruteo arranque | ✅ Yes | `AppShell.Loaded` decision; login is initial ShellContent; documented login-flash accepted |
| D9 Shell/Flyout | ✅ Yes | Login hidden, home FlyoutItem, FlyoutHeader, MenuItem logout |
| D10 UI Login | ✅ Yes | Spinner via `Grid` overlay (Button can't host children) |
| D11 Estilos | ✅ Yes | `Colors.xaml` edited + `BrandStyles.xaml` merged in `App.xaml` |
| D12 Ícono/splash | ✅ Yes | `MauiIcon`/`MauiSplashScreen` → `logo.png`, no template assets |
| D13 Paquetes | ✅ Yes | `sqlite-net-pcl` 1.9.172 + explicit `SQLitePCLRaw.bundle_green` 2.1.11 (security patch) |
| D14 Manifiesto archivos | ✅ Yes | All 21 new + 3 assets present; all 6 deletions confirmed |
| D16 Android-only | ✅ Yes | `TargetFrameworks=net10.0-android`, `SupportedOSPlatformVersion=21.0` |

Deviations from design are all documented in `apply-progress.md` (D1–D17) and are either improvements (bundle_green patch, `ContentTemplate` DI resolution) or authoritatively re-scoped by the orchestrator. None are regressions.

---

## High-Risk Item Checks (requested)

| Check | Result | Evidence |
|-------|--------|----------|
| Expiry math | ✅ Correct | `TokenExpiry.cs:10-17` — 3600→2400; null/≤1200→no margin; never negative |
| SecureStorage fallback | ✅ Correct | `TokenStore.cs:54-71` — encrypted path default; fallback only on keystore exception, logs warning, `IsEncrypted=false`, plaintext stored in SQLite only then |
| Double-submit guard | ✅ Correct | `LoginViewModel.cs:77` + `IsEnabled={IsNotBusy}` + `finally IsBusy=false` |
| Password cleared | ✅ Correct | `LoginViewModel.cs:108`; never persisted (`SessionModel`/`SessionRow` have no password field) |
| No Registrar/Recuperar | ✅ Correct | Grep across project = 0 hits |
| Back not to login | ✅ Correct | Absolute `//home`/`//login` only; no `PushAsync`; login `FlyoutItemIsVisible=False` |
| Error mapping | ✅ Correct | Exact spec strings in `AuthMessages.Get`; 400/401/timeout/5xx/malformed all mapped |
| Startup routing | ✅ Correct | `AppShell.OnLoaded`; documented login flash accepted |
| Icon/splash | ✅ Correct | csproj entries, files present (identical hash), manifest consistent, no template assets |
| DI | ✅ Correct | All deps resolvable; only service-locator is `IPlatformApplication.Current` in `App.CreateWindow` |
| Leaks/dead code | ⚠️ Minor | See SUGGESTIONS (gitignored `.csproj.user` MainPage entry, dead platform folders, unused `HomeViewModel` members, unused using) |

---

## Issues Found

### CRITICAL (must fix before archive)
None.

### WARNING (should fix / track)
- **W1 — NU1903 high-severity advisory** (`SQLitePCLRaw.lib.e_sqlite3.android` 2.1.11, CVE-2025-6965). `InsigniaConsultoresApp.csproj:58`. Transitive native SQLite bundle with no upstream patch. Accepted + documented (B1), not suppressed. Action: monitor upstream; bump `SQLitePCLRaw.bundle_green` when a build with SQLite ≥ 3.50.2 ships.
- **W2 — AT-R8 "401 en solicitud protegida" not implemented** (`autenticacion-token/spec.md`). No protected request exists in this change (business functionality is out of scope), so the trigger is absent. Deferred to the change that adds the first protected endpoint. Not a build/runtime break.

### SUGGESTION (nice to have)
- **S1 — Android native theme colors still template purple**: `Platforms/Android/Resources/values/colors.xml:3-5` (`#512BD4` / `#2B0B98`). May surface as a purple status-bar/window flash on cold start. Align `colorPrimary`/`colorPrimaryDark`/`colorAccent` to brand (`#0F4C81`/`#0A3A63`).
- **S2 — Dead platform folders**: `Platforms/iOS`, `Platforms/MacCatalyst`, `Platforms/Windows` remain fully populated but are no longer compiled (android-only). Safe to delete for cleanliness.
- **S3 — `InsigniaConsultoresApp.csproj.user:16-18` still lists `<None Update="MainPage.xaml">`**. Gitignored (`*.user`), non-build, but a leftover reference to a deleted file.
- **S4 — `HomeViewModel.Username`/`LoadAsync()`/`LogoutCommand` are effectively unused** (`ViewModels/HomeViewModel.cs`). Flyout header is populated in `AppShell` code-behind (D15) and logout is via `AppShell.OnLogoutClicked`. `HomePage.OnAppearing` calls `LoadAsync()` but `Username` isn't bound anywhere; `LogoutCommand` isn't bound to any control.
- **S5 — `launchSettings.json:3` still has a "Windows Machine" profile** (leftover template).
- **S6 — Unused using**: `ViewModels/LoginViewModel.cs:4` (`InsigniaConsultoresApp.Models.Responses`) — `LoginResult` is only type-inferred via `var`.

---

## Verdict

**PASS WITH WARNINGS**

The implementation is complete (36/36 tasks), structurally matches all three specs (16/17 requirements implemented; 23/24 scenarios covered statically), follows the design, and builds clean (0 errors). The only warnings are a known-upstream SQLite advisory (W1) and one deferred requirement with no in-scope trigger (W2); runtime scenarios could not be executed due to the absence of a device/emulator. No spec violations or build breaks were found.

---

## Coverage Gaps (runtime, NOT failures)

All scenarios requiring a device/emulator are **NOT EXECUTED** (no `adb`, no connected device):
- BR-R1 (icon, splash, adaptive-crop), AT-R5 (real keystore encryption), AT-R7 (cold-start routing), AT-R10 (logout), IN-R4 (back-press) — plus visual/network behaviors in AT-R1/AT-R3/AT-R4/AT-R9.

The 9-step manual test in `tasks.md` §10.2 and the 8-step plan in `apply-progress.md` remain pending execution on hardware.
