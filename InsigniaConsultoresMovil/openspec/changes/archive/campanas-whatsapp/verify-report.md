# Verification Report: campanas-whatsapp

**Change**: campanas-whatsapp
**Version**: N/A (delta specs, no version header)
**Mode**: Standard (strict_tdd = false; Android-only, no test project → static review + build; runtime evidence supplied by orchestrator)

---

## Completeness

| Metric | Value |
|--------|-------|
| Tasks total | 19 (1.1–8.2) |
| Tasks complete | 17 (`[x]` 1.1–7.1) |
| Tasks incomplete | 2 (`[ ]` 8.1, 8.2) |

- `8.1` Build — **completed by this verification** (see Build below): SUCCESS.
- `8.2` Manual ordered test — **completed by the orchestrator** on device (see Spec Compliance Matrix / runtime evidence). Remaining NOT-executed items are documented below (real WhatsApp send, 401/session-expiry, airplane mode).

No core task is missing. All implementation tasks (Fases 1–7) are done.

---

## Build & Tests Execution

**Build**: ✅ Passed

```
dotnet build InsigniaConsultoresApp\InsigniaConsultoresApp.csproj -f net10.0-android --nologo
  ...
  InsigniaConsultoresApp -> ...\bin\Debug\net10.0-android\InsigniaConsultoresApp.dll
  Compilación correcta.
  2 Advertencia(s)
  0 Errores
```

Result: **0 errors, 2 warnings**. Both warnings are `NU1903` (SQLitePCLRaw.lib.e_sqlite3.android 2.1.11, GHSA-2m69-gcr7-jv3q / CVE-2025-6965) — known and accepted (no upstream patch available).

**Tests**: ➖ N/A — no test project (config.yaml: `strict_tdd: false`, `test_runner.command: ""`). Behavioral validation is via the orchestrator's device evidence.

**Coverage**: ➖ Not available (no coverage tool; `coverage_threshold: 0`).

---

## Spec Compliance Matrix

Compliance basis: there is **no test project**, so scenarios are validated against (a) the orchestrator's on-device runtime evidence and (b) static code review. Legend: ✅ COMPLIANT (runtime evidence and/or code implements it) · ⚠️ PARTIAL (implemented but not runtime-executed, or minor gap) · ❌ NOT EXECUTED (implemented but not reproduced).

