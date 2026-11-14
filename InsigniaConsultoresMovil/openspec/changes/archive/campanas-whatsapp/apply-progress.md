# Apply Progress: campanas-whatsapp

> Lote (2026-11-14) — Etapas 5, 6 y 7 (Vistas, Navegación y DI).
> Modo: **Standard** (strict_tdd = false, sin proyecto de tests). Modo de artefactos: **openspec**.
> Etapas 1–4 completadas en lotes previos (compiladas y verificadas por el orquestador).

## Resultado del build

`dotnet build InsigniaConsultoresApp\InsigniaConsultoresApp.csproj -f net10.0-android -v q --nologo` → **SUCCESS** (0 errores, 2 advertencias NU1903).

Advertencias (no bloqueantes, upstream sin fix):
- **NU1903**: `SQLitePCLRaw.lib.e_sqlite3.android` 2.1.11, advisory GHSA-2m69-gcr7-jv3q (CVE-2025-6965). "Patched versions: None". No se suprime; al existir bundle con SQLite ≥ 3.50.2, subir `SQLitePCLRaw.bundle_green`.

## Tareas completadas

### Etapa 5 — Vistas (3 páginas + 1 converter)

- [x] **5.1** `Views/CampaignsPage.xaml(.cs)`:
  - `RefreshView` (IsRefreshing OneWay + `Command=RefreshCommand`) + `CollectionView` (`SelectionMode=Single`, `SelectionChanged` → `OpenCommand` en code-behind, limpia `SelectedItem`).
  - Item template: `Nombre` (bold, `BrandPrimary`) + `FechaCreacion` `dd/MM/yyyy` vía `StringFormat`.
  - `ActivityIndicator` (`IsBusy`), error `Label` (`HasError`) + botón "Reintentar" (`LoadCommand`, `BrandPrimaryButton`), vacío `Label` (`IsEmpty`) "No hay campañas de WhatsApp".
  - `OnAppearing` → `LoadAsync()` una vez (guard `_loaded`).

- [x] **5.2** `Views/CampaignDetailPage.xaml(.cs)`:
  - `[QueryProperty(nameof(CampaignId), "campaignId")]`; propiedad `string? CampaignId` que parsea a int y asigna `ViewModel.CampaignId`.
  - Campos `Nombre` (título), `Descripcion`, `Asunto`, `EstatusTexto`, `FechaInicio`, `FechaFin`, `Total`, `Enviados`, `Pendientes` (labels de solo lectura).
  - Botón "Comenzar campaña" (`StartCampaignCommand`, `BrandButton`) — habilitado por `CanExecute` del comando (no se enlaza `IsEnabled`).
  - Spinner (`IsBusy`) + error `Label` (`HasError`). `OnAppearing` → `LoadAsync()`.

- [x] **5.3** `Views/RecipientsPage.xaml(.cs)`:
  - `SearchBar` arriba (`Placeholder="Buscar por nombre o teléfono"`, `Text` two-way a `SearchText`).
  - `CollectionView` por fila: `Nombre` (bold), `Telefono`, badge `EstadoTexto`, `FechaEnvioTexto` (visible con `IsSent`), `InvalidPhoneHint` (visible con `HasInvalidPhone`).
  - Botones por fila: "Enviar" (`SendCommand`, `IsEnabled=CanSend`, `IsVisible` con `InvertedBool`), "Deshacer" (`UndoCommand`, `IsVisible=IsSent`), `ActivityIndicator` (`IsBusy`).
  - Estados de página: spinner (`IsBusy`), error + "Reintentar" (`LoadCommand`), `EmptyMessage` (`IsEmpty`).
  - `OnAppearing` → `LoadAsync()`.

- [x] **Converter** `Converters/InvertedBoolConverter.cs`: `IValueConverter` true↔false. Registrado en `App.xaml` como `{x:Key InvertedBool}`.

### Etapa 6 — Navegación

- [x] **6.1** `AppShell.xaml`: `FlyoutItem Title="Campañas" Route="campaigns"` tras "Inicio" (un solo `ShellContent`).
- [x] **6.2** `AppShell.xaml.cs`: en el constructor:
  - `Routing.RegisterRoute("campaign-detail", typeof(CampaignDetailPage));`
  - `Routing.RegisterRoute("campaign-recipients", typeof(RecipientsPage));`
- [x] **6.3** Fix de las dos cadenas de navegación (un solo segmento, sin ambigüedad de ruta):
  - `CampaignsViewModel`: `GoToAsync($"campaign-detail?campaignId={c.Id}")`
  - `CampaignDetailViewModel`: `GoToAsync($"campaign-recipients?campaignId={CampaignId}")`

### Etapa 7 — DI wiring

