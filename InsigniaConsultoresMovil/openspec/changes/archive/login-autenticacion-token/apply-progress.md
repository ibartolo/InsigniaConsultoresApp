# Apply Progress: login-autenticacion-token

> Lote 1 (2026-11-13) — Etapas 1 y 2. Lote 2 (2026-11-13) — Etapas 3 y 4 + tarea 1.4.
> Modo: **Standard** (strict_tdd = false, sin proyecto de tests).

## Resultado del build

`dotnet build InsigniaConsultoresApp\InsigniaConsultoresApp.csproj -f net10.0-android` → **SUCCESS** (0 warnings, 0 errors).

## Tareas completadas

### Etapa 1 — Proyecto y recursos
- [x] **1.1** `.csproj` → Android-only: único `TargetFrameworks` `net10.0-android`, sin líneas ios/maccatalyst/windows, sin `WindowsPackageType`, `SupportedOSPlatformVersion` simplificado a `21.0` (sin condiciones por plataforma). También se quitó el comentario muerto de MacCatalyst.
- [x] **1.2** Copiado `logo.png` (42 258 bytes) desde `C:\Git\InsigniaConsultoresAxaPaginaWeb\imagenes\marca\logo.png` a `Resources\AppIcon\logo.png`, `Resources\Splash\logo.png` y `Resources\Images\brand_logo.png` (ver desviación D2).
- [x] **1.3** `MauiIcon Include="Resources\AppIcon\logo.png"` (sin `ForegroundFile`, sin `Color`) y `MauiSplashScreen Include="Resources\Splash\logo.png" Color="#0F4C81" BaseSize="128,128"`.
- [ ] **1.4** `sqlite-net-pcl` — **DIFERIDO** (ver D1).
- [x] **1.5** Quitada la línea `<MauiImage Update="Resources\Images\dotnet_bot.png" ... />`.
- [x] **1.6** Eliminados `appicon.svg`, `appiconfg.svg`, `splash.svg`, `dotnet_bot.png`. (Eliminación de `MainPage.xaml(.cs)` movida a Etapa 9 — ver D4.)

### Etapa 2 — Modelos y validación
- [x] **2.1** `Models/Requests/TokenRequest.cs` — `Username`, `Password`, `GrantType`, `ToForm()` (FormUrlEncodedContent).
- [x] **2.2** `Models/Responses/TokenResponse.cs` — DTO con `JsonPropertyName`.
- [x] **2.3** `Models/Responses/LoginResult.cs` — `AuthErrorKind`, `LoginResult` (`Ok`/`Fail`), `AuthMessages.Get`.
- [x] **2.4** `Models/SessionModel.cs` — propiedades de sesión.
- [x] **2.5** `Validations/LoginValidator.cs` — `ValidationResult` + `LoginValidator.Validate`.

## Archivos

### Creados
| Archivo | Nota |
|---------|------|
| `Resources/AppIcon/logo.png` | Ícono (copiado) |
| `Resources/Splash/logo.png` | Splash (copiado) |
| `Resources/Images/brand_logo.png` | Logo in-app (copiado, renombrado — D2) |
| `Models/Requests/TokenRequest.cs` | Datos de login + `ToForm()` |
| `Models/Responses/TokenResponse.cs` | DTO token |
| `Models/Responses/LoginResult.cs` | `AuthErrorKind` + `LoginResult` + `AuthMessages` |
| `Models/SessionModel.cs` | Sesión persistida |
| `Validations/LoginValidator.cs` | `ValidationResult` + `LoginValidator` |

### Modificados
| Archivo | Cambio |
|---------|--------|
| `InsigniaConsultoresApp/InsigniaConsultoresApp.csproj` | Android-only; `MauiIcon`/`MauiSplashScreen` → `logo.png`; sin `MauiImage Update` de `dotnet_bot.png` |
| `Platforms/Android/AndroidManifest.xml` | `@mipmap/appicon`/`appicon_round` → `@mipmap/logo`/`logo_round` (D3) |
| `openspec/changes/login-autenticacion-token/tasks.md` | Marcas `[x]`, reorden y notas |

### Eliminados
- `Resources/AppIcon/appicon.svg`
- `Resources/AppIcon/appiconfg.svg`
- `Resources/Splash/splash.svg`
- `Resources/Images/dotnet_bot.png`

## Desviaciones del diseño / decisiones

