# Tasks: Fundación de autenticación (login + token)

> Change: `login-autenticacion-token` · Solo Android (`net10.0-android`) · Verificación por build (sin proyecto de tests).
> Etiquetas de trazabilidad: `BR` = branding · `AT` = autenticacion-token · `IN` = inicio-y-navegacion.

## Etapa 1: Proyecto y recursos

- [x] 1.1 `InsigniaConsultoresApp.csproj` → `TargetFrameworks` a `net10.0-android` únicamente; quitar líneas ios/maccatalyst/windows, `WindowsPackageType` y condiciones `SupportedOSPlatformVersion`/`TargetPlatformMinVersion` no-Android (conservar `SupportedOSPlatformVersion=21.0` android). `AT`
- [x] 1.2 Copiar `logo.png` desde `C:\Git\InsigniaConsultoresAxaPaginaWeb\imagenes\marca\logo.png` a `Resources\AppIcon\logo.png`, `Resources\Splash\logo.png` y `Resources\Images\brand_logo.png` (renombrado para evitar colisión de Resizetizer; en XAML referenciar `brand_logo.png`). `BR-R1.1` `BR-R1.2` `IN-R2.1`
- [x] 1.3 `InsigniaConsultoresApp.csproj` → `MauiIcon Include="Resources\AppIcon\logo.png"` y `MauiSplashScreen Include="Resources\Splash\logo.png" Color="#0F4C81" BaseSize="128,128"`. `BR-R1.1` `BR-R1.2`
- [x] 1.4 `InsigniaConsultoresApp.csproj` → agregar `<PackageReference Include="sqlite-net-pcl" Version="1.9.172" />`. `AT-R5.1`
- [x] 1.5 `InsigniaConsultoresApp.csproj` → quitar línea `<MauiImage Update="Resources\Images\dotnet_bot.png" ... />`. Limpieza
- [x] 1.6 Eliminar archivos plantilla: `Resources\AppIcon\appicon.svg`, `Resources\AppIcon\appiconfg.svg`, `Resources\Splash\splash.svg`, `Resources\Images\dotnet_bot.png`. Limpieza — la eliminación de `MainPage.xaml`/`MainPage.xaml.cs` se movió a Etapa 9 (AppShell.xaml aún los referencia).

## Etapa 2: Capa de modelos y validación

- [x] 2.1 Crear `Models/Requests/TokenRequest.cs`: `Username`, `Password`, `GrantType` + `ToForm()` (body url-encoded). `AT-R3.1`
- [x] 2.2 Crear `Models/Responses/TokenResponse.cs`: DTO con `JsonPropertyName` (`access_token`, `token_type`, `expires_in`, `refresh_token`). `AT-R3.1`
- [x] 2.3 Crear `Models/Responses/LoginResult.cs`: enum `AuthErrorKind`, `LoginResult` (`Ok`/`Fail`) y `AuthMessages.Get`. `AT-R3.2` `AT-R9`
- [x] 2.4 Crear `Models/SessionModel.cs`: `Username`, `TokenType`, `AccessToken`, `ExpiresAtUtc`, `LastLoginUtc`, `IsEncrypted`. `AT-R5.1` `AT-R6`
- [x] 2.5 Crear `Validations/LoginValidator.cs`: `ValidationResult` + `LoginValidator.Validate(username, password)`. `AT-R2.1`

## Etapa 3: API y expiración

- [x] 3.1 Crear `Services/Api/AuthConfig.cs`: `BaseUrl`, `TokenEndpoint`, `TokenExpiryMarginSeconds=20*60`. `AT-R3.1` `AT-R6`
- [x] 3.2 Crear `Services/Api/TokenExpiry.cs`: `CalculateUtc(nowUtc, expiresInSeconds)` con margen (nunca negativo). `AT-R6.1` `AT-R6.2`
- [x] 3.3 Crear `Services/Api/IAuthService.cs`: `Task<LoginResult> LoginAsync(TokenRequest)`. `AT-R3`
- [x] 3.4 Crear `Services/Api/AuthService.cs`: POST `/Token` (`FormUrlEncodedContent`), deserializar `PropertyNameCaseInsensitive=true`, mapear 400/401·red·5xx·malformado. `AT-R3.1` `AT-R3.2` `AT-R9`

