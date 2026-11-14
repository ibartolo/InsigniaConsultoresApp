# Design: Módulo de campañas WhatsApp

## Technical Approach

MVVM simple + DI en `MauiProgram`, reutilizando la arquitectura existente (sin CommunityToolkit, sin repositorios/mediadores). Un nuevo `ICampaignService` consume `api/Campana/*` con el `HttpClient` singleton ya registrado, inyectando el bearer token por request. Tres ViewModels + tres `ContentPage`, un helper puro de teléfono, y rutas Shell. El manejo de 401 se centraliza en una clase base de ViewModels de campañas. Se reutilizan estilos de marca (`BrandButton`, `BrandTitle`, `BrandLabel`, `BrandEntry`, claves de `Colors.xaml`).

## Architecture Decisions

### Decisión 1 — Auth en llamadas API: header por request (NO DelegatingHandler)

**Choice**: `CampaignService` lee el token desde `ITokenStore.GetSessionAsync()` y lo asigna en cada `HttpRequestMessage` (`Authorization: Bearer {token}`), sin modificar el `HttpClient` singleton.

**Alternativas**: (a) `DelegatingHandler` que inyecta el header globalmente; (b) `HttpClient` dedicado con `DefaultRequestHeaders`.

**Rationale**: Un `DelegatingHandler` afectaría a TODAS las llamadas del `HttpClient` compartido, incluido `POST /Token` de `AuthService` (que NO debe llevar bearer) y complicaría el wiring de DI del singleton. Releer el token en cada request garantiza que siempre sea el vigente (p. ej. tras re-login). No rompe `AuthService` (intacto). Coincide con el patrón de `AuthService` (construye requests inline).

```csharp
private async Task<HttpRequestMessage> CreateRequestAsync(HttpMethod method, string path)
{
    var session = await _tokenStore.GetSessionAsync();
    var req = new HttpRequestMessage(method, path);
    if (!string.IsNullOrWhiteSpace(session?.AccessToken))
        req.Headers.Authorization = new("Bearer", session.AccessToken);
    return req;
}
```

### Decisión 2 — 401 (CW-R8): resultado discriminado + base común (no excepción, no duplicado)

**Choice**: `CampaignService` devuelve `CampaignResult<T>` (discriminado, espejo de `LoginResult`; sin excepciones para errores esperados). Nueva `CampaignViewModelBase : ViewModelBase` con `ExecuteAsync<T>` que detecta `Unauthorized`, limpia sesión y navega `//login` UNA sola vez.

**Alternativas**: (a) `SessionExpiredException` + catch en base; (b) `CampaignResult<T>` sin base (cada VM maneja 401).

**Rationale**: El codebase ya usa resultados discriminados (`AuthService`/`LoginResult`), no excepciones para flujos esperados. La base evita duplicar la lógica de 401 en los 3 VMs; el servicio permanece puro (sin navegación). El efecto lateral (clear + `//login`) vive en exactamente un método.

```csharp
public abstract class CampaignViewModelBase : ViewModelBase
{
    private readonly ITokenStore _tokenStore;
    protected CampaignViewModelBase(ITokenStore ts) => _tokenStore = ts;

    protected async Task<CampaignResult<T>> ExecuteAsync<T>(Func<Task<CampaignResult<T>>> op)
    {
        var r = await op();
        if (r.Error == CampaignErrorKind.Unauthorized)
        {
            await _tokenStore.ClearAsync();
            await Shell.Current.GoToAsync("//login");
        }
        return r;
    }
}
```

### Decisión 3 — DTOs (`Models/Responses`)

**Choice**: DTOs inmutables con `JsonPropertyName` camelCase. `createdDt` ausente → `DateTime? CreatedDt` queda `null`; el valor mostrado se resuelve en el VM como `CreatedDt ?? FechaInicio`.

