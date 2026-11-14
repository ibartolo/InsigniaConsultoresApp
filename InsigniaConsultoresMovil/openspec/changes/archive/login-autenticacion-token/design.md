# Design: Fundación de autenticación (login + token)

## Technical Approach

Reemplazar la plantilla MAUI ("Hello World", sin DI/MVVM) por una base de autenticación OAuth2 password-grant con MVVM simple y DI en `MauiProgram`. Se agregan las capas `Models`, `Services`, `Validations`, `ViewModels`, `Views` con clases planas (sin repositories/unit-of-work/CQRS). El token se persiste híbridamente (SecureStorage + SQLite) y el arranque decide login vs Home dentro de un único `AppShell`. Se reemplazan ícono y splash con `logo.png` y se aplica la paleta de marca. Verificación = build-only (no hay proyecto de tests).

---

## Architecture Decisions

### Decisión 1 — DI y composición

**Elección**: registrar en `MauiProgram.CreateMauiApp()`:

| Servicio | Lifetime | Razón |
|----------|----------|-------|
| `HttpClient` | Singleton | Un único host de API; evita agotar sockets del pool; se configura `BaseAddress` y `Timeout=30s` |
| `IAuthService`/`AuthService` | Singleton | Sin estado; depende del `HttpClient` singleton |
| `ITokenStore`/`TokenStore` | Singleton | Estado de sesión compartido; inicialización lazy de SQLite |
| `LoginViewModel`, `HomeViewModel` | Transient | Estado por página/vida de la vista |
| `LoginPage`, `HomePage`, `AppShell` | Transient | Resueltos por Shell/DI con inyección por constructor |

**Alternativas descartadas**: `HttpClient` por factory/transient (sin beneficio aquí y multiplica conexiones); `IHttpClientFactory` (sobre-ingeniería para un endpoint); VMs singleton (riesgo de estado cruzado).

**Bosquejo**:
```csharp
builder.Services.AddSingleton(_ => new HttpClient
{
    BaseAddress = new Uri(AuthConfig.BaseUrl),
    Timeout = TimeSpan.FromSeconds(30),
});
builder.Services.AddSingleton<IAuthService, AuthService>();
builder.Services.AddSingleton<ITokenStore, TokenStore>();
builder.Services.AddTransient<LoginViewModel>();
builder.Services.AddTransient<HomeViewModel>();
builder.Services.AddTransient<LoginPage>();
builder.Services.AddTransient<HomePage>();
builder.Services.AddTransient<AppShell>();
```
`App.CreateWindow` resuelve `AppShell` vía `IPlatformApplication.Current.Services.GetRequiredService<AppShell>()`. Las páginas reciben su VM por constructor (Shell resuelve `ContentTemplate`/`DataTemplate` a través del `IServiceProvider`). `LoginValidator` es estático (sin registro).

### Decisión 2 — Layering (clases y responsabilidades)

| Capa | Clase/Interfaz | Responsabilidad |
|------|----------------|-----------------|
| `Models/Requests` | `TokenRequest` | Datos del login (`username`, `password`, `grant_type`); construye el body url-encoded |
| `Models/Responses` | `TokenResponse` | DTO del token con `JsonPropertyName` (`access_token`, `token_type`, `expires_in`, + `refresh_token` opcional) |
| `Models/Responses` | `LoginResult` + `AuthErrorKind` | Resultado discriminado para la UI + mensaje en español |
| `Models` | `SessionModel` | Sesión persistida: `Username`, `TokenType`, `AccessToken`, `ExpiresAtUtc`, `LastLoginUtc`, `IsEncrypted` |
| `Services/Api` | `IAuthService` / `AuthService` | `LoginAsync(TokenRequest)` → POST `/Token`; mapea HTTP/red a `LoginResult` |
| `Services/Api` | `AuthConfig` | Constantes: `BaseUrl`, `TokenEndpoint`, `TokenExpiryMarginSeconds` |
| `Services/Api` | `TokenExpiry` | Cálculo puro de `expires_at` con margen |
| `Services/Storage` | `ITokenStore` / `TokenStore` | Persistencia híbrida (SecureStorage + SQLite); `ClearAsync` |
| `Services/Storage` | `SessionDatabase` (+`SessionRow`) | SQLite lazy init, tabla `Session`, guardar/leer/limpiar |
| `Validations` | `LoginValidator` (+`ValidationResult`) | Validación síncrona pura de campos |
| `ViewModels` | `ViewModelBase` | Base `INotifyPropertyChanged` (10 líneas) |
| `ViewModels` | `LoginViewModel` | Estado del formulario, `LoginCommand`, `IsBusy`, `ErrorMessage` |
| `ViewModels` | `HomeViewModel` | `Username` para el flyout |
| `Views` | `LoginPage` / `HomePage` | UI; `LoginPage` sin flyout, `HomePage` con flyout |