- [x] **7.1** `MauiProgram.cs`:
  - `ICampaignService` → `CampaignService` (singleton).
  - `CampaignsViewModel`, `CampaignDetailViewModel`, `RecipientsViewModel` (transient).
  - `CampaignsPage`, `CampaignDetailPage`, `RecipientsPage` (transient).
  - Registros existentes intactos.

## Archivos

### Creados
| Archivo | Nota |
|---------|------|
| `Converters/InvertedBoolConverter.cs` | `IValueConverter` true↔false |
| `Views/CampaignsPage.xaml` | Lista con RefreshView + estados |
| `Views/CampaignsPage.xaml.cs` | Code-behind + selección + guard `_loaded` |
| `Views/CampaignDetailPage.xaml` | Detalle de campaña |
| `Views/CampaignDetailPage.xaml.cs` | `QueryProperty` + `LoadAsync` |
| `Views/RecipientsPage.xaml` | SearchBar + lista por fila |
| `Views/RecipientsPage.xaml.cs` | `QueryProperty` + `LoadAsync` |

### Modificados
| Archivo | Cambio |
|---------|--------|
| `App.xaml` | xmlns `converters` + `InvertedBoolConverter` global (`InvertedBool`) |
| `AppShell.xaml` | `FlyoutItem` "Campañas" (`Route="campaigns"`) tras "Inicio" |
| `AppShell.xaml.cs` | `using ...Views` + `Routing.RegisterRoute` ×2 |
| `ViewModels/CampaignsViewModel.cs` | Ruta `campaigns/detail` → `campaign-detail` |
| `ViewModels/CampaignDetailViewModel.cs` | Ruta `campaigns/recipients` → `campaign-recipients` |
| `MauiProgram.cs` | Registros DI (servicio singleton + 3 VMs + 3 páginas transient) |
| `openspec/changes/campanas-whatsapp/tasks.md` | Marcas `[x]` (1.1–7.1) |

## Rutas registradas (exactas)

- Flyout: `campaigns` (`FlyoutItem Route`)
- Global 1: `campaign-detail` → `CampaignDetailPage`
- Global 2: `campaign-recipients` → `RecipientsPage`

Cadenas de navegación: `campaign-detail?campaignId={id}` y `campaign-recipients?campaignId={id}`.

## Desviaciones del diseño / decisiones

- **D1 — Rutas de un solo segmento con guion** (`campaign-detail`/`campaign-recipients`): el diseño (Decisión 8 / tarea 6.2) proponía `campaignDetail`/`campaignRecipients`; los ViewModels de la etapa 4 usaban `campaigns/detail`/`campaigns/recipients` (multi-segmento, relativo a la ruta `campaigns` del flyout → ambigüedad). El orquestador fijó rutas globales de un solo segmento (`campaign-detail`, `campaign-recipients`) para evitar la ambigüedad; se aplicó y se corrigieron las dos cadenas de los ViewModels.
- **D2 — Sin toggle de visibilidad del ítem "Campañas"** (`FlyoutItemIsVisible`): el diseño (Decisión 8 / tarea 6.2) proponía alternar la visibilidad en `OnNavigated`. El orquestador no lo incluyó en la Etapa 6; la visibilidad queda garantizada por el mecanismo existente del flyout (sin sesión → login con `FlyoutBehavior=Disabled`, así "Campañas" no es alcanzable; con sesión → flyout activo). No se añadió lógica de visibilidad.
- **D3 — Detalle sin botón "Reintentar"**: `CampaignDetailViewModel` no expone `LoadCommand`; el alcance de la Etapa 5.2 no lo pide. El detalle muestra spinner + error (sin reintento); `OnAppearing` recarga. (Lista y destinatarios sí tienen "Reintentar" vía sus `LoadCommand`.)
- **D4 — `IsRefreshing` en modo OneWay**: `CampaignsViewModel.IsRefreshing` tiene setter privado; se enlaza `IsRefreshing="{Binding IsRefreshing, Mode=OneWay}"` + `Command="{Binding RefreshCommand}"` para no escribir el setter privado desde el binding.
- **D5 — Selección vía `SelectionChanged` + `OpenCommand`**: `CollectionView` no tiene `SelectedItem` two-way ni `SelectionChangedCommand` tipado a `Campana`; el code-behind invoca `_viewModel.OpenCommand.Execute(campaign)` y limpia `SelectedItem` para permitir volver a tocar la misma fila.
- **Sin cambios** en login (vistas/VMs), `AuthService`, `AuthConfig`, `TokenStore`, `SessionDatabase` ni specs archivadas (constraint cumplido).

## Restante (siguiente fase)

- Fase 8 (verificación): 8.1 build (verificado en este lote, pendiente de confirmar por `sdd-verify`) y 8.2 prueba manual ordenada en dispositivo.
- Próxima fase recomendada: **sdd-verify**.