```csharp
public sealed class Campana
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("nombre")] public string? Nombre { get; set; }
    [JsonPropertyName("descripcion")] public string? Descripcion { get; set; }
    [JsonPropertyName("asunto")] public string? Asunto { get; set; }
    [JsonPropertyName("fechaInicio")] public DateTime? FechaInicio { get; set; }
    [JsonPropertyName("fechaFin")] public DateTime? FechaFin { get; set; }
    [JsonPropertyName("estatus")] public string? Estatus { get; set; }
    [JsonPropertyName("canal")] public int Canal { get; set; }
    [JsonPropertyName("preparada")] public bool Preparada { get; set; }
    [JsonPropertyName("total")] public int Total { get; set; }
    [JsonPropertyName("enviados")] public int Enviados { get; set; }
    [JsonPropertyName("pendientes")] public int Pendientes { get; set; }
    [JsonPropertyName("excluidos")] public int Excluidos { get; set; }
    [JsonPropertyName("errores")] public int Errores { get; set; }
    [JsonPropertyName("porcentaje")] public decimal Porcentaje { get; set; }
    [JsonPropertyName("createdDt")] public DateTime? CreatedDt { get; set; }
}

public sealed class CampanaWhatsAppDestinatario
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("campanaID")] public int CampanaID { get; set; }
    [JsonPropertyName("clienteID")] public int ClienteID { get; set; }
    [JsonPropertyName("nombre")] public string? Nombre { get; set; }
    [JsonPropertyName("telefono")] public string? Telefono { get; set; }
    [JsonPropertyName("estado")] public string? Estado { get; set; }
    [JsonPropertyName("fechaEnvio")] public DateTime? FechaEnvio { get; set; }
}

public sealed class ModelResponseDto<T>
{
    [JsonPropertyName("isSuccess")] public bool IsSuccess { get; set; }
    [JsonPropertyName("message")] public string? Message { get; set; }
    [JsonPropertyName("enum")] public T? Enum { get; set; }
}
```

`estatus` se mapea a español con `CampaignStatus.GetLabel(string?)` (switch con fallback al valor crudo). Deserialización: `JsonSerializerOptions { PropertyNameCaseInsensitive = true }`, compartido en `CampaignService`.

### Decisión 4 — Superficie del servicio

**Choice**: filtro `Canal == 2` DENTRO del servicio (el método ya se llama `GetWhatsAppCampaignsAsync`), VM tonto.

```csharp
public interface ICampaignService
{
    Task<CampaignResult<List<Campana>>> GetWhatsAppCampaignsAsync(CancellationToken ct = default);
    Task<CampaignResult<Campana>> GetCampaignAsync(int id, CancellationToken ct = default);
    Task<CampaignResult> PrepareRecipientsAsync(int id, CancellationToken ct = default);
    Task<CampaignResult<List<CampanaWhatsAppDestinatario>>> GetRecipientsAsync(int id, CancellationToken ct = default);
    Task<CampaignResult> MarkSentAsync(int destinatarioId, CancellationToken ct = default);
    Task<CampaignResult> MarkPendingAsync(int destinatarioId, CancellationToken ct = default);
}
```

Rutas (relativas al `BaseAddress`): `api/Campana/List`, `api/Campana/{id}`, `api/Campana/{id}/WhatsApp/Preparar`, `api/Campana/{id}/WhatsApp/Destinatarios`, `api/Campana/WhatsApp/Enviado/{id}`, `api/Campana/WhatsApp/Pendiente/{id}`. El servicio clasifica: `IsSuccessStatusCode` + `IsSuccess==true` → Ok; HTTP 401 → `Unauthorized`; timeout/`HttpRequestException`/`TaskCanceledException` → `Network`; ≥500 → `Server`; resto → `Business` (usa `Message` del backend) o `Malformed` si no deserializa.

### Decisión 5 — Phone helper puro

**Choice**: `Validations/PhoneHelper.cs` estático y puro.