### Decisión 3 — Solicitud de token

**Elección**: body con `FormUrlEncodedContent` (codifica valores, incl. caracteres especiales de la contraseña) y lectura robusta con `PropertyNameCaseInsensitive=true`.

```csharp
var body = new FormUrlEncodedContent(new Dictionary<string, string>
{
    ["grant_type"] = "password",
    ["username"]  = request.Username ?? "",
    ["password"]  = request.Password ?? "",
});
using var resp = await _http.PostAsync(AuthConfig.TokenEndpoint, body);
```

```csharp
public sealed class TokenResponse
{
    [JsonPropertyName("access_token")]  public string? AccessToken  { get; set; }
    [JsonPropertyName("token_type")]    public string? TokenType    { get; set; }
    [JsonPropertyName("expires_in")]    public int?    ExpiresIn    { get; set; }
    [JsonPropertyName("refresh_token")] public string? RefreshToken { get; set; }
}

var json = await resp.Content.ReadAsStringAsync();
var dto = JsonSerializer.Deserialize<TokenResponse>(json,
    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
```
`PropertyNameCaseInsensitive` cubre casing distinto (Web API 2 suele devolver PascalCase); los campos extra se ignoran por defecto. Si `json` es vacío o `dto.AccessToken` es null → `MalformedResponse`.

### Decisión 4 — Modelo de error

**Elección**: `LoginResult` + enum `AuthErrorKind` (no excepciones). Los fallos de auth (400/401, timeout, 5xx, body malformado) son **resultados esperados**, no flujo excepcional; un tipo de resultado mantiene el `try/catch` del VM mínimo y centraliza el mapeo de mensajes en un solo lugar.

```csharp
public enum AuthErrorKind { None, InvalidCredentials, Network, Server, MalformedResponse, Storage }

public sealed class LoginResult
{
    public bool Success { get; private init; }
    public SessionModel? Session { get; private init; }
    public AuthErrorKind Error { get; private init; }
    public string Message { get; private init; } = "";
    public static LoginResult Ok(SessionModel s) => new() { Success = true, Session = s };
    public static LoginResult Fail(AuthErrorKind e, string msg) => new() { Error = e, Message = msg };
}

public static class AuthMessages
{
    public static string Get(AuthErrorKind e) => e switch
    {
        AuthErrorKind.InvalidCredentials => "Usuario o contraseña incorrectos",
        AuthErrorKind.Network            => "No se pudo conectar con el servidor",
        AuthErrorKind.Server             => "Ocurrió un error en el servidor, intenta de nuevo",
        AuthErrorKind.MalformedResponse  => "Respuesta inválida del servidor",
        AuthErrorKind.Storage            => "No se pudo guardar la sesión",
        _                                => "Ocurrió un error",
    };
}
```
Mapeo en `AuthService`: 400/401 → `InvalidCredentials`; `HttpRequestException`/`TaskCanceledException` (timeout) → `Network`; 5xx → `Server`; body vacío/DTO nulo → `MalformedResponse`.

### Decisión 5 — Validaciones

**Elección**: `LoginValidator` estático + `ValidationResult` (lista de mensajes), sin FluentValidation. Puro, síncrono, sin dependencias.