- **D1 — `sqlite-net-pcl` (tarea 1.4) diferido**: el lote prohíbe paquetes NuGet (`no extra NuGet packages yet`, `no dotnet add package`). Se agrega junto con la Etapa 4 (persistencia), donde se usa por primera vez.
- **D2 — Logo in-app renombrado a `brand_logo.png`**: MAUI Resizetizer exige nombres de salida únicos; `logo.png` repetido en `AppIcon` + `Splash` + `Images` colisiona (`error: One or more duplicate file names ... logo`). Se mantuvo `logo.png` para ícono y splash (según instrucción) y se renombró solo el asset in-app. Las vistas de etapas posteriores (FlyoutHeader, LoginPage) deben referenciar `brand_logo.png`, no `logo.png`.
- **D3 — `AndroidManifest.xml` actualizado**: al cambiar el ícono a `logo.png`, el mipmap generado pasa de `appicon` a `logo`; el manifest aún apuntaba a `@mipmap/appicon` y el build fallaba (`APT2260`). Se corrigió a `@mipmap/logo`/`@mipmap/logo_round`. Es consecuencia directa del cambio de ícono de este lote.
- **D4 — `MainPage.xaml(.cs)` no eliminados aquí** (corrección de orden indicada por el orquestador): `AppShell.xaml` aún los referencia (`DataTemplate local:MainPage`); eliminarlos rompería el build. Se movieron a Etapa 9 (nueva tarea 9.1), junto con la verificación de referencias (9.2) y el check de UI (9.3).

---

## Lote 2 — Etapas 3 y 4 (API/expiración + persistencia) + tarea 1.4

> Fecha: 2026-11-13 · Modo: **Standard**.

### Resultado del build

`dotnet build InsigniaConsultoresApp\InsigniaConsultoresApp.csproj -f net10.0-android` → **SUCCESS** (0 errores, 3 advertencias).

Advertencias (no bloqueantes, transitivas del bundle nativo sqlite):
- **NU1903**: `SQLitePCLRaw.lib.e_sqlite3.android` 2.1.2 tiene una vulnerabilidad alta conocida (GHSA-2m69-gcr7-jv3q). Es dependencia transitiva de `sqlite-net-pcl`; el diseño (Decisión 13) difiere actualizar el bundle salvo error en `Batteries_V2.Init()`.
- **XA0141**: `libe_sqlite3.so` sin soporte de páginas de 16 KB (Android 16). Aviso informativo del paquete nativo.

### Tareas completadas

- [x] **1.4** `sqlite-net-pcl` 1.9.172 agregado (restaura limpio sobre `net10.0-android`, netstandard2.0).

#### Etapa 3 — API y expiración
- [x] **3.1** `Services/Api/AuthConfig.cs` — `BaseUrl`, `TokenEndpoint = BaseUrl + "/Token"`, `TokenExpiryMarginSeconds = 20*60` (=1200), `RequestTimeoutSeconds = 30`.
- [x] **3.2** `Services/Api/TokenExpiry.cs` — `CalculateUtc(nowUtc, expiresInSeconds)`: margen, nunca negativo, fallback sin margen.
- [x] **3.3** `Services/Api/IAuthService.cs` — `LoginAsync(TokenRequest, CancellationToken = default)`.
- [x] **3.4** `Services/Api/AuthService.cs` — POST `/Token` con `FormUrlEncodedContent`, `PropertyNameCaseInsensitive=true`, mapeo 400/401·red/timeout·5xx·vacío/malformado a `LoginResult`/`AuthErrorKind` con mensajes de `AuthMessages`.

#### Etapa 4 — Persistencia
- [x] **4.1** `Services/Storage/ITokenStore.cs` — `HasValidSessionAsync`, `GetSessionAsync`, `SaveSessionAsync`, `ClearAsync`.
- [x] **4.2** `Services/Storage/SessionDatabase.cs` (+ `SessionRow`) — SQLite lazy (`Lazy<Task<SQLiteAsyncConnection>>`), `insignia.db3` en `FileSystem.AppDataDirectory`, `Batteries_V2.Init()` idempotente, upsert de fila única (`Id=1`).
- [x] **4.3** `Services/Storage/TokenStore.cs` — híbrido SecureStorage (clave `access_token`) + SQLite, `try/catch` defensivo en set/get/remove, fallback a SQLite sin cifrar (`IsEncrypted=false`) con warning, `ClearAsync` limpia ambos.