```csharp
public static class PhoneHelper
{
    public static bool TryNormalizeTo10Digits(string? raw, out string ten)
    {
        ten = "";
        if (string.IsNullOrWhiteSpace(raw)) return false;
        var d = new string(raw.Where(char.IsDigit).ToArray());
        if (d.Length < 10) return false;
        ten = d.Length > 10 ? d[^10..] : d;
        return true;
    }

    public static string BuildWhatsAppUri(string tenDigits, string? message)
    {
        var q = string.IsNullOrEmpty(message) ? "" : $"?text={Uri.EscapeDataString(message)}";
        return $"https://wa.me/52{tenDigits}{q}";
    }
}
```

Edge cases: espacios/`+52`/paréntesis/guias → se quitan (no dígitos); `+52 33 1234 5678` → últimos 10 = `3312345678`; `044...`/cero inicial → se caen por "últimos 10"; `<10` → `false` (no enviable). `BuildWhatsAppUri` asume entrada ya validada de 10 dígitos (solo se llama tras `TryNormalize…==true`).

### Decisión 6 — Estado por fila

**Choice**: `RecipientItemViewModel : ViewModelBase` envuelve el DTO y recibe callbacks del padre. El padre (`RecipientsViewModel`) ejecuta el envío/deshacer real (servicio + optimista + rollback); el ítem solo expone estado y comandos que delegan.

```csharp
public sealed class RecipientItemViewModel : ViewModelBase
{
    public int Id { get; }
    public string Nombre { get; }
    public string Telefono { get; }
    public bool IsSent { get; private set; }
    public bool IsBusy { get; private set; }
    public string? FechaEnvio { get; private set; }
    public bool HasInvalidPhone { get; }
    public bool CanSend => !IsBusy && !IsSent && !HasInvalidPhone;
    public ICommand SendCommand { get; }
    public ICommand UndoCommand { get; }
    // ctor(CampanaWhatsAppDestinatario dto, Func<RecipientItemViewModel,Task> onSend, Func<RecipientItemViewModel,Task> onUndo)
    // SendCommand = new Command(async () => await _onSend(this), () => CanSend);
}
```

La `CollectionView` se enlaza a `ObservableCollection<RecipientItemViewModel>`; cada fila usa `IsEnabled="{Binding CanSend}"` y `IsVisible="{Binding IsSent}"` para Enviar/Deshacer.

### Decisión 7 — Búsqueda local (CW-R4)

**Choice**: mantener `List<RecipientItemViewModel> _all` + exponer `ObservableCollection<RecipientItemViewModel> Filtered`; al cambiar `SearchText` se reconstruye `Filtered` (LINQ, `Contains` case-insensitive sobre `Nombre` o `Telefono`). **Sin debounce** (filtro en memoria sobre listas moderadas; `CollectionView` virtualiza). Limpiar → restaurar lista completa.

Se distingue vacío de datos (`"No hay destinatarios"`) de filtro sin resultado (`"Sin resultados para «{texto}»"`) con `HasItems`/`HasResults`.

### Decisión 8 — Navegación y rutas

**Choice**: rutas absolutas para top-level; rutas globales registradas para detalle/destinatarios (push relativo). Id de campaña vía **query property** (`[QueryProperty]`), pasando SOLO `campaignId` (primitivo; evita el gotcha del conversor de bool de `QueryProperty`).

```xml
<!-- AppShell.xaml: nuevo FlyoutItem tras "Inicio" -->
<FlyoutItem Title="Campañas" Route="campaigns" x:Name="CampaignsItem">
    <ShellContent ContentTemplate="{DataTemplate views:CampaignsPage}" />
</FlyoutItem>
```

```csharp
// AppShell.xaml.cs constructor
Routing.RegisterRoute("campaignDetail", typeof(CampaignDetailPage));
Routing.RegisterRoute("campaignRecipients", typeof(RecipientsPage));
// OnNavigated: visibilidad del ítem
CampaignsItem.FlyoutItemIsVisible = location.Contains("home", StringComparison.Ordinal)
                                 || location.Contains("campaign", StringComparison.Ordinal);
```