```csharp
public sealed class ValidationResult
{
    public bool IsValid => Messages.Count == 0;
    public IReadOnlyList<string> Messages { get; }
    public ValidationResult(IReadOnlyList<string> m) => Messages = m;
}

public static class LoginValidator
{
    public static ValidationResult Validate(string? username, string? password)
    {
        var msgs = new List<string>();
        if (string.IsNullOrWhiteSpace(username)) msgs.Add("Ingresa tu usuario");
        if (string.IsNullOrWhiteSpace(password)) msgs.Add("Ingresa tu contraseña");
        return new ValidationResult(msgs);
    }
}
```
El VM une los mensajes en `ErrorMessage` y, si `IsValid`, prosigue. Al no depender de nada, es verificable por lectura/invocación directa (sin proyecto de tests).

### Decisión 6 — Persistencia híbrida (SecureStorage + SQLite)

**Elección**: `ITokenStore` con implementación `TokenStore` + `SessionDatabase` (SQLite) y wrapper seguro sobre `SecureStorage`.

```csharp
public interface ITokenStore
{
    Task<bool> HasValidSessionAsync();
    Task<SessionModel?> GetSessionAsync();
    Task SaveSessionAsync(SessionModel session);
    Task ClearAsync();
}
```

- **SecureStorage**: clave `"access_token"`. En Android se respalda con el Keystore del dispositivo.
- **SQLite**: archivo `Path.Combine(FileSystem.AppDataDirectory, "insignia.db3")`; tabla `Session` (`Username`, `TokenType`, `AccessToken`, `ExpiresAtUtc`, `LastLoginUtc`, `IsEncrypted`). Inicialización lazy async (`SQLiteAsyncConnection` creado en el primer uso + `CreateTableAsync`); llamar `SQLitePCL.Batteries_V2.Init()` (idempotente) una vez.
- **Robustez del keystore del dispositivo**: `SecureStorage.SetAsync/GetAsync` puede lanzar si el Keystore no está disponible. El wrapper captura y degrada:

```csharp
public async Task SaveSessionAsync(SessionModel s)
{
    try
    {
        await SecureStorage.Default.SetAsync(AccessTokenKey, s.AccessToken ?? "");
        s.IsEncrypted = true;
    }
    catch (Exception ex)
    {
        // Keystore del dispositivo no disponible: degradar a SQLite sin cifrar
        _logger?.LogWarning(ex, "SecureStorage no disponible; token en SQLite sin cifrar");
        s.IsEncrypted = false;
    }
    await _db.SaveAsync(s); // SQLite: token solo si IsEncrypted=false
}
```
`GetSessionAsync` lee la fila SQLite; si `IsEncrypted` recupera el token de SecureStorage, si no, del `AccessToken` de la fila. `ClearAsync` borra SecureStorage (`SecureStorage.Remove`) y `DeleteAllAsync<SessionRow>()`.

### Decisión 7 — Lógica de expiración

**Elección**: constante única `AuthConfig.TokenExpiryMarginSeconds = 20 * 60`; cálculo puro en `TokenExpiry.CalculateUtc`.

```csharp
public static class AuthConfig
{
    public const string BaseUrl = "https://api.insigniaconsultores.com.mx";
    public const string TokenEndpoint = BaseUrl + "/Token";
    public const int TokenExpiryMarginSeconds = 20 * 60; // 20 min
}

public static class TokenExpiry
{
    // expires_at = now + expires_in - margen; si expires_in falta o <= margen,
    // se usa expires_in sin margen (nunca negativo). null => caduca ya (seguro).
    public static DateTime CalculateUtc(DateTime nowUtc, int? expiresInSeconds)
    {
        int lifetime = expiresInSeconds ?? 0;
        int effective = lifetime > AuthConfig.TokenExpiryMarginSeconds
            ? lifetime - AuthConfig.TokenExpiryMarginSeconds
            : lifetime;
        return nowUtc.AddSeconds(effective); // siempre >= nowUtc
    }
}
```

### Decisión 8 — Ruteo de arranque (login vs Home)

**Elección**: **`AppShell` siempre es la raíz**; la decisión ocurre en `AppShell.Loaded` (async void con try/catch). El primer `ShellContent` es `login` (oculto, `FlyoutBehavior=Disabled`) y es el contenido inicial por defecto; `home` es el `FlyoutItem` visible.

