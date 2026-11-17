# Apply Progress: indicadores-home

- **Fecha**: 2026-11-16
- **Modo**: Standard (`strict_tdd: false`, sin test runner)
- **Estado**: completo — build `net10.0-android` OK (0 errores)

## Tareas

- [x] 1.1 DTO `IndicadoresPeriodo`
- [x] 1.2 `IReportsService`
- [x] 1.3 `ReportsService`
- [x] 1.4 Registro DI (singleton)
- [x] 2.1 `HomeViewModel` extendido (hereda `CampaignViewModelBase`)
- [x] 2.2 `HomePage.xaml` + `HomePage.xaml.cs`
- [x] 2.3 Estilos KPI (`KpiCard`, `KpiValue`, `KpiLabel`)
- [x] 3.1 Build OK

## Archivos

| Archivo | Acción | Detalle |
|---------|--------|---------|
| `Models/Responses/IndicadoresPeriodo.cs` | Creado | DTO con 5 `int` + `FechaInicio`/`FechaFin`, `[JsonPropertyName]` |
| `Services/Api/IReportsService.cs` | Creado | Contrato `GetIndicadoresAsync(desde, hasta, ct)` |
| `Services/Api/ReportsService.cs` | Creado | `GET api/Reports/Indicadores`, bearer por request, mapeo de errores `CampaignResult<T>` |
| `ViewModels/HomeViewModel.cs` | Modificado | Rango de fechas, indicadores, periodo, comandos, validación, 401, recarga |
| `Views/HomePage.xaml` | Modificado | RefreshView + ScrollView con KPIs y estados |
| `Views/HomePage.xaml.cs` | Modificado | Carga única con guard `_loaded` |
| `Resources/Styles/BrandStyles.xaml` | Modificado | Estilos `KpiCard`, `KpiValue`, `KpiLabel` |
| `MauiProgram.cs` | Modificado | `AddSingleton<IReportsService, ReportsService>()` |

## Desviaciones

- `MaximumDate` de "Hasta" se resuelve vía propiedad `Hoy` en el ViewModel en lugar de `x:Static` (evita ambigüedad de ensamblado de `System.DateTime` en XAML source-gen).
- `HomeViewModel` hereda de `CampaignViewModelBase` (reutiliza `IsBusy`/`ErrorMessage`/`HasError`/`RunAsync`/`HandleUnauthorizedAsync`) en lugar de duplicarlos, siguiendo la propuesta ("heredar el manejo 401 de `CampaignViewModelBase`").
- No se modifica `inicio-y-navegacion` en specs principales (eso corresponde a `sdd-archive`).

## Riesgos

- El endpoint devuelve conteos globales (sin filtro por compañía) — aceptado en spec.
- `FechaInicio`/`FechaFin` ausentes caen a `DateTime.MinValue`; el VM cae al rango seleccionado en `PeriodoTexto`.

## Build

`dotnet build InsigniaConsultoresApp\InsigniaConsultoresApp.csproj -f net10.0-android -v q --nologo` → **0 errores**, 2 warnings NU1903 (esperados).