Lista→detalle: `GoToAsync($"campaignDetail?campaignId={id}")`; detalle→destinatarios: `GoToAsync($"campaignRecipients?campaignId={id}")`. `RecipientsViewModel` relee `GET api/Campana/{id}` para obtener `Preparada` (autocontenido e idempotente). Back: destinatarios→detalle→lista (pop), nunca a login (login es raíz oculta aparte).

### Decisión 9 — UX de carga/vacío/error

| Pantalla | Cargando | Vacío | Error |
|---|---|---|---|
| Lista | `ActivityIndicator` + "Cargando campañas…" | "No hay campañas de WhatsApp" | mensaje es + `BrandPrimaryButton` "Reintentar" (`RetryCommand`) |
| Detalle | spinner | — | mensaje es + "Reintentar" |
| Destinatarios | spinner "Preparando…"/"Cargando destinatarios…" | "No hay destinatarios" / "Sin resultados para «{texto}»" | mensaje es + "Reintentar" |

`RefreshView` (SHOULD CW-R1) en la lista. Mensajes es centralizados en `CampaignMessages` (espejo de `AuthMessages`). CTA "Comenzar campaña" usa `BrandButton` (naranja); títulos `BrandTitle`; texto `BrandLabel`; búsqueda con `SearchBar` (ya estilado en `Styles.xaml`).

### Decisión 10 — Idempotencia de `Preparar`

**Choice**: flujo de carga de destinatarios = (1) `GET api/Campana/{id}` → lee `Preparada`; (2) si `Preparada == false` → `POST Preparar`; (3) `GET Destinatarios`. Si `Preparada == true` → solo (3). `Preparar` no se repite sobre campaña ya preparada.

Si (2) ok pero (3) falla → error + Reintentar; el reintento relee `Preparada` (ahora `true` en servidor) y salta (2). Si (2) falla → error es + Reintentar, sin tocar (3).

### Decisión 11 — Deep link (CW-R6)

**Choice**: `Launcher.Default.TryOpenAsync(uri)` (devuelve `bool`, no lanza) envuelto en try/catch. Orden: **lanzar → esperar → marcar optimista → rollback si falla**.

```csharp
var canOpen = await Launcher.Default.TryOpenAsync(uri);   // + try/catch defensivo
if (!canOpen) { /* "No se pudo abrir WhatsApp" ; NO marcar */ return; }
item.MarkSentOptimistic(DateTime.Now);                    // IsSent=true, FechaEnvio=now
var r = await _service.MarkSentAsync(item.Id);            // vía ExecuteAsync (401 central)
if (!r.Success) { item.RevertToPending(); /* mostrar r.Message */ }
```

Rollback de Enviar = volver a Pendiente; rollback de Deshacer = restaurar Enviado + `FechaEnvio` previo. `IsBusy` protege doble toque.

## Data Flow

```
Lista:       CampaignsViewModel → ICampaignService.GetWhatsAppCampaignsAsync → filtra Canal==2 → CollectionView
Detalle:     CampaignDetailViewModel → GetCampaignAsync(id) → //campaigns list
Recipients:  RecipientsViewModel → GetCampaignAsync(id) → [Preparar si !Preparada] → GetRecipientsAsync(id) → item VMs
Enviar:      item.SendCommand → padre: PhoneHelper.BuildWhatsAppUri → Launcher.TryOpenAsync → MarkSentAsync (optimista→rollback)
401 (cualquier call):  CampaignViewModelBase.ExecuteAsync → ClearAsync + GoToAsync("//login")
```

## File Changes