**Alternativas descartadas**:
- *`StartupPage` que reemplaza `Window.Page`*: añade un ciclo de vida extra y un swap manual de raíz frágil frente al flyout/back de Shell.
- *`App.OnStart` fire-and-forget*: corre antes de que el Shell esté listo → race con la navegación.
- *`App.CreateWindow` async*: no puede `await` por contrato.

El arranque absoluto (`//home` / `//login`) **reemplaza** el stack ⇒ el back nunca regresa al login.

### Decisión 9 — Shell / Flyout

```xml
<Shell x:Class="...AppShell" FlyoutBehavior="Flyout" Shell.FlyoutBehavior="Flyout">
  <!-- login: oculto del flyout, sin barra, contenido inicial -->
  <ShellContent Title="Login" Route="login"
                FlyoutItemIsVisible="False"
                Shell.FlyoutBehavior="Disabled"
                ContentTemplate="{DataTemplate views:LoginPage}" />
  <!-- home: flyout visible -->
  <FlyoutItem Title="Inicio" Route="home" Shell.FlyoutBehavior="Flyout">
    <ShellContent ContentTemplate="{DataTemplate views:HomePage}" />
  </FlyoutItem>

  <Shell.FlyoutHeader>
    <Grid ColumnDefinitions="Auto,*" Padding="16">
      <Image Source="logo.png" WidthRequest="40" HeightRequest="40" />
      <Label x:Name="UsernameLabel" Grid.Column="1" VerticalOptions="Center" />
    </Grid>
  </Shell.FlyoutHeader>

  <MenuItem Text="Cerrar sesión" Clicked="OnLogoutClicked" />
</Shell>
```
- Inicio (`//login` inicial, `//home` tras validar); login exitoso → `GoToAsync("//home")` (absoluto).
- Logout (`OnLogoutClicked`): `await _tokenStore.ClearAsync(); await GoToAsync("//login");` — `//login` es absoluto y `login` no está en el flyout ⇒ no queda en el stack. El nombre de usuario del header se setea en code-behind tras leer la sesión (más simple que binding para un header).

### Decisión 10 — Composición UI del Login

Estructura XAML (ContentPage `Shell.FlyoutBehavior="Disabled"`, `Shell.NavBarIsVisible="False"`, fondo `BrandBackground`):

```xml
<Grid RowDefinitions="Auto,*">
  <Border Grid.Row="0" StrokeThickness="0" HeightRequest="260">
    <Border.Background>
      <LinearGradientBrush StartPoint="0,0" EndPoint="1,1">
        <GradientStop Color="{StaticResource BrandDark}"   Offset="0.0"/>
        <GradientStop Color="{StaticResource BrandPrimary}" Offset="0.55"/>
        <GradientStop Color="{StaticResource BrandLight}"  Offset="1.0"/>
      </LinearGradientBrush>
    </Border.Background>
    <VerticalStackLayout HorizontalOptions="Center" VerticalOptions="Center" Spacing="12">
      <Border WidthRequest="56" HeightRequest="56" StrokeThickness="0"
              StrokeShape="RoundRectangle 12" BackgroundColor="White">
        <Image Source="logo.png" Aspect="AspectFill" />
      </Border>
      <Label Text="Insignia Consultores" TextColor="White" FontSize="22"
             FontAttributes="Bold" HorizontalOptions="Center" />
    </VerticalStackLayout>
  </Border>

  <VerticalStackLayout Grid.Row="1" Padding="24" Spacing="16">
    <Entry Placeholder="Usuario" Text="{Binding Username}" Style="{StaticResource BrandEntry}" />
    <Entry Placeholder="Contraseña" IsPassword="True" Text="{Binding Password}" Style="{StaticResource BrandEntry}" />
    <Label Text="{Binding ErrorMessage}" TextColor="Red" IsVisible="{Binding HasError}" />
    <Grid>
      <Button Text="Iniciar sesión" Command="{Binding LoginCommand}"
              IsEnabled="{Binding IsNotBusy}" Style="{StaticResource BrandPrimaryButton}" />
      <ActivityIndicator IsRunning="{Binding IsBusy}" IsVisible="{Binding IsBusy}"
                         Color="White" HorizontalOptions="Center" VerticalOptions="Center" />
    </Grid>
  </VerticalStackLayout>
</Grid>
```
**Spinner en el botón**: `Button` de MAUI **no** admite contenido arbitrario (solo texto/imagen), por lo que no es posible meter un `ActivityIndicator` dentro. Se usa **overlay con `Grid`** (la opción más simple que funciona): `Button` + `ActivityIndicator` superpuestos en la misma celda, ambos ligados a `IsBusy`. Se descarta un layout custom con `TapGestureRecognizer` (pierde semántica de botón y accesibilidad). El VM expone `IsBusy` e `IsNotBusy` (computed `!IsBusy`), garantizando anti doble-envío.