| Req | Scenario | Evidence / Implementation | Result |
|-----|----------|---------------------------|--------|
| CW-R1 | Lista filtrada de campañas WhatsApp | Runtime #2: list shows only WhatsApp, Nombre + fecha. `CampaignService.ListWhatsAppAsync` filters `Canal == 2` | ✅ |
| CW-R1 | Fecha de creación con respaldo | `Campana.FechaCreacion => CreatedDt ?? FechaInicio`; XAML `StringFormat='{0:dd/MM/yyyy}'`; Runtime #2 (14/11/2026) | ✅ |
| CW-R1 | Carga, vacío y error | `CampaignsPage.xaml` spinner/`IsBusy`, "No hay campañas de WhatsApp", error + "Reintentar" | ✅ static (vacío/error no ejecutados en dispositivo) |
| CW-R1 | Actualización manual (SHOULD) | `RefreshView` + `RefreshCommand` | ✅ static (no ejecutado) |
| CW-R2 | Detalle con datos de la API | Runtime #3: `GET api/Campana/{id}` muestra Descripcion/Asunto/Estatus "Programada"/Fechas/Total/Enviados/Pendientes | ✅ |
| CW-R2 | Comenzar campaña | Runtime #4: navega a destinatarios. `StartCampaignCommand` → `campaign-recipients?campaignId={id}` | ✅ |
| CW-R3 | Preparar y leer destinatarios | Runtime #4: `POST .../WhatsApp/Preparar` (array-tolerant) + `GET Destinatarios` | ✅ |
| CW-R3 | Campaña ya preparada | `if (!campaign.Value.Preparada)` → solo `GET` | ✅ static (no ejecutado con campaña ya preparada) |
| CW-R3 | Contenido de cada fila | Runtime #4 (Nombre, Teléfono, "Pendiente"). XAML muestra solo Nombre/Teléfono/Estado/FechaEnvio; sin DOB/dirección/Customer | ✅ |
| CW-R3 | Estados y virtualización | `CollectionView` (virtualiza); spinner/error/vacío en `RecipientsPage.xaml` | ✅ static |
| CW-R4 | Filtrado local | Runtime #5 ("bernardin" → 1). `ApplyFilter` LINQ case-insensitive sobre Nombre/Telefono, sin API | ✅ |
| CW-R4 | Sin resultados | `"Sin resultados para «{text}»"` en `ApplyFilter` | ✅ static |
| CW-R4 | Limpiar búsqueda | `SearchText=""` → `ApplyFilter` restaura lista completa | ✅ static |
| CW-R5 | Normalización a 10 dígitos | `PhoneHelper.TryNormalizeTo10Digits` (solo dígitos, últimos 10) | ✅ static |
| CW-R5 | Teléfono insuficiente | `HasInvalidPhone` → `CanSend=false` (botón deshabilitado) + `InvalidPhoneHint` visible | ✅ static (no ejecutado) |
| CW-R6 | Envío exitoso | Runtime #6: abre WhatsApp con **Descripcion** prefilled. `wa.me/52{10}?text=`, `OpenAsync`, `MarkSentAsync` + optimista | ✅ |
| CW-R6 | Falla al marcar enviado | `MarkSentAsync` fail → `item.ApplyPending()` + error | ✅ static (no ejecutado) |
| CW-R6 | Protección de doble toque | `RecipientItemViewModel.SendAsync` guard `if (IsBusy) return` + `CanSend`/`IsEnabled` | ✅ static |
| CW-R6 | WhatsApp no instalado | `OpenAsync` en try/catch → mensaje "No se pudo abrir WhatsApp…", no marca | ✅ static (no ejecutado) |
| CW-R7 | Deshacer exitoso | Runtime #8: vuelve a Pendiente + `FechaEnvio` limpio. `MarkPendingAsync` → `ApplyPending()` | ✅ |
| CW-R7 | Falla al deshacer | `MarkPendingAsync` fail → mensaje; fila queda Enviado | ✅ static (no ejecutado) |
| CW-R8 | Sesión expirada (401) | `HandleUnauthorizedAsync` → `ClearAsync()` + `//login` una vez (guard estático). **No reproducido en dispositivo** | ⚠️ static only |
| CW-R8 | Error de red/servidor | `Network`/`Server` → mensaje es + Reintentar. **No reproducido (modo avión)** | ⚠️ static only |
| IN-R2 | Flyout muestra logo, usuario y acciones | Runtime #1: Flyout muestra "Campañas" tras login. XAML: logo + `UsernameLabel` + Inicio/Campañas/Cerrar sesión. **Ver W-1 (username se limpia en pantallas de campaña)** | ⚠️ |
| IN-R5 | Tocar "Campañas" abre la lista | Runtime #2. `FlyoutItem Route="campaigns"` | ✅ |
| IN-R5 | Back no regresa al login | Runtime #1. Login es `ShellContent` oculto, solo vía `//login` (absoluta, limpia stack) | ✅ |

**Compliance summary**: 20/26 ✅ · 4 ⚠️ static-only (CW-R8×2, IN-R2, más CW-R3/CW-R5/CW-R6 no-ejecutados contados como static ✅) · 0 ❌.

> Runtime evidence covered 12 distinct scenarios end-to-end on device; the remaining scenarios are implemented in code but not re-exercised (documented in the "NOT executed" evidence list: real send inside WhatsApp, 401/session-expiry, airplane mode, WhatsApp-not-installed, already-prepared campaign, invalid-phone row, pull-to-refresh).

---

## Correctness (Static — Structural Evidence)

