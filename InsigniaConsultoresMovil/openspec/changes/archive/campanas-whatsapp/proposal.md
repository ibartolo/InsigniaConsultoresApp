# Proposal: Módulo de campañas WhatsApp

## Intent

La agencia hoy hace outreach por WhatsApp desde la app web MVC. Este cambio lo lleva al teléfono: un asesor autenticado ve sus campañas WhatsApp (solo `Canal == 2`), las abre, prepara los destinatarios y dispara el envío 1 a 1 por WhatsApp, registrando el estado (Pendiente/Enviado) contra el backend. Valor: agilidad de campo y consistencia con el flujo existente del web.

## Scope

### In Scope
- Ítem de flyout **"Campañas"** visible solo con sesión activa.
- Listado de campañas **solo WhatsApp**, mostrando **solo `Nombre` + fecha de creación**.
- Detalle de campaña: `Descripcion`, `Asunto`, `Estatus`, fechas y totales + botón **"Comenzar campaña"**.
- Pantalla de destinatarios: `POST WhatsApp/Preparar` → `GET WhatsApp/Destinatarios`.
- Lista de destinatarios con **solo lo que devuelve la API**: `Nombre`, `Telefono`, `Estado` (+ `FechaEnvio` si enviado).
- Enviar por destinatario: normalizar `Telefono` a 10 dígitos, anteponer prefijo `52`, abrir `https://wa.me/52{10dígitos}?text={EscapeDataString(Asunto)}` y, de inmediato, marcar Enviado con `POST WhatsApp/Enviado/{id}`.
- Deshacer por destinatario enviado: `POST WhatsApp/Pendiente/{id}` → vuelve a Pendiente.
- Nuevo `CampaignService` + DTOs (`Campana`, `CampanaWhatsAppDestinatario`, `ModelResponse`) en `Models/Responses`; 3 ViewModels y 3 `ContentPage`; rutas Shell; reuso de token/`HttpClient`.

### Out of Scope
- Adjunto/imagen de la campaña.
- Fecha de nacimiento, dirección y **cualquier** llamada a `api/Customer/*` o enriquecimiento de cliente.
- Anexos: `GET Anexos`, `GET Anexo/{id}`, `GET Anexo/{id}/File`.
- CRUD de campañas (crear/editar/borrar/cancelar).
- Campañas de correo (`Canal == 1`) y plantillas.
- Envío de prueba.
- Validación de teléfono más allá de los 10 dígitos.

## Capabilities

### New Capabilities
- `campanas-whatsapp`: listado/detalle de campañas WhatsApp, preparación y lectura de destinatarios, envío y deshacer por WhatsApp con registro de estado.

### Modified Capabilities
- `inicio-y-navegacion`: nuevo ítem de flyout "Campañas" + rutas Shell de las 3 vistas.

## Approach

MVVM simple + DI en `MauiProgram`. Nuevo `CampaignService` con `HttpClient` (mismo `BaseAddress` = `AuthConfig.BaseUrl`) que inyecta `Authorization: Bearer {token}` por request (token desde SecureStorage; reutilizar el acceso del `AuthService` existente — decidir en design si se comparte el `HttpClient` singleton o se crea uno dedicado). DTOs inmutables en `Models/Responses`. ViewModels: `CampaignsViewModel` (carga `GET api/Campana/List`, filtra `Canal == 2`), `CampaignDetailViewModel` (datos de detalle + navegar a destinatarios), `RecipientsViewModel` (preparar, leer destinatarios, enviar/deshacer, estado de carga por fila). Vistas: `CampaignsPage`, `CampaignDetailPage`, `RecipientsPage`. `AppShell` registra las rutas y agrega el ítem "Campañas". Normalización de teléfono: quitar no-dígitos → si >10 tomar últimos 10; anteponer `52`. Envío por deep link abre WhatsApp (sale de la app) y marca Enviado tras el lanzamiento (sin confirmar el envío real dentro de WhatsApp).

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| `MauiProgram.cs` | Modified | Registro DI de `CampaignService` + ViewModels |
| `Models/Responses/` | New | `Campana`, `CampanaWhatsAppDestinatario`, `ModelResponse` |
| `Services/Api/CampaignService.cs` | New | Llamadas a `api/Campana/*` |
| `ViewModels/` | New | `CampaignsViewModel`, `CampaignDetailViewModel`, `RecipientsViewModel` |
| `Views/` | New | `CampaignsPage`, `CampaignDetailPage`, `RecipientsPage` |
| `AppShell.xaml(.cs)` | Modified | Ítem flyout "Campañas" + rutas |

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| Token expirado/ausente → 401 | Med | Manejo central de 401 → redirigir a login (reusar patrón de auth) |
| Lista grande de destinatarios → rendimiento | Med | `CollectionView` con virtualización; sin N+1 |
| Deep link falla si WhatsApp no instalado | Med | `Launcher.TryOpenAsync` → aviso si falla |
| Sin confirmación real de envío dentro de WhatsApp | High | Aceptado por el usuario: se marca Enviado tras lanzar el link |
| `Preparar` lento o filas duplicadas en llamadas repetidas | Med | Loading + deshabilitar botón; no re-preparar si ya hay destinatarios |
| Consistencia UI en español (es-MX) | Low | Reutilizar estilos/patrones existentes del login/home |

## Rollback Plan

Revertir el change (borrar carpeta + archivos nuevos) deja la app igual al estado post-login: `git checkout` de `MauiProgram.cs` y `AppShell.xaml(.cs)`, y eliminar `CampaignService`, DTOs, ViewModels y Vistas nuevos. No hay código previo que consuma estos componentes, así que no rompen dependencias.

## Dependencies

- Backend `https://webapi.insigniaconsultores.com.mx` con los endpoints de `api/Campana/*` operativos (contrato verificado).
- Token OAuth2 disponible (fundación de `login-autenticacion-token` ya implementada).
- No se requieren paquetes NuGet nuevos.

## Success Criteria

- [ ] Build `dotnet build -f net10.0-android` → SUCCESS.
- [ ] Ítem "Campañas" aparece solo con sesión activa y lista solo campañas WhatsApp (`Nombre` + fecha).
- [ ] Detalle muestra la info de campaña y navega a destinatarios vía "Comenzar campaña".
- [ ] Destinatarios se cargan tras Preparar y muestran solo Nombre/Teléfono/Estado (+FechaEnvio).
- [ ] Enviar abre el deep link con teléfono normalizado a 10 dígitos + prefijo 52 y marca Enviado.
- [ ] Deshacer regresa el destinatario a Pendiente.
- [ ] Un 401 redirige a login; sin llamadas a `api/Customer/*`; sin adjuntos ni CRUD de campañas.

## Open Questions

- None — el usuario ya decidió las preguntas principales (teléfono celular, prefijo 52, sin imagen, sin enriquecimiento de cliente).