### Decisión 11 — Estilos

**Elección**: **editar `Resources/Styles/Colors.xaml`** (fuente única de colores) + **agregar `Resources/Styles/BrandStyles.xaml`** (estilos de marca) y mergearlo en `App.xaml`. Se evita tocar el `Styles.xaml` de la plantilla.

Keys en `Colors.xaml`:
```xml
<!-- Repoint (cambia el look por defecto de Button/Indicator/Shell) -->
<Color x:Key="Primary">#0F4C81</Color>
<Color x:Key="PrimaryDark">#0A3A63</Color>
<!-- Paleta de marca (nueva) -->
<Color x:Key="BrandPrimary">#0F4C81</Color>
<Color x:Key="BrandDark">#0A3A63</Color>
<Color x:Key="BrandLight">#1B6BA8</Color>
<Color x:Key="BrandOrange">#E8952B</Color>
<Color x:Key="BrandOrangeDark">#C97A18</Color>
<Color x:Key="BrandBackground">#FDF8F1</Color>
<Color x:Key="BrandTextBody">#4A5568</Color>
<Color x:Key="BrandInputBackground">#fbfdff</Color>
<Color x:Key="BrandInputBorder">#dde5ee</Color>
<Color x:Key="BrandSurface">#FFFFFF</Color>
<Color x:Key="BrandCardBorder">#eef2f7</Color>
```
`BrandStyles.xaml` (nuevo, mergeado en `App.xaml`): `BrandEntry` (borde `BrandInputBorder`, radio 10), `BrandPrimaryButton` (gradiente naranja CTA `#E8952B→#C97A18`, radio 999, texto blanco).

### Decisión 12 — Ícono y splash

Ediciones en `.csproj` (reemplazar las dos líneas actuales):

```xml
<MauiIcon Include="Resources\AppIcon\logo.png" />
<MauiSplashScreen Include="Resources\Splash\logo.png" Color="#0F4C81" BaseSize="128,128" />
```
- `MauiIcon`: PNG, **sin** `ForegroundFile` (full-bleed) y sin `Color`.
- `MauiSplashScreen`: fondo sólido `#0F4C81` + logo `BaseSize="128,128"`.
- Copiar `logo.png` desde `C:\Git\InsigniaConsultoresAxaPaginaWeb\imagenes\marca\logo.png` a `Resources/AppIcon/`, `Resources/Splash/` y `Resources/Images/` (este último para el logo in-app; `MauiImage Include="Resources\Images\*"` ya lo globea).
- **Stock `appicon.svg`/`appiconfg.svg`/`splash.svg`** y `dotnet_bot.png`: se **eliminan** junto con `MainPage.xaml(.cs)` (aprobado — no se usarán); se quita también la línea `<MauiImage Update="Resources\Images\dotnet_bot.png" ... />` del `.csproj`.

### Decisión 13 — Paquetes NuGet

| Paquete | Versión | Nota |
|---------|---------|------|
| `sqlite-net-pcl` | `1.9.172` | Último estable; netstandard2.0 → compatible con net10.0. Trae `SQLitePCLRaw.bundle_green` transitivamente |