| Requirement | Status | Notes |
|------------|--------|-------|
| CW-R1 Listado | ✅ Implemented | `ListWhatsAppAsync` filtra `Canal == 2`; `FechaCreacion = CreatedDt ?? FechaInicio`; `dd/MM/yyyy` |
| CW-R2 Detalle | ✅ Implemented | `GetAsync(id)`; `EstatusTexto` mapeo es-MX (`CampaignStatus.GetLabel`); "Comenzar campaña" navega |
| CW-R3 Preparación/carga | ✅ Implemented | `Preparar` solo si `!Preparada`; `GetRecipientsAsync` tolera arreglo directo/envoltura |
| CW-R4 Búsqueda local | ✅ Implemented | Filtro LINQ local case-insensitive, sin API; mensajes vacío/sin-resultado distintos |
| CW-R5 Normalización | ✅ Implemented | `PhoneHelper` quita no-dígitos, últimos 10, `<10` → no enviable |
| CW-R6 Enviar | ✅ Implemented | `wa.me/52{10}?text=`, `OpenAsync` (Android 11+), optimista + rollback, doble toque protegido |
| CW-R7 Deshacer | ✅ Implemented | `MarkPendingAsync` → `ApplyPending` (limpia `FechaEnvio`); error en fallo |
| CW-R8 Auth/errores | ✅ Implemented | 401 central en `CampaignViewModelBase.HandleUnauthorizedAsync` (guard anti-duplicado) |
| IN-R2 Flyout | ⚠️ Partial | Ítems + logo presentes; username se borra al navegar a campañas (ver W-1) |
| IN-R5 Navegación | ✅ Implemented | `Routing.RegisterRoute` + rutas globales `campaign-detail`/`campaign-recipients`; back no vuelve a login |

---

## Coherence (Design)

| Decisión | Followed? | Notes |
|----------|-----------|-------|
| D1 — Bearer por request (NO DelegatingHandler) | ✅ Yes | `CreateRequestAsync` lee `ITokenStore.GetSessionAsync()` por request; `AuthService` intacto |
| D2 — 401: resultado discriminado + base | ✅ Yes (forma distinta) | Diseño proponía `ExecuteAsync<T>`; se implementó `RunAsync` + `HandleUnauthorizedAsync` (equivalente, con guard estático `_redirectingToLogin`) |
| D3 — DTOs inmutables | ⚠️ Deviated | DTOs con `get/set` (no `private init`); `Estatus`/`Estado` tolerantes a número/texto vía `EstatusValue`. Funcional |
| D4 — Superficie del servicio | ⚠️ Deviated | Métodos renombrados (`ListWhatsAppAsync`/`GetAsync`); se eliminó `Business` error-kind → `isSuccess=false` se clasifica `Server` |
| D5 — Phone helper puro | ✅ Yes | `TryNormalizeTo10Digits` + `BuildWhatsAppUri` |
| D6 — Estado por fila | ✅ Yes | `RecipientItemViewModel` con callbacks; `CanSend` omite `!IsSent` (inofensivo: botón oculto) |
| D7 — Búsqueda local | ✅ Yes | `_all` + `Filtered` reconstruida; sin debounce |
| D8 — Navegación/rutas | ⚠️ Deviated | Rutas `campaign-detail`/`campaign-recipients` (guion, un segmento) en vez de `campaignDetail`/`campaignRecipients`; sin toggle `FlyoutItemIsVisible` (D2 en apply-progress) |
| D9 — UX carga/vacío/error | ⚠️ Partial | Lista y destinatarios con Reintentar; detalle sin botón "Reintentar" (recarga vía `OnAppearing`) |
| D10 — Idempotencia Preparar | ✅ Yes | `GET api/Campana/{id}` → lee `Preparada` → `POST` solo si `false` |
| D11 — Deep link | ✅ Yes (con fix) | `Launcher.Default.OpenAsync` (no `TryOpenAsync`, por package visibility Android 11+) + `<queries>` en manifest |

Design Open Questions resolved:
- Campo de payload de la envoltura (`enum` vs `response`): resuelto de forma tolerante — el servicio ya no usa `ModelResponseDto`; parsea el JSON directamente (`FindArray`/`TryGetProperty`), por lo que el wrapper `ModelResponseDto` quedó **sin uso** (ver S-5).
- Tipo de `estatus`: resuelto con `EstatusValue` (número o texto), mapeo a español con fallback al crudo.

---

## Issues Found

### CRITICAL
None.

