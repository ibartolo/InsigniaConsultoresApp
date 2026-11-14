# Insignia Consultores App (Móvil)

## Propósito

Aplicación móvil para **Insignia Consultores AXA**, una agencia de seguros
(agente de pólizas AXA). La app permitirá a los asesores y clientes gestionar
pólizas, consultas y flujos propios del negocio asegurador.

## Capacidades (specs — fuente de verdad)

Tras los cambios archivados (`login-autenticacion-token` y `campanas-whatsapp`),
existen estas capacidades en el árbol de specs principal (`openspec/specs/`):

- `branding` — identidad visual: ícono, splash y paleta de marca.
- `autenticacion-token` — login OAuth2 (password grant), token seguro y auto-login.
- `inicio-y-navegacion` — Home post-autenticación con Shell Flyout (incluye el ítem "Campañas").
- `campanas-whatsapp` — listado/detalle de campañas WhatsApp, preparación de destinatarios y envío 1 a 1 con registro de estado.

## Stack

- **Framework**: .NET MAUI (proyecto único, `UseMaui=true`)
- **Lenguaje**: C# (.NET 10)
- **UI**: XAML con `MauiXamlInflator=SourceGen`
- **Configuración del proyecto**:
  - `Nullable` habilitado
  - `ImplicitUsings` habilitado

## Frameworks de destino (TargetFrameworks)

| Framework | Descripción |
|-----------|-------------|
| `net10.0-android` | Android |
| `net10.0-ios` | iOS |
| `net10.0-maccatalyst` | macOS (Catalyst) |
| `net10.0-windows10.0.19041.0` | Windows 10+ |

- SDK instalado: `11.0.100` (workloads android/ios/maccatalyst/maui-windows en 11.0.100).
- Build verificado: `dotnet build -f net10.0-android` → **SUCCESS** (0 warnings, 0 errors).

## Backend

- **API**: .NET Framework Web API 2
- **Autenticación**: OAuth2 *password grant*
- **Base de datos**: SQL Server

## Convenciones

### Idioma

- Todo el contenido de cara al usuario se redacta en **español (es-MX)**.
- Los nombres de clases, métodos y archivos siguen las convenciones estándar de
  .NET (PascalCase); las cadenas y textos de UI van en español.

### Estructura en capas (simple)

El proyecto sigue un patrón MVVM con capas sencillas:

```
InsigniaConsultoresApp/
├── Models/
│   ├── Requests/      ← DTOs de entrada a la API
│   └── Responses/     ← DTOs de respuesta de la API
├── Services/
│   ├── Api/           ← Cliente HTTP / integración con la Web API
│   └── Storage/       ← Persistencia local (SecureStorage, Preferences, etc.)
├── Validations/       ← Reglas de validación de formularios
├── ViewModels/        ← MVVM (binding a las vistas)
├── Views/             ← Páginas XAML
├── App.xaml / AppShell.xaml
└── MauiProgram.cs
```

### Estado actual

- Autenticación implementada: login OAuth2 password grant + token persistido en
  SecureStorage/SQLite + auto-login; Home con Shell Flyout.
- Módulo de campañas WhatsApp implementado: listado filtrado (`Canal == 2`), detalle,
  preparación/lectura de destinatarios, búsqueda local, envío 1 a 1 por deep link
  `wa.me/52…` y deshacer, con registro de estado contra el backend.
- MVVM con capas simples (`Models/`, `Services/`, `Validations/`, `ViewModels/`,
  `Views/`) y DI registrada en `MauiProgram.cs`.
- Android-only: `TargetFrameworks` = `net10.0-android`.
- `ApplicationId`: `com.companyname.insigniaconsultoresapp` (placeholder, cambiará).
- `ApplicationTitle`: `InsigniaConsultoresApp` (placeholder).

## Brecha de pruebas (Testing gap)

- **No existe** ningún proyecto de pruebas (sin xUnit, NUnit ni MSTest).
- **No hay** herramienta de cobertura (Coverlet).
- Por lo tanto, **TDD estricto NO está disponible** (`strict_tdd: false`).
- Comando de pruebas: **ninguno**.
- Comando de build de referencia: `dotnet build -f net10.0-android`.