**MVVM toolkit**: **no** se agrega `CommunityToolkit.Mvvm`. Con solo 2 ViewModels, `ViewModelBase` a mano (10 líneas de `INotifyPropertyChanged`) es más simple, sin generadores de código ni dependencia nueva. Se puede adoptar el toolkit después si crece la cantidad de VMs. `SQLitePCLRaw.bundle_green` se añade explícitamente solo si aparece un error de `Batteries_V2.Init()` en runtime.

### Decisión 14 — Manifiesto de archivos

**Nuevos (21 + 3 recursos):**

| Archivo | Propósito |
|---------|-----------|
| `Models/Requests/TokenRequest.cs` | Datos de login + body url-encoded |
| `Models/Responses/TokenResponse.cs` | DTO token con `JsonPropertyName` |
| `Models/Responses/LoginResult.cs` | `AuthErrorKind` + `LoginResult` + `AuthMessages` |
| `Models/SessionModel.cs` | Modelo de sesión persistida |
| `Services/Api/AuthConfig.cs` | Constantes (URLs, margen) |
| `Services/Api/TokenExpiry.cs` | Cálculo puro de expiración |
| `Services/Api/IAuthService.cs` | Contrato auth |
| `Services/Api/AuthService.cs` | POST `/Token` + mapeo de errores |
| `Services/Storage/ITokenStore.cs` | Contrato persistencia |
| `Services/Storage/TokenStore.cs` | Persistencia híbrida + fallback a SQLite (keystore no disponible) |
| `Services/Storage/SessionDatabase.cs` | SQLite lazy + `SessionRow` |
| `Validations/LoginValidator.cs` | `ValidationResult` + `LoginValidator` |
| `ViewModels/ViewModelBase.cs` | Base `INotifyPropertyChanged` |
| `ViewModels/LoginViewModel.cs` | Estado del login + `LoginCommand` |
| `ViewModels/HomeViewModel.cs` | `Username` del flyout |
| `Views/LoginPage.xaml` | UI de login |
| `Views/LoginPage.xaml.cs` | Code-behind login |
| `Views/HomePage.xaml` | Placeholder en blanco |
| `Views/HomePage.xaml.cs` | Code-behind home |
| `Resources/Styles/BrandStyles.xaml` | Estilos de marca (CTA/Entry) |
| `Resources/AppIcon/logo.png`, `Resources/Splash/logo.png`, `Resources/Images/logo.png` | Assets de marca (copiados) |

**Modificados (7):**

| Archivo | Cambio |
|---------|--------|
| `InsigniaConsultoresApp.csproj` | `TargetFrameworks` → `net10.0-android` (quitar ios/maccatalyst/windows, `WindowsPackageType` y condiciones no-android); `MauiIcon`/`MauiSplashScreen` → `logo.png`; quitar `MauiImage Update` de `dotnet_bot.png`; paquete `sqlite-net-pcl` |
| `MauiProgram.cs` | Registros DI |
| `App.xaml` | Merge `BrandStyles.xaml` |
| `App.xaml.cs` | Resolver `AppShell` vía DI en `CreateWindow` |
| `AppShell.xaml` | Rutas `login`/`home`, FlyoutHeader, `Cerrar sesión` |
| `AppShell.xaml.cs` | Ruteo de arranque + logout |
| `Resources/Styles/Colors.xaml` | Paleta de marca + repoint `Primary`/`PrimaryDark` |

**Eliminados (6):**

| Archivo | Motivo |
|---------|--------|
| `MainPage.xaml` | Plantilla reemplazada por `Views/LoginPage`/`Views/HomePage` |
| `MainPage.xaml.cs` | Idem |
| `Resources/AppIcon/appicon.svg` | Reemplazado por `logo.png` |
| `Resources/AppIcon/appiconfg.svg` | Reemplazado por `logo.png` |
| `Resources/Splash/splash.svg` | Reemplazado por `logo.png` |
| `Resources/Images/dotnet_bot.png` | Plantilla (también se quita su `MauiImage Update`) |

### Decisión 15 — Plan de verificación

**Build (no hay tests):**
```
dotnet build -f net10.0-android
```