| File | Acción | Propósito |
|---|---|---|
| `Models/Responses/Campana.cs` | Crear | DTO de campaña + `CampaignStatus.GetLabel` |
| `Models/Responses/CampanaWhatsAppDestinatario.cs` | Crear | DTO de destinatario |
| `Models/Responses/ModelResponseDto.cs` | Crear | Envoltura genérica de respuesta |
| `Models/Responses/CampaignResult.cs` | Crear | `CampaignErrorKind` + `CampaignResult`/`CampaignResult<T>` + `CampaignMessages` |
| `Services/Api/ICampaignService.cs` | Crear | Contrato de endpoints |
| `Services/Api/CampaignService.cs` | Crear | Implementación HTTP + filtro Canal==2 |
| `Validations/PhoneHelper.cs` | Crear | Normalización a 10 dígitos + URI wa.me |
| `ViewModels/CampaignViewModelBase.cs` | Crear | Base con `ExecuteAsync` (401 central) |
| `ViewModels/CampaignsViewModel.cs` | Crear | Lista de campañas |
| `ViewModels/CampaignDetailViewModel.cs` | Crear | Detalle + navegar a destinatarios |
| `ViewModels/RecipientsViewModel.cs` | Crear | Preparar/leer/enviar/deshacer + búsqueda |
| `ViewModels/RecipientItemViewModel.cs` | Crear | Estado por fila + comandos |
| `Views/CampaignsPage.xaml(.cs)` | Crear | Pantalla lista |
| `Views/CampaignDetailPage.xaml(.cs)` | Crear | Pantalla detalle |
| `Views/RecipientsPage.xaml(.cs)` | Crear | Pantalla destinatarios |
| `MauiProgram.cs` | Modificar | Registrar `ICampaignService`, VMs y páginas |
| `AppShell.xaml` | Modificar | Ítem flyout "Campañas" |
| `AppShell.xaml.cs` | Modificar | `Routing.RegisterRoute` + visibilidad del ítem |

## Interfaces / Contracts

Ver Decisiones 3–4 (DTOs y `ICampaignService`). `CampaignResult<T>`:

```csharp
public enum CampaignErrorKind { None, Unauthorized, Network, Server, Business, Malformed }

public sealed class CampaignResult<T>
{
    public bool Success { get; private init; }
    public T? Data { get; private init; }
    public CampaignErrorKind Error { get; private init; }
    public string Message { get; private init; } = "";
    public static CampaignResult<T> Ok(T d) => new() { Success = true, Data = d };
    public static CampaignResult<T> Fail(CampaignErrorKind e, string m) => new() { Error = e, Message = m };
}
// Endpoints sin payload usan CampaignResult<object?>
```

## Testing Strategy

| Capa | Qué | Cómo |
|---|---|---|
| Build | Compilación Android | `dotnet build -f net10.0-android` → SUCCESS |
| Manual | Flujo completo | Script ordenado abajo |

**Script manual (ordenado)**: (1) login válido → Home; (2) flyout muestra Inicio/Campañas/Cerrar sesión; (3) "Campañas" → lista solo Canal==2, Nombre + `dd/MM/yyyy`; (4) pull-to-refresh; (5) abrir detalle → campos + Estatus español; (6) "Comenzar campaña" → Preparar → lista Nombre/Teléfono/Estado; (7) búsqueda por nombre/teléfono, sin match → "Sin resultados…", limpiar → lista completa; (8) Enviar válido → wa.me/52{10}?text=… y fila Enviado; (9) teléfono <10 dígitos → Enviar deshabilitado + nota; (10) Deshacer → Pendiente, `FechaEnvio` limpio; (11) token expirado → 401 limpia sesión + login; (12) modo avión → error es + Reintentar; (13) sin WhatsApp → mensaje es, no marca Enviado.

## Migration / Rollout

No migration required. Rollback: `git checkout` de `MauiProgram.cs`/`AppShell.*` y borrar archivos nuevos (sin dependencias previas).

## Open Questions

- [ ] Nombre exacto del campo de payload de la envoltura (`enum` vs `response`) — confirmar contra respuesta real en sdd-apply/verify.
- [ ] Tipo exacto de `estatus` (string vs int) para el mapeo a español — el diseño usa `string?` con fallback al valor crudo.
