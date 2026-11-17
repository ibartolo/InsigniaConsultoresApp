# Tasks: indicadores-home

Checklist de implementación ejecutada por `sdd-apply`.

## Phase 1: Infraestructura (DTO + servicio)

- [x] 1.1 Crear `Models/Responses/IndicadoresPeriodo.cs` (5 conteos `int` + `FechaInicio`/`FechaFin` `DateTime`, con `[JsonPropertyName]`, deserialización tolerante sin fallar ante campos faltantes).
- [x] 1.2 Crear `Services/Api/IReportsService.cs` con el contrato `GetIndicadoresAsync`.
- [x] 1.3 Crear `Services/Api/ReportsService.cs` (Bearer por request desde `ITokenStore`, `PropertyNameCaseInsensitive`, mapeo de errores idéntico a `CampaignService`, reutiliza `CampaignResult<T>`).
- [x] 1.4 Registrar `IReportsService` → `ReportsService` como singleton en `MauiProgram.cs`.

## Phase 2: Estado y presentación

- [x] 2.1 Extender `ViewModels/HomeViewModel.cs`: `Desde`/`Hasta` (defaults primer día del mes / hoy), `Indicadores`, `IsRefreshing`, `PeriodoTexto`, valores KPI formateados es-MX, `LoadCommand`/`RefreshCommand`, `LoadAsync`, validación de rango antes de llamar, 401 one-shot (heredado de `CampaignViewModelBase`) y recarga al cambiar fechas con guardas.
- [x] 2.2 Reemplazar el placeholder de `Views/HomePage.xaml` (RefreshView + ScrollView: título, periodo, 2 DatePickers, 5 tarjetas KPI en grid de 2 columnas, spinner y error/retry) y actualizar `HomePage.xaml.cs` (carga única con `_loaded`).
- [x] 2.3 Agregar estilos `KpiCard`, `KpiValue`, `KpiLabel` en `Resources/Styles/BrandStyles.xaml`.

## Phase 3: Verificación

- [x] 3.1 `dotnet build -f net10.0-android` → 0 errores (solo warnings NU1903 esperados).