### Archivos creados (lote 2)
| Archivo | Nota |
|---------|------|
| `Services/Api/AuthConfig.cs` | Constantes (URL, endpoint, margen, timeout) |
| `Services/Api/TokenExpiry.cs` | Cálculo puro de expiración |
| `Services/Api/IAuthService.cs` | Contrato auth |
| `Services/Api/AuthService.cs` | POST `/Token` + mapeo de errores |
| `Services/Storage/ITokenStore.cs` | Contrato persistencia |
| `Services/Storage/SessionDatabase.cs` | SQLite lazy + `SessionRow` |
| `Services/Storage/TokenStore.cs` | Híbrido SecureStorage + SQLite |

### Archivos modificados (lote 2)
| Archivo | Cambio |
|---------|--------|
| `InsigniaConsultoresApp/InsigniaConsultoresApp.csproj` | `sqlite-net-pcl` 1.9.172 |
| `openspec/changes/login-autenticacion-token/tasks.md` | Marcas `[x]` (1.4, 3.1–3.4, 4.1–4.3) |

### Desviaciones / decisiones (lote 2)

- **D5 — `ITokenStore` con 4 métodos**: el resumen del orquestador mencionaba `SaveAsync/GetAsync/ClearAsync`, pero `tasks.md` 4.1 y `design.md` (Decisión 6) definen `HasValidSessionAsync` + `GetSessionAsync` + `SaveSessionAsync` + `ClearAsync`. Se siguió el diseño/tareas (autoritativos); `HasValidSessionAsync` es necesario para el ruteo de arranque de Etapa 7.
- **D6 — `AuthConfig.RequestTimeoutSeconds = 30`**: el diseño ubica `Timeout = TimeSpan.FromSeconds(30)` en el `HttpClient` (Decisión 1, etapa 8). Se añadió la constante para centralizar el valor y satisfacer "timeout 30s" del orquestador; la etapa 8 la aplicará al registrar el `HttpClient`.
- **D7 — `AuthService.LoginAsync` acepta `CancellationToken = default`**: superset del contrato del diseño; preserva la firma `LoginAsync(TokenRequest)` y permite cancelación.

## Restante (siguientes lotes)
- Etapas 5–8 (ViewModels, vistas/estilos, Shell/navegación, DI) y Etapa 9/10 (limpieza + verificación).
- NO se registró nada en DI (wiring es Etapa 8). `TokenStore` recibe `ILogger<TokenStore>?` opcional (default `null`) por lo que no requiere registro.

---

## Lote 3 — Etapas 5 y 6 (ViewModels + Vistas/estilos) + fix de seguridad SQLite

> Fecha: 2026-11-13 · Modo: **Standard**.

### Resultado del build

`dotnet build InsigniaConsultoresApp\InsigniaConsultoresApp.csproj -f net10.0-android` → **SUCCESS** (0 errores, 3 advertencias, todas NU1903).

| Advertencia | Antes (lote 2) | Después (lote 3) |
|-------------|----------------|------------------|
| NU1903 (`SQLitePCLRaw.lib.e_sqlite3.android`) | en **2.1.2** | en **2.1.11** — **no resoluble por versión** (ver B1) |
| XA0141 (sin soporte páginas 16 KB) | presente | **ELIMINADA** ✅ |
| CS8622 / CS8625 (nulabilidad code-behind) | — | corregidas en este lote ✅ |

### Alcance A — Fix de seguridad SQLite

- Agregado `PackageReference` explícito `SQLitePCLRaw.bundle_green` **2.1.11** (última estable 2.1.x; restaura limpio sobre `net10.0-android`) que **sobreescribe** la transitiva 2.1.2 de `sqlite-net-pcl` 1.9.172 (que **no se tocó**).
- **XA0141 resuelto**: 2.1.11 incluye `libe_sqlite3.so` con soporte de páginas de 16 KB (Android 16).
- **NU1903 parcial**: la advertencia ahora apunta a 2.1.11, pero persiste porque la advisory **GHSA-2m69-gcr7-jv3q (CVE-2025-6965)** afecta a `SQLitePCLRaw.lib.e_sqlite3.android <= 2.1.11` con **"Patched versions: None"** (SQLite < 3.50.2; no existe versión empaquetada corregida aún). **No se suprime** el warning (sería ocultar una vulnerabilidad real). Reportado como ítem bloqueado (ver B1).
- `SQLitePCL.Batteries_V2.Init()` **no requirió cambios**: ya está en `SessionDatabase` (lote 2) con guarda estática `_batteriesInitialized` + `lock` (llamado exactamente una vez).

