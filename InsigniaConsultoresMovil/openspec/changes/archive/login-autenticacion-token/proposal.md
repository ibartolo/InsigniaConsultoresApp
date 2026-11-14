# Proposal: Fundación de autenticación (login + token)

## Intent

La app móvil de **Insignia Consultores AXA** parte de la plantilla MAUI por defecto ("Hello World", sin MVVM/DI). Este cambio sienta la base de autenticación: reemplazar la identidad visual de la plantilla, permitir iniciar sesión contra el backend OAuth2 (`/Token`, `grant_type=password`) y persistir el token localmente para auto-login, para luego mostrar un Home vacío con menú hamburguesa.

## Scope

### In Scope
- R1: ícono de app y splash con el logo `logo.png` (600×600, full-bleed, sin alpha).
- R2: pantalla de login; POST a `/Token`; persistencia híbrida (SecureStorage + SQLite cableada); auto-login mientras el token sea válido (expiración desde `expires_in`, margen configurable por defecto 20 min).
- R3: pantalla Home en blanco con menú hamburguesa (Shell Flyout).
- R4: capas simples (Models/Requests, Models/Responses, Services/Api, Services/Storage, Validations, ViewModels, Views).
- R5: `ActivityIndicator` dentro del botón "Iniciar sesión"; botón deshabilitado mientras carga.

### Out of Scope
- Botón "Registrar" y "Recuperar contraseña" (R6 — explícitamente fuera por ahora).
- Renombrar `ApplicationId`/`ApplicationTitle` (se decide como cambio separado).
- Funcionalidad de negocio posterior al login (pólizas, clientes, consultas).
- Pruebas automatizadas (no existe proyecto de tests).

## Capabilities

### New Capabilities
- `branding`: ícono de app, splash screen y paleta de colores/identidad visual de la marca.
- `autenticacion-token`: login OAuth2 password grant, almacenamiento seguro del token y auto-login.
- `inicio-y-navegacion`: pantalla Home post-autenticación con menú hamburguesa (Shell Flyout).

### Modified Capabilities
- None (no hay specs existentes; `openspec/specs/` está vacío).

## Approach

MVVM con DI registrada en `MauiProgram.cs` (`builder.Services`): `HttpClient` tipado hacia la Web API, `IAuthService`/`ITokenStorage`, `IStorageService` (SecureStorage + `sqlite-net-pcl`), `LoginViewModel`, `HomeViewModel`. Flujo OAuth2 password grant contra `POST /Token` (campos `username`/`password`). Token cifrado en SecureStorage; SQLite se cablea (tabla base) para datos futuros. Expiración = `expires_in` menos margen configurable (20 min). Shell Flyout con `LoginPage` (sin flyout) → `HomePage` (con flyout).

**Paleta exacta (para replicar en fases posteriores):**
Primary `#0F4C81` · Dark `#0A3A63` · Light `#1B6BA8` · Orange `#E8952B` · Orange hover `#C97A18` · Cream bg `#FDF8F1` · Body `#4A5568` · Input border `#dde5ee` · Focus border `#E8952B` + ring `rgba(232,149,43,.15)` · Surface `#FFFFFF` · Card border `#eef2f7` · Radius 14px / inputs 10px / pills 999px · Shadow `0 8px 28px rgba(15,76,129,.12)` · Fuente Segoe UI · Hero `linear-gradient(120deg,#0A3A63,#0F4C81 55%,#1B6BA8)` · CTA `linear-gradient(120deg,#E8952B,#C97A18)`.

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| `Resources/AppIcon/` + `Resources/Splash/` | Modified | Reemplazar por `logo.png` |
| `Resources/Styles/Colors.xaml` | Modified | Paleta de marca |
| `MauiProgram.cs` | Modified | Registros DI, HttpClient, SQLite |
| `Models/`, `Services/`, `Validations/`, `ViewModels/`, `Views/` | New | Capas simples |
| `AppShell.xaml` | Modified | Rutas Login/Home, Flyout |
| `App.xaml.cs` | Modified | Lógica de auto-login al arrancar |

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| URL base del backend sin confirmar | High | Flag como supuesto abierto; centralizar en config/constante para cambiarla en un punto |
| Sin proyecto de tests → verificación solo por build | Med | `verify` = `dotnet build -f net10.0-android`; revisión manual del flujo |
| Primer scaffolding MVVM/DI sobre plantilla | Med | Capas mínimas; sin sobre-ingeniería |
| Valores placeholder (`ApplicationId`/`Title`) | Low | No tocar en este cambio; documentar como pregunta abierta |
| `logo.png` full-bleed sin alpha → recorte en Android adaptive icon | Med | Ajustar en `appicon.svg`/splash con fondo sólido; validar visualmente |

## Rollback Plan

Revertir el cambio (borrar la carpeta del change y los archivos de código) restaura la plantilla por defecto. `git checkout` de los archivos de proyecto/resource si ya estuvieran commiteados. La DI y las capas nuevas se eliminan sin romper dependencias externas (no hay código previo que las consuma).

## Dependencies

- Backend Web API 2 con `/Token` operativo (URL exacta pendiente).
- `logo.png` en `C:\Git\InsigniaConsultoresAxaPaginaWeb\imagenes\marca\logo.png` (verificado existente).
- Paquete `sqlite-net-pcl` (a agregar en este cambio).

## Success Criteria

- [ ] Build `dotnet build -f net10.0-android` → SUCCESS.
- [ ] Ícono y splash muestran el logo (sin artefactos de la plantilla).
- [ ] Login envía credenciales a `/Token`, recibe token y lo persiste cifrado.
- [ ] Auto-login funciona mientras el token sea válido (margen de 20 min aplicado).
- [ ] Tras autenticar se navega a Home con menú hamburguesa.
- [ ] Botón "Iniciar sesión" muestra loader y se deshabilita durante la carga.
- [ ] No existen botones "Registrar" ni "Recuperar contraseña".

## Open Questions

- ~~URL base exacta del backend (`/Token`)~~ — **Resuelto:** `https://api.insigniaconsultores.com.mx`, token endpoint `https://api.insigniaconsultores.com.mx/Token`.
- ¿Renombrar `ApplicationId`/`ApplicationTitle` ahora o en un cambio separado?
- ¿Validar el token también contra un endpoint de perfil o solo por expiración local?
