# Tasks: Módulo de campañas WhatsApp

Proyecto .NET MAUI (carpeta del proyecto: `InsigniaConsultoresApp/`). Ruta base de las tareas: `InsigniaConsultoresApp/`.

## Fase 1: Robustez de contratos (primero)

- [x] 1.1 `Models/Responses/Campana.cs`: `Estatus` tolerante a JSON numérico o texto (converter propio o auto-prop sobre `JsonElement`) → expone `EstatusLabel` en español vía `CampaignStatus.GetLabel` con fallback al valor crudo. [CW-R2]
- [x] 1.2 `Models/Responses/ModelResponseDto.cs`: tolerar payload `enum` OR `response` (`[JsonExtensionData]` o 2ª propiedad opcional `response`); payload ausente → `Malformed`. [CW-R8]
- [x] 1.3 `Models/Responses/Campana.cs`: `CreatedDt` ausente/null → resolver `CreatedDt ?? FechaInicio` en el VM. [CW-R1]

## Fase 2: Modelos/DTOs y helper

- [x] 2.1 `Models/Responses/Campana.cs`: campos `JsonPropertyName` (id, nombre, descripcion, asunto, fechaInicio, fechaFin, canal, preparada, total, enviados, pendientes, excluidos, errores, porcentaje, createdDt) + `static CampaignStatus.GetLabel`. [CW-R1, CW-R2]
- [x] 2.2 `Models/Responses/CampanaWhatsAppDestinatario.cs`: id, campanaID, clienteID, nombre, telefono, estado, fechaEnvio. [CW-R3]
- [x] 2.3 `Models/Responses/CampaignResult.cs`: `enum CampaignErrorKind` + `CampaignResult`/`CampaignResult<T>` (Ok/Fail) + `CampaignMessages` (es-MX). [CW-R8]
- [x] 2.4 `Validations/PhoneHelper.cs`: `TryNormalizeTo10Digits` + `BuildWhatsAppUri` (prefijo `52`). [CW-R5]

## Fase 3: Servicio de campañas

- [x] 3.1 `Services/Api/ICampaignService.cs`: 6 métodos (list, get, prepare, recipients, markSent, markPending) con `CancellationToken`. [CW-R1, CW-R2, CW-R3, CW-R6, CW-R7]
- [x] 3.2 `Services/Api/CampaignService.cs`: bearer por request (`CreateRequestAsync` con `ITokenStore.GetSessionAsync`), filtro `Canal == 2`, clasificación 401→Unauthorized, timeout/`HttpRequestException`→Network, ≥500→Server, resto→Business/Malformed. [CW-R1, CW-R8]

## Fase 4: ViewModels base y de pantalla

- [x] 4.1 `ViewModels/CampaignViewModelBase.cs`: `ExecuteAsync<T>` → `Unauthorized` limpia sesión y `//login` una vez. [CW-R8]
- [x] 4.2 `ViewModels/CampaignsViewModel.cs`: carga lista, loading/vacío/error, `RetryCommand`, `RefreshCommand` (pull-to-refresh), fecha `dd/MM/yyyy`. [CW-R1]
- [x] 4.3 `ViewModels/CampaignDetailViewModel.cs`: detalle con `EstatusLabel` + "Comenzar campaña" → `campaignRecipients?campaignId={id}`. [CW-R2]
- [x] 4.4 `ViewModels/RecipientsViewModel.cs`: Preparar si `!Preparada` → GET destinatarios; `SearchText` filtro local; envío/deshacer optimista + rollback; estados vacío. [CW-R3, CW-R4, CW-R6, CW-R7]
- [x] 4.5 `ViewModels/RecipientItemViewModel.cs`: estado por fila (`IsSent`, `IsBusy`, `FechaEnvio`, `HasInvalidPhone`, `CanSend`, `SendCommand`/`UndoCommand`). [CW-R5, CW-R6, CW-R7]

## Fase 5: Vistas

- [x] 5.1 `Views/CampaignsPage.xaml(.cs)`: lista con `RefreshView`+`CollectionView`, spinner/vacío/error con `BrandPrimaryButton` "Reintentar", estilos de marca. [CW-R1]
- [x] 5.2 `Views/CampaignDetailPage.xaml(.cs)`: campos + `EstatusLabel`, botón "Comenzar campaña" (`BrandButton`). [CW-R2]
- [x] 5.3 `Views/RecipientsPage.xaml(.cs)`: `SearchBar` arriba + `CollectionView` por fila (Enviar/Deshacer según `IsSent`, nota teléfono inválido). [CW-R3, CW-R4, CW-R5, CW-R6, CW-R7]

## Fase 6: Navegación

- [x] 6.1 `AppShell.xaml`: `FlyoutItem Title="Campañas" Route="campaigns"` tras "Inicio". [IN-R2]
- [x] 6.2 `AppShell.xaml.cs`: `Routing.RegisterRoute` para `campaign-detail`/`campaign-recipients`. [IN-R5]
- [x] 6.3 `[QueryProperty(nameof(CampaignId))]` en detalle/destinatarios; `GoToAsync` rutas globales; back nunca a `//login`. [IN-R5]

## Fase 7: DI wiring

- [x] 7.1 `MauiProgram.cs`: registrar `ICampaignService`→`CampaignService` (singleton), 3 ViewModels y 3 páginas (transient). [CW-*, IN-*]

## Fase 8: Verificación

- [ ] 8.1 Build: `dotnet build -f net10.0-android` → SUCCESS.
- [ ] 8.2 Test manual ordenado: login→Home; flyout Inicio/Campañas/Cerrar; lista solo `Canal==2` (Nombre+fecha); pull-to-refresh; detalle (Estatus es); Comenzar→Preparar→lista Nombre/Teléfono/Estado; búsqueda (sin match→"Sin resultados", limpiar→completa); enviar válido (`wa.me/52{10}?text=`)→fila Enviado; teléfono <10→botón deshabilitado+nota; deshacer→Pendiente; 401→sesión limpia+login; modo avión→error es+Reintentar; sin WhatsApp→mensaje es sin marcar. [CW-R1..R8, IN-R2, IN-R5]