### Etapa 5 — ViewModels
- [x] **5.1** `ViewModels/ViewModelBase.cs` — `INotifyPropertyChanged` + `SetProperty<T>` (sin CommunityToolkit).
- [x] **5.2** `ViewModels/LoginViewModel.cs` — `Username`, `Password`, `IsBusy`, `IsNotBusy` (computed), `ErrorMessage`, `HasError` (computed), `LoginCommand` (`Microsoft.Maui.Controls.Command`). Flujo: anti doble-envío (`if (IsBusy) return`) → `LoginValidator.Validate` → `IsBusy=true` → `IAuthService.LoginAsync` → `ITokenStore.SaveSessionAsync` → `Password=""` → `Shell.Current.GoToAsync("//home")`. `finally { IsBusy=false; }` + `catch` defensivo. Constructor inyecta `IAuthService` + `ITokenStore`.
- [x] **5.3** `ViewModels/HomeViewModel.cs` — `Username` (cargado desde sesión vía `LoadAsync()`) + `LogoutCommand` (`ITokenStore.ClearAsync()` → `GoToAsync("//login")`). Constructor inyecta `ITokenStore`.

### Etapa 6 — Vistas y estilos
- [x] **6.1** `Colors.xaml` — repoint `Primary`=#0F4C81, `PrimaryDark`=#0A3A63 (sin tocar/renombrar las claves de plantilla); agregada paleta `BrandPrimary`, `BrandDark`, `BrandLight`, `BrandOrange`, `BrandOrangeDark`, `BrandBackground`, `BrandTextBody`, `BrandInputBackground`, `BrandInputBorder`, `BrandSurface`, `BrandCardBorder` + `BrandFocusRing` (#26E8952B = rgba(232,149,43,.15)).
- [x] **6.2** `BrandStyles.xaml` — `BrandLabel`, `BrandTitle`, `BrandEntry` (Entry transparente) + `BrandEntryBorder` (borde 1.5px/radio 10), `BrandButton` (CTA gradiente #E8952B→#C97A18 vía `Button.Background`, radio 999, texto blanco bold, sombra 0 6 18 rgba(232,149,43,.35)), `BrandPrimaryButton` (#0F4C81).
- [x] **6.3** `App.xaml` — merge de `BrandStyles.xaml` en `MergedDictionaries`.
- [x] **6.4** `Views/LoginPage.xaml` — hero `LinearGradientBrush` (stops 0.0/0.55/1.0), logo `brand_logo.png` 56×56 radio 12 con sombra, título, `Entry` usuario/contraseña (`IsPassword=True`), `Label` error (solo visible con `HasError`, color #c53030), `Button` "Iniciar sesión" + `ActivityIndicator` overlay en `Grid` (sin botones Registrar/Recuperar). `Shell.FlyoutBehavior=Disabled`, `Shell.NavBarIsVisible=False`.
- [x] **6.5** `Views/LoginPage.xaml.cs` — constructor `LoginPage(LoginViewModel)`; foco de entrada (borde #E8952B + ring) vía eventos `Focused`/`Unfocused`.
- [x] **6.6** `Views/HomePage.xaml` — página en blanco con placeholder atenuado ("Sin contenido por ahora").
- [x] **6.7** `Views/HomePage.xaml.cs` — constructor `HomePage(HomeViewModel)`; `OnAppearing` → `LoadAsync()`.

### Archivos creados (lote 3)
| Archivo | Nota |
|---------|------|
| `ViewModels/ViewModelBase.cs` | Base `INotifyPropertyChanged` + `SetProperty` |
| `ViewModels/LoginViewModel.cs` | Estado del login + `LoginCommand` |
| `ViewModels/HomeViewModel.cs` | `Username` + `LogoutCommand` |
| `Resources/Styles/BrandStyles.xaml` | Estilos de marca (Label/Title/Entry/Border/Button) |
| `Views/LoginPage.xaml` | UI de login (hero + form + spinner) |
| `Views/LoginPage.xaml.cs` | Code-behind + foco del Entry |
| `Views/HomePage.xaml` | Placeholder en blanco |
| `Views/HomePage.xaml.cs` | Code-behind + carga de sesión |

### Archivos modificados (lote 3)
| Archivo | Cambio |
|---------|--------|
| `InsigniaConsultoresApp/InsigniaConsultoresApp.csproj` | `SQLitePCLRaw.bundle_green` 2.1.11 (parche seguridad) |
| `Resources/Styles/Colors.xaml` | Repoint `Primary`/`PrimaryDark` + paleta de marca + `BrandFocusRing` |
| `App.xaml` | Merge `BrandStyles.xaml` |
| `openspec/changes/login-autenticacion-token/tasks.md` | Marcas `[x]` (5.1–5.3, 6.1–6.7) |

### Desviaciones / decisiones (lote 3)

- **B1 — NU1903 bloqueado (upstream sin fix)**: `CVE-2025-6965` (agregados SQLite < 3.50.2) afecta `<= 2.1.11` con **"Patched versions: None"**. Subir el bundle a 2.1.11 elimina XA0141 y mueve NU1903 a la última versión disponible, pero **no existe** versión que lo corrija. Se reporta como ítem bloqueado (no se suprime el warning). Al salir un `SQLitePCLRaw` con SQLite ≥ 3.50.2, basta subir `bundle_green` en el `.csproj`.
- **D8 — `BrandEntry` dividido en Entry + Border**: MAUI `Entry` no expone borde/radio; el fondo/borde 1.5px/radio 10 viven en un `Border` contenedor (`BrandEntryBorder`), y `BrandEntry` es el estilo del `Entry` transparente. El foco (borde #E8952B + ring) se implementa en code-behind (`Focused`/`Unfocused`) porque `Border` no es enfocable.
- **D9 — Nombres de botón según alcance C**: `BrandButton` = CTA naranja (gradiente) usado en el botón de login; `BrandPrimaryButton` = azul sólido #0F4C81. (El diseño Decisión 11 llamaba "BrandPrimaryButton" al gradiente; el alcance del orquestador es autoritativo.)
- **D10 — Gradiente del CTA vía `Button.Background`**: `Button` MAUI admite `Background` (Brush), así se conserva un `Button` real (accesibilidad + overlay del spinner + `IsEnabled`), como exige Decisión 10.
- **D11 — Gradiente hero**: `StartPoint=0,0 EndPoint=1,1` aproxima el `120deg` CSS (coincide con Decisión 10); stops exactos 0.0/0.55/1.0.
- **D12 — Ring de foco en code-behind con literales**: `Colors.xaml` define `BrandFocusRing` (#26E8952B), pero el code-behind usa `Color.FromArgb("#E8952B")`/`"#dde5ee"` para el stroke/ring (evita la ambigüedad de lookup de recursos en `Application.Current.Resources`). Valores espejo documentados.
- **D13 — `HomeViewModel` expone `LogoutCommand`** (además de `Username`): pedido por el alcance B, aunque el diseño ubica el logout en `AppShell` (Etapa 7). No se registra nada en DI aún (Etapa 8); las vistas usan inyección por constructor y **compilan** sin registros.

## Restante (siguientes lotes)
- Etapa 7 (AppShell/Flyout + ruteo de arranque), Etapa 8 (DI en `MauiProgram`/`App.xaml.cs`), Etapa 9 (limpieza `MainPage` + referencias), Etapa 10 (verificación).
- Pendiente: re-evaluar `SQLitePCLRaw` cuando exista versión con SQLite ≥ 3.50.2 (cierra NU1903).

---

## Lote 4 (FINAL) — Etapas 7, 8, 9 y 10 (Shell/navegación + DI + limpieza + verificación)

> Fecha: 2026-11-14 · Modo: **Standard**. Cierra el change (todas las etapas completas).

### Resultado del build

| Config | Comando | Resultado |
|--------|---------|-----------|
| Debug | `dotnet build InsigniaConsultoresApp\InsigniaConsultoresApp.csproj -f net10.0-android` | **SUCCESS** — 0 errores, 3 advertencias (todas NU1903) |
| Release | `dotnet build ... -f net10.0-android -c Release` | **SUCCESS** — 0 errores, 3 advertencias (todas NU1903) |

Advertencias restantes (solo NU1903, aceptada como upstream sin fix — ver B1 del Lote 3):
- **NU1903** ×3: `SQLitePCLRaw.lib.e_sqlite3.android` 2.1.11, advisory GHSA-2m69-gcr7-jv3q (CVE-2025-6965). "Patched versions: None". No se suprime; al existir bundle con SQLite ≥ 3.50.2, subir `SQLitePCLRaw.bundle_green`.
- La advertencia **CS8602** que apareció en el primer build de este lote (`IPlatformApplication.Current` nullable) se corrigió con `IPlatformApplication.Current!.Services`; el build final ya solo reporta NU1903. El build Release compila el AOT/nativo (`[96/98] Java.Interop.dll.so`) sin errores de firma/empaquetado.

### Tareas completadas

#### Etapa 7 — Shell y navegación
- [x] **7.1** `AppShell.xaml` reescrito: `FlyoutBehavior="Flyout"` en la raíz; `ShellContent` login (ruta `login`, `Shell.FlyoutBehavior="Disabled"`, `Shell.FlyoutItemIsVisible="False"`, `Shell.TabBarIsVisible="False"`, `ContentTemplate` → `LoginPage`) como primer ítem (contenido inicial); `FlyoutItem` home (ruta `home`, título "Inicio", `Shell.FlyoutBehavior="Flyout"`) → `HomePage`; `Shell.FlyoutHeader` con `brand_logo.png` (40×40) + `UsernameLabel` (bold, `BrandPrimary`) + subtítulo "Sesión activa"; `MenuItem` "Cerrar sesión" (`Clicked="OnLogoutClicked"`).
- [x] **7.2** `AppShell.xaml.cs`: constructor con inyección `ITokenStore`; `Loaded` (async, `try/catch`) → `HasValidSessionAsync()` → `GoToAsync("//home")` si válido, si no permanece en login; `Navigated` refresca/limpia `UsernameLabel` (al aterrizar en home lee `GetSessionAsync()`; al volver a login lo vacía); `OnLogoutClicked` → `ClearAsync()` → `GoToAsync("//login")` (absoluto, sin back stack). Ambos logout (MenuItem y `HomeViewModel.LogoutCommand`) terminan en el mismo estado.

#### Etapa 8 — Wiring / DI
- [x] **8.1** `MauiProgram.cs`: `HttpClient` singleton (`BaseAddress=AuthConfig.BaseUrl`, `Timeout=AuthConfig.RequestTimeoutSeconds`); `SessionDatabase` singleton; `ITokenStore`→`TokenStore` singleton; `IAuthService`→`AuthService` singleton; `LoginViewModel`/`HomeViewModel` transient; `LoginPage`/`HomePage`/`AppShell` transient.
- [x] **8.2** `App.xaml.cs`: `CreateWindow` resuelve `AppShell` vía `IPlatformApplication.Current!.Services.GetRequiredService<AppShell>()`.

#### Etapa 9 — Limpieza
- [x] **9.1** Eliminados `MainPage.xaml` y `MainPage.xaml.cs`.
- [x] **9.2** Grep sobre el proyecto: **0 referencias** a `MainPage`, `dotnet_bot.png`, `appicon.svg`, `appiconfg.svg`, `splash.svg` en `.csproj`/XAML/cs.
- [x] **9.3** `LoginPage.xaml` verificado: **sin** botones "Registrar" ni "Recuperar contraseña".

#### Etapa 10 — Verificación
- [x] **10.1** Build Debug y Release → SUCCESS (arriba).
- [x] **10.2** Prueba manual ordenada documentada (8 pasos, abajo).

### Archivos creados (lote 4)
Ninguno (solo edición/eliminación).

### Archivos modificados (lote 4)
| Archivo | Cambio |
|---------|--------|
| `InsigniaConsultoresApp/AppShell.xaml` | Reescrito: rutas login/home, FlyoutHeader, MenuItem |
| `InsigniaConsultoresApp/AppShell.xaml.cs` | Ruteo de arranque + logout + usuario del flyout |
| `InsigniaConsultoresApp/MauiProgram.cs` | Registros DI (HttpClient, servicios, VMs, páginas, Shell) |
| `InsigniaConsultoresApp/App.xaml.cs` | `CreateWindow` resuelve `AppShell` desde DI |
| `openspec/changes/login-autenticacion-token/tasks.md` | Marcas `[x]` (7.1–7.2, 8.1–8.2, 9.1–9.3, 10.1–10.2) |

### Archivos eliminados (lote 4)
- `InsigniaConsultoresApp/MainPage.xaml`
- `InsigniaConsultoresApp/MainPage.xaml.cs`

### Desviaciones / decisiones (lote 4)

- **D14 — Shell `ContentTemplate` resuelve páginas por DI (verificado)**: se confirma en la documentación de MAUI que, para usar inyección por constructor en `ContentPage`s dentro de Shell, basta registrarlas en DI ("register them with DI just like your view models"). Con `LoginPage`/`HomePage` registradas como transient, los `DataTemplate` (`ContentTemplate="{DataTemplate views:LoginPage}"`) se resuelven a través del `IServiceProvider` (ShellContentCreator). No se requirió ningún workaround; se conserva la inyección por constructor y la limpieza del DI.
- **D15 — Usuario del flyout se refresca en `Navigated`**: el diseño (Decisión 9) propone setear el usuario en code-behind tras leer la sesión. Como `Shell.Loaded` solo corre una vez al arranque (no al hacer login), se actualiza el `UsernameLabel` en el evento `Shell.Navigated` al aterrizar en `//home` (cubre arranque con sesión válida Y login exitoso) y se limpia al volver a login. Mantiene el principio "code-behind, no binding" del diseño.
- **D16 — Propiedades adjuntas de Shell**: se usó la sintaxis adjunta explícita `Shell.FlyoutBehavior`, `Shell.FlyoutItemIsVisible` y `Shell.TabBarIsVisible` (en lugar de `FlyoutItemIsVisible="False"` simple del boceto del diseño) para que la ocultación del login del flyout sea inambigua y compile con XamlC (`Shell.GetFlyoutItemIsVisible`/`GetTabBarIsVisible` son ABPs confirmadas en la API pública de MAUI).
- **D17 — `IPlatformApplication.Current!.Services`**: `IPlatformApplication.Current` es nullable; se aplica `!` porque ya está establecido al invocarse `CreateWindow`. Evita la advertencia CS8602 y es semánticamente correcto.
- **Sin cambios** en `AuthService`, `TokenStore`, `SessionDatabase`, `LoginValidator`, modelos ni specs (constraint cumplido).

### Prueba manual (ordenada — 8 pasos)

> Requiere un dispositivo/emulador Android con acceso a `https://api.insigniaconsultores.com.mx` y credenciales reales para los pasos 3–8.

1. **Arranque en frío sin sesión**: instalar/reabrir con datos limpios → se muestra `LoginPage` (hero, logo, título, usuario/contraseña, "Iniciar sesión"), **sin** hamburguesa/flyout y sin botones "Registrar"/"Recuperar contraseña".
2. **Contraseña incorrecta**: usuario válido + contraseña errónea → aparece "Usuario o contraseña incorrectos", el spinner se detiene y el botón se re-habilita.
3. **Login válido → Home + flyout con usuario**: credenciales correctas → spinner en el botón durante la carga → aterriza en Home ("Inicio" + placeholder); abrir el menú hamburguesa muestra logo, el **nombre de usuario** autenticado, subtítulo "Sesión activa", ítem "Inicio" y "Cerrar sesión".
4. **Back no regresa al login**: estando en Home, presionar retroceso → la app NO regresa al login (el stack fue reemplazado con la navegación absoluta `//home`).
5. **Kill + reopen → directo a Home**: cerrar la app por completo y reabrir → auto-login directo a Home (token válido), sin pasar por login.
6. **Token expirado → login**: forzar `expires_at` en el pasado (o esperar la expiración) y reabrir → se muestra login.
7. **Logout → login y sesión limpia**: "Cerrar sesión" → regresa al login sin flyout; verificar que SecureStorage (`access_token`) y la fila SQLite (`Session`) quedaron vacías; back no regresa a Home.
8. **Ícono/splash**: instalar en Android → el ícono del launcher y la pantalla splash muestran el logo de marca (fondo #0F4C81) sin artefactos de la plantilla.

## Estado final del change

Todas las etapas (1–10) completas. Build Debug y Release verdes (0 errores; única advertencia NU1903, upstream sin fix). Pendiente de revisión del orquestador para `sdd-verify` y posterior `sdd-archive`.

---

## Lote 5 — Revisión de UI (feedback de dispositivo real)

> Fecha: 2026-11-14 · Modo: **Standard**. Ajustes de layout de login + fix de teclado (IME) + fix de color nativo Android. No toca flujo, ViewModel, validación, mensajes de error, comportamiento spinner-en-botón, regla "sin Registrar/Recuperar" ni `AuthConfig.BaseUrl` (ya `https://webapi.insigniaconsultores.com.mx`).

### Resultado del build

`dotnet build InsigniaConsultoresApp\InsigniaConsultoresApp.csproj -f net10.0-android` → **SUCCESS** (0 errores, 2 advertencias NU1903 — upstream sin fix, ver B1 del Lote 3).

### Feedback implementado

1. **Hero compacto y texto completo**: eliminado `HeightRequest="260"`; el hero (`Grid` fila `Auto`) ahora se dimensiona por su contenido con padding modesto (`0,32,0,24`). Título cambiado a **"Insignia Consultores Financieros"** (FontSize 18, centrado). El formulario (`*`) gana el espacio vertical sobrante.
2. **Inputs sin "cajita"**: quitados los `Border` contenedores (`BrandEntryBorder`). Los `Entry` son transparentes, a todo el ancho del contenido y sin fondo/borde. La separación visual es solo un `BoxView` (línea inferior) de `1px #dde5ee` que pasa a `2px #E8952B` en foco.
3. **Etiquetas arriba**: `Label` "Usuario"/"Contraseña" encima de cada `Entry`, negrita 13px `#0F4C81` (nuevo estilo `BrandFieldLabel`). Ya no usan `Placeholder`.
4. **Footer**: `Label "By DESI"` centrado, 12px, `Gray500`, en la fila `Auto` inferior del `Grid` (`RowDefinitions="Auto,*,Auto"`).
5. **Fix de teclado (IME)**:
   - `Border` wrappers eliminados por completo (sospechosos del bloqueo del soft keyboard).
   - `MainActivity` → `WindowSoftInputMode = SoftInput.AdjustResize` (+ `using Android.Views;`).
   - Sin `TapGestureRecognizer` sobre los entries y los entries NO están dentro de un `ScrollView` (el login usa `Grid`, nunca `ScrollView`).
   - `Keyboard="Default"` en el entry de usuario; `IsPassword="True"` se conserva en contraseña.

### Fix verificación S1 — color nativo Android

`Platforms/Android/Resources/values/colors.xml` reemplazado: `colorPrimary=#0F4C81`, `colorPrimaryDark=#0A3A63`, `colorAccent=#E8952B` (antes púrpura plantilla `#512BD4`/`#2B0B98`). Elimina el flash púrpura en barra de estado/splash.

### Archivos modificados (lote 5)

| Archivo | Cambio |
|---------|--------|
| `Views/LoginPage.xaml` | Hero compacto (altura por contenido) + título completo + etiquetas arriba + inputs sin caja con línea inferior + footer "By DESI" |
| `Views/LoginPage.xaml.cs` | `SetLine(BoxView, bool)` reemplaza `SetFocus(Border, bool)`; línea 1px→2px y color `#dde5ee`→`#E8952B` en foco |
| `Resources/Styles/BrandStyles.xaml` | `BrandEntry` sin fondo/caja/placeholder; nuevo `BrandFieldLabel`; eliminado `BrandEntryBorder` |
| `Platforms/Android/MainActivity.cs` | `WindowSoftInputMode = SoftInput.AdjustResize` + `using Android.Views;` |
| `Platforms/Android/Resources/values/colors.xml` | `colorPrimary`/`colorPrimaryDark`/`colorAccent` → paleta de marca (sin púrpura) |
| `openspec/changes/login-autenticacion-token/tasks.md` | Lote 5 (5.1–5.6) con `[x]` y trazabilidad |
| `openspec/changes/login-autenticacion-token/specs/autenticacion-token/spec.md` | Requirement "Pantalla de inicio de sesión" + escenario de teclado |
| `openspec/changes/login-autenticacion-token/specs/branding/spec.md` | Tabla de componentes (separador/foco de input) + requirement de colores nativos Android |

### Desviaciones / decisiones (lote 5)

- **D18 — Línea inferior vía `BoxView` + code-behind**: MAUI `Entry` no expone un "bottom border" nativo; la línea inferior es un `BoxView` bajo cada `Entry`. El foco (color + grosor) se maneja en code-behind (`Focused`/`Unfocused`), continuando el patrón preexistente (D8/D12). Sin `Border` wrapper (requisito explícito del fix de IME).
- **D19 — `BrandEntryBorder` eliminado**: al quitar las cajas, el estilo quedó sin uso; se eliminó para no dejar código muerto. `BrandFieldLabel` es el nuevo estilo de etiqueta de campo.
- **D20 — `colorAccent=#E8952B`**: además de los azules pedidos, el accent nativo se alinea al naranja de marca (evita residuos púrpura `#2B0B98` en ripples de Android). No es un requisito funcional; es coherencia de marca.
- **Sin cambios** en `AuthService`, `TokenStore`, `SessionDatabase`, `LoginValidator`, ViewModels, `AuthConfig`, `AppShell`, `MauiProgram` ni `App.xaml.cs` (constraint cumplido). El overlay `ActivityIndicator` (`IsBusy`/`IsNotBusy`) se conserva intacto.

### Pendiente de verificación en dispositivo real

- Confirmar en el dispositivo que **al tocar cada campo se abre el teclado** (el fix es defensivo: `AdjustResize` + eliminación de `Border` wrappers + `Keyboard="Default"`; si persiste, investigar `ShowSoftInputOnFocus`/`CursorVisibility` del handler de `Entry` en Android).