## Etapa 4: Persistencia

- [x] 4.1 Crear `Services/Storage/ITokenStore.cs`: `HasValidSessionAsync`, `GetSessionAsync`, `SaveSessionAsync`, `ClearAsync`. `AT-R5` `AT-R7` `AT-R10`
- [x] 4.2 Crear `Services/Storage/SessionDatabase.cs` (+ `SessionRow`): init lazy `SQLiteAsyncConnection` + `CreateTableAsync`, `SQLitePCL.Batteries_V2.Init()`, guardar/leer/limpiar. `AT-R5.1`
- [x] 4.3 Crear `Services/Storage/TokenStore.cs`: híbrido SecureStorage (clave `access_token`) + SQLite, `try/catch` defensivo (keystore), `ClearAsync` limpia ambos. `AT-R5.1` `AT-R7` `AT-R10.1`

## Etapa 5: ViewModels

- [x] 5.1 Crear `ViewModels/ViewModelBase.cs`: base `INotifyPropertyChanged` + `SetProperty`. Base
- [x] 5.2 Crear `ViewModels/LoginViewModel.cs`: `Username`, `Password`, `ErrorMessage`, `IsBusy`, `IsNotBusy`, `HasError`, `LoginCommand` (validar → busy → auth → guardar → navegar `//home`). `AT-R2.1` `AT-R4.1` `AT-R5.1` `AT-R9`
- [x] 5.3 Crear `ViewModels/HomeViewModel.cs`: `Username` para el header del flyout. `IN-R2.1`

## Etapa 6: Vistas y estilos

