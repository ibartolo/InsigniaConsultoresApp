# Proposal: Indicadores del Home (KPIs)

## Intent

Mostrar en la pantalla Home (post-login) una sección de 5 indicadores (KPIs) del negocio con selector de rango de fechas, para que el asesor vea de un vistazo las métricas del periodo sin salir de la app. Hoy el Home es un placeholder en blanco.

## Scope

### In Scope

- 5 tarjetas KPI con su número: Clientes nuevos, Pólizas nuevas, Pólizas renovadas, Pólizas vencidas, Pólizas canceladas.
- Selector de rango en Home: dos DatePickers (Desde / Hasta), defaults = primer día del mes / hoy. `Hasta` no futuro; `Desde` no posterior a `Hasta`. Cambiar el rango recarga los KPIs.
- Etiqueta de periodo "Del {desde} al {hasta}".
- Pull-to-refresh y acción de reintento ante error.
- DTO `IndicadoresPeriodo` + `IReportsService`/`ReportsService` (patrón `CampaignService`: token desde `ITokenStore`, header Bearer por request, resultado tipado). El 401 limpia sesión + `//login` (como `CampaignViewModelBase`).

### Out of Scope

- Detalle de indicadores (`IndicadoresDetalle`), gráficas, tap-through.
- Crear o editar contenido.
- Filtrado por compañía (el SQL del indicador no filtra; conteos globales — aceptado tal cual).

## Capabilities

### New Capabilities

- `indicadores-home`: sección de KPIs en Home con selector de rango de fechas, refresh y manejo de errores.

### Modified Capabilities

- `inicio-y-navegacion`: el requisito "Área de contenido placeholder" cambia — Home deja de estar en blanco y muestra las tarjetas KPI.

## Approach

1. **DTO**: `Models/Responses/IndicadoresPeriodo.cs` (5 ints de conteo + `FechaInicio`/`FechaFin`).
2. **Servicio**: `Services/Api/IReportsService.cs` + `ReportsService.cs`, reutilizando el patrón `CampaignService` (`ITokenStore` → Bearer, resultado tipado `None|Unauthorized|Network|Server|Malformed`). `GetIndicadores(fechaInicio, fechaFin)` → `GET api/Reports/Indicadores`.
3. **ViewModel**: extender `HomeViewModel` con `Desde`, `Hasta`, indicadores, `PeriodLabel`, `IsBusy`, `LoadCommand`/`RefreshCommand`/`RetryCommand`; heredar el manejo 401 de `CampaignViewModelBase`.
4. **Vista**: `Views/HomePage.xaml` — 2 DatePickers + label de periodo + 5 tarjetas KPI + `RefreshView`.

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| `Models/Responses/IndicadoresPeriodo.cs` | New | DTO de respuesta |
| `Services/Api/ReportsService.cs` + interfaz | New | Cliente del endpoint |
| `ViewModels/HomeViewModel.cs` | Modified | Estado KPI + rango + carga + 401 |
| `Views/HomePage.xaml` | Modified | Selector de fechas + tarjetas KPI |

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| Conteos globales (sin filtro por compañía) | High | Aceptado; documentado en spec; sin acción |
| 401 / sesión expirada | Med | Reutilizar manejo 401 (limpiar sesión + `//login`) |
| Fechas inválidas (Hasta futuro, Desde > Hasta) | Med | Validar antes de llamar; defaults seguros |

## Rollback Plan

Revertir `HomeViewModel`/`HomePage.xaml` al placeholder y eliminar `ReportsService`/DTO. Sin migración ni cambios de backend.

## Dependencies

- Backend `GET api/Reports/Indicadores` (verificado) — sin cambios en servidor.

## Success Criteria

- [ ] Home muestra 5 KPIs con defaults del mes actual.
- [ ] Cambiar fechas recarga KPIs; rango siempre válido.
- [ ] Pull-to-refresh y retry funcionan; 401 → login.
- [ ] `dotnet build -f net10.0-android` OK.