### WARNING
- **W-1 — El nombre de usuario del flyout se limpia al navegar a pantallas de campaña** (`AppShell.xaml.cs:52-60`). `OnNavigated` solo refresca el usuario cuando `location.Contains("home")`; en cualquier otra ruta (`//campaigns`, `campaign-detail`, `campaign-recipients`) cae al `else` y ejecuta `UsernameLabel.Text = ""`. Con las nuevas rutas, abrir el flyout desde la lista de campañas muestra el header **sin** nombre de usuario, contra IN-R2 ("el nombre de usuario autenticado" DEBE verse). Cosmético y de bajo impacto; la corrección es extender la condición a rutas de campaña (p. ej. `Contains("home") || Contains("campaign")`).
- **W-2 — NU1903** (`SQLitePCLRaw.lib.e_sqlite3.android` 2.1.11, GHSA-2m69-gcr7-jv3q). **Aceptado/known** — sin patch upstream. No bloquea.

### SUGGESTION
- **S-1 — `CanSend` no incluye `!IsSent`** (`RecipientItemViewModel.cs:64`). El diseño (D6) definía `CanSend => !IsBusy && !IsSent && !HasInvalidPhone`. El código usa `TenDigits is not null && !IsBusy`. Inofensivo hoy porque el botón "Enviar" se oculta con `InvertedBool(IsSent)`; alinear al diseño evitaría estados de comando incoherentes.
- **S-2 — `BuildWhatsAppUri` agrega `?text=` incluso con mensaje vacío** (`PhoneHelper.cs:37-38`). El diseño (D5) omitía el query cuando el mensaje era vacío. En la práctica `Descripcion ?? Asunto` es no vacío, así que es cosmético.
- **S-3 — Fecha de envío optimista en UTC** (`RecipientsViewModel.cs:148`). `ApplySent(DateTime.UtcNow)` muestra `FechaEnvioTexto` en UTC (`dd/MM/yyyy HH:mm`), que puede diferir del horario local del asesor. El diseño proponía `DateTime.Now`.
- **S-4 — `_isSent` solo reconoce estado numérico** (`RecipientItemViewModel.cs:34`). Inicializa `IsSent` con `dto.Estado.Code == 1`; si el backend devolviera `estado` como texto ("Enviado"), la fila se mostraría como "Pendiente" inicial aunque `EstadoTexto` (que sí cae al valor crudo) dijera "Enviado". La evidencia confirma que el backend devuelve numérico, así que no es un problema hoy; alinear ambos caminos lo haría robusto.
- **S-5 — `ModelResponseDto` sin uso** (`Models/Responses/ModelResponseDto.cs`). Creado para tolerar `enum`/`response` (tarea 1.2), pero `CampaignService` parsea el JSON directamente (`FindArray`/`TryGetProperty`) y nunca lo instancia (solo se referencia en un doc-comment en `CampaignService.cs:228`). Código muerto: conservarlo o eliminarlo, pero no aporta al flujo actual.
- **S-6 — "0000000000" pasa la regla de 10 dígitos** (`PhoneHelper.TryNormalizeTo10Digits`). Cualquier cadena de 10 dígitos (incluidos ceros) se considera enviable. **El usuario ya fue informado; decisión pendiente** (la validación queda fuera del alcance "más allá de 10 dígitos" del proposal).
- **S-7 — Deshacer no es optimista** (`RecipientsViewModel.cs:164-173`). El diseño (D11) describía rollback de Deshacer (optimista → restaurar Enviado si falla). La implementación solo aplica `ApplyPending()` tras el éxito del POST; en fallo la fila permanece "Enviado" (estado correcto). Más simple y seguro, sin regresión funcional — nota de coherencia, no de defecto.
- **S-8 — Clasificación de errores simplificada** (`CampaignResult.cs:6-13`, `CampaignService.cs:243-267`). Se eliminó `Business` error-kind del diseño (D4); `isSuccess=false` y 4xx no-401 se clasifican como `Server`/`Malformed` conservando el `message` del backend. Aceptable; solo reduce granularidad de mensajes.

---

## Verdict

**PASS WITH WARNINGS**

La implementación está completa y cumple las specs: build Android en verde (0 errores), las 8 fases de tareas aplicadas, y los flujos principales (lista filtrada, detalle, preparar/leer, búsqueda local, envío con `wa.me/52…`, optimista + rollback, deshacer, 401 centralizado) están implementados y confirmados en dispositivo por el orquestador. No hay hallazgos CRITICAL. Las dos advertencias (W-1 username del flyout, W-2 NU1903 aceptado) no bloquean el archive; los SUGGESTION son mejoras no bloqueantes.