- [x] 6.1 Editar `Resources/Styles/Colors.xaml`: repoint `Primary`=#0F4C81, `PrimaryDark`=#0A3A63 + paleta `BrandPrimary`…`BrandCardBorder`. `BR-R2.1`
- [x] 6.2 Crear `Resources/Styles/BrandStyles.xaml`: `BrandEntry` (radio 10, borde `BrandInputBorder`) y `BrandPrimaryButton` (gradiente CTA #E8952B→#C97A18, radio 999). `BR-R3.1`
- [x] 6.3 Editar `App.xaml`: mergear `Resources/Styles/BrandStyles.xaml` en `MergedDictionaries`. `BR-R2.1` `BR-R3.1`
- [x] 6.4 Crear `Views/LoginPage.xaml`: hero gradiente + logo 56×56 + título + `Entry` usuario/contraseña + `Label` error + `Button` con `ActivityIndicator` overlay. `AT-R1.1` `AT-R4.1`
- [x] 6.5 Crear `Views/LoginPage.xaml.cs`: code-behind (BindingContext VM). `AT-R1.1`
- [x] 6.6 Crear `Views/HomePage.xaml`: página en blanco (placeholder). `IN-R1.1` `IN-R3.1`
- [x] 6.7 Crear `Views/HomePage.xaml.cs`: code-behind. `IN-R1.1`

## Etapa 7: Shell y navegación

- [x] 7.1 Editar `AppShell.xaml`: `ShellContent` login (oculto, `FlyoutBehavior=Disabled`) + `FlyoutItem` home + `FlyoutHeader` (logo + `UsernameLabel`) + `MenuItem` "Cerrar sesión". `IN-R1.1` `IN-R2.1`
- [x] 7.2 Editar `AppShell.xaml.cs`: ruteo en `Loaded` (`HasValidSessionAsync` → `//home` o `//login`), handler logout (`ClearAsync` → `//login`), setear `UsernameLabel`. `AT-R7.1` `AT-R7.2` `AT-R10.1` `IN-R4.1`

## Etapa 8: Wiring / DI

- [x] 8.1 Editar `MauiProgram.cs`: registrar `HttpClient` singleton, `IAuthService`/`ITokenStore` singleton, `LoginViewModel`/`HomeViewModel` transient, `LoginPage`/`HomePage`/`AppShell` transient. DI
- [x] 8.2 Editar `App.xaml.cs`: resolver `AppShell` vía `IPlatformApplication.Current.Services.GetRequiredService<AppShell>()` en `CreateWindow`. DI

## Etapa 9: Limpieza

- [x] 9.1 Eliminar archivos plantilla `MainPage.xaml` y `MainPage.xaml.cs` (movido desde Etapa 1: `AppShell.xaml` aún los referencia; se eliminan tras el cambio de Shell en Etapa 7). Limpieza
- [x] 9.2 Verificar que no queden referencias a `MainPage`, `dotnet_bot.png`, `appicon.svg` en `.csproj`, `AppShell.xaml` o XAML. Limpieza
- [x] 9.3 Verificar que la UI de login NO incluye botones "Registrar" ni "Recuperar contraseña". `AT-R1.1`

## Etapa 10: Verificación

- [x] 10.1 Build: `dotnet build -f net10.0-android` → SUCCESS. `verify`
- [x] 10.2 Prueba manual ordenada (abajo). `verify`

### Prueba manual (ordenada)

1. **Happy path**: login válido → spinner en botón + botón deshabilitado → Home con flyout; back NO regresa al login.
2. **Contraseña incorrecta**: 400/401 → "Usuario o contraseña incorrectos", spinner se detiene, botón re-habilitado.
3. **Sin red**: modo avión → "No se pudo conectar con el servidor".
4. **Arranque en frío con token válido**: cerrar/reabrir → auto-login directo a Home.
5. **Expiración**: `expires_at` en el pasado → al reabrir se muestra login.
6. **Logout**: "Cerrar sesión" → login sin flyout; SecureStorage + SQLite limpios.
7. **Back-navigation**: estando en Home, el retroceso NO regresa al login.
8. **Ícono/splash**: instalar en Android → ícono y splash muestran `logo.png` sin artefactos de plantilla.
9. **Validación de campos**: usuario o contraseña vacíos → mensaje inline, sin request, botón re-habilitado.

---

## Lote 5 — Revisión de UI (feedback de dispositivo real)

> Fecha: 2026-11-14 · Modo: **Standard**. Ajustes de layout + fix de teclado (IME). No toca flujo, VM, validación ni AuthConfig.

- [x] **5.1** `Views/LoginPage.xaml` → hero compacto (sin `HeightRequest` fijo, altura por contenido) con título **"Insignia Consultores Financieros"**; etiquetas "Usuario"/"Contraseña" arriba de cada campo; inputs **sin caja/borde/fondo** a todo el ancho con solo línea inferior (BoxView 1px `#dde5ee`); footer **"By DESI"** centrado. `AT-R1.1`
- [x] **5.2** `Views/LoginPage.xaml.cs` → foco del input cambia la línea inferior (1px `#dde5ee` → 2px `#E8952B`), en reemplazo del antiguo `Border`/ring. `AT-R1.1`
- [x] **5.3** `Resources/Styles/BrandStyles.xaml` → `BrandEntry` sin fondo/caja/placeholder, `BrandFieldLabel` (negrita 13px `#0F4C81`), eliminado `BrandEntryBorder` (ya sin uso). `BR-R3.1`
- [x] **5.4** `Platforms/Android/MainActivity.cs` → `WindowSoftInputMode = SoftInput.AdjustResize` (+ `using Android.Views;`) para que el teclado abra correctamente. `AT-R1.1`
- [x] **5.5** `Platforms/Android/Resources/values/colors.xml` → reemplazar púrpura plantilla `#512BD4` por `colorPrimary=#0F4C81`, `colorPrimaryDark=#0A3A63`, `colorAccent=#E8952B` (elimina flash púrpura en barra de estado/splash). `BR-R1.2`
- [x] **5.6** Specs: `specs/autenticacion-token/spec.md` (nuevo layout + teclado) y `specs/branding/spec.md` (separador/foco de input + colores nativos). `BR` `AT`