**Prueba manual (ordenada):**
1. Arranque en frío sin sesión → se ve `LoginPage`, sin flyout.
2. Usuario vacío → "Ingresa tu usuario", sin request, botón re-habilitado.
3. Contraseña vacía → "Ingresa tu contraseña".
4. Credenciales inválidas → "Usuario o contraseña incorrectos", spinner se detiene, botón re-habilitado.
5. Modo avión/sin red → "No se pudo conectar con el servidor".
6. Login válido → spinner dentro del botón y botón deshabilitado durante la carga; luego Home con flyout; **back NO regresa al login**.
7. Flyout muestra logo, usuario, "Inicio" y "Cerrar sesión".
8. Cerrar y reabrir (token válido) → auto-login directo a Home.
9. Expiración: `expires_at` en el pasado → al reabrir se muestra login.
10. "Cerrar sesión" → vuelve al login, sin flyout; SecureStorage + SQLite limpios.
11. (Android) Instalar → ícono y splash muestran el logo sin artefactos de la plantilla.

### Decisión 16 — Plataforma objetivo: Android only

**Elección**: la app se publica **solo en Android**. En `InsigniaConsultoresApp.csproj` se deja un único `TargetFrameworks` y se quitan las líneas de iOS/macCatalyst/Windows, `WindowsPackageType` y las condiciones `SupportedOSPlatformVersion`/`TargetPlatformMinVersion` no-Android:

```xml
<TargetFrameworks>net10.0-android</TargetFrameworks>
<SupportedOSPlatformVersion>21.0</SupportedOSPlatformVersion>
```

Con esto desaparecen las consideraciones de Windows/iOS: el `try/catch` de `SecureStorage` se mantiene únicamente como robustez ante un Keystore de dispositivo no disponible, y la verificación es solo `dotnet build -f net10.0-android`.

---

## Data Flow

```
LoginPage ── LoginViewModel.LoginCommand
   ├─ LoginValidator.Validate (sync) ── inválido → ErrorMessage, return
   └─ IsBusy=true
        └─ AuthService.LoginAsync(TokenRequest)
             └─ HttpClient.PostAsync("/Token", FormUrlEncodedContent)
                  ├─ 200 → TokenResponse → TokenExpiry.CalculateUtc → SessionModel
                  │        └─ TokenStore.SaveSessionAsync (SecureStorage + SQLite)
                  │             └─ LoginResult.Ok → GoToAsync("//home")
                  └─ 400/401/5xx/timeout/malformado → LoginResult.Fail → ErrorMessage, IsBusy=false

Arranque: App.CreateWindow ──► AppShell.Loaded ──► ITokenStore.HasValidSessionAsync()
   ├─ válido   → GoToAsync("//home")
   └─ inválido → permanece "//login"

Logout: AppShell "Cerrar sesión" → ITokenStore.ClearAsync() → GoToAsync("//login")
```

## Interfaces / Contracts

```csharp
public interface IAuthService
{
    Task<LoginResult> LoginAsync(TokenRequest request);
}

public interface ITokenStore
{
    Task<bool> HasValidSessionAsync();
    Task<SessionModel?> GetSessionAsync();
    Task SaveSessionAsync(SessionModel session);
    Task ClearAsync();
}

public sealed class SessionModel
{
    public string? Username { get; set; }
    public string? TokenType { get; set; }
    public string? AccessToken { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime LastLoginUtc { get; set; }
    public bool IsEncrypted { get; set; }
}
```

## Migration / Rollout

No se requiere migración de datos. Revertir = `git checkout` de los archivos modificados y borrar las carpetas nuevas (`Models`, `Services`, `Validations`, `ViewModels`, `Views`, `BrandStyles.xaml`); la DI y las capas no rompen dependencias externas.

## Open Questions

- [ ] ¿Renombrar `ApplicationId`/`ApplicationTitle` ahora o en un cambio separado? (fuera de alcance, documentado en proposal)
- [ ] ¿Validar el token contra un endpoint de perfil, o solo por expiración local? (este cambio: solo expiración local)
- [ ] Recorte del adaptive icon Android (~10%) con logo full-bleed — aceptado como riesgo documentado; validar visualmente en el paso 11.
