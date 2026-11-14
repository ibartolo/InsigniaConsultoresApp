# autenticacion-token Specification

## Purpose

Autenticación de usuarios contra el backend OAuth2 (`grant_type=password`), almacenamiento seguro del token y auto-login por expiración local.

## Requirements

### Requirement: Pantalla de inicio de sesión

El sistema DEBE mostrar una pantalla de login con:

- **Hero compacto** (altura según contenido, sin alto fijo): logo (56×56, radio 12px) arriba y título **"Insignia Consultores Financieros"** debajo.
- **Etiquetas arriba de cada campo**: "Usuario" y "Contraseña" (negrita, ~13px, color `#0F4C81`).
- **Campos sin caja**: sin borde, sin fondo y sin caja, a todo el ancho del contenido; separados visualmente solo por una **línea inferior de 1px `#dde5ee`** que pasa a **2px naranja `#E8952B`** al enfocar.
- Campo de contraseña enmascarado y botón "Iniciar sesión" con `ActivityIndicator` dentro del botón.
- **Footer "By DESI"** centrado, pequeño (~12px) y de color atenuado.

El sistema NO DEBE incluir botón "Registrar" ni enlace "Recuperar contraseña" (fuera de alcance).

#### Scenario: Elementos de la pantalla de login

- GIVEN la app no tiene sesión activa
- WHEN se muestra la pantalla de login
- THEN se ven el logo y el título "Insignia Consultores Financieros" en el hero compacto
- AND las etiquetas "Usuario" y "Contraseña" están encima de cada campo
- AND los campos no tienen caja ni borde (solo línea inferior de 1px)
- AND se ven el botón "Iniciar sesión" y el footer "By DESI"
- AND no existen botones "Registrar" ni "Recuperar contraseña"

#### Scenario: Teclado en pantalla al tocar un campo

- GIVEN se muestra la pantalla de login en un dispositivo físico
- WHEN el usuario toca el campo de usuario o el de contraseña
- THEN el campo adquiere el foco y se abre el teclado en pantalla (soft keyboard)
- AND la línea inferior del campo enfocado pasa a naranja `#E8952B` (2px)

### Requirement: Validación de credenciales

El sistema DEBE validar ambos campos antes de enviar la solicitud.

#### Scenario: Campos vacíos

- GIVEN el usuario deja vacío el campo usuario o contraseña
- WHEN toca "Iniciar sesión"
- THEN se muestra un mensaje inline en español y la solicitud NO se envía
- AND el botón se re-habilita

### Requirement: Solicitud de token OAuth2

El sistema DEBE enviar `POST https://webapi.insigniaconsultores.com.mx/Token` con `Content-Type: application/x-www-form-urlencoded` y body `grant_type=password&username=...&password=...`.

#### Scenario: Respuesta exitosa

- GIVEN las credenciales son válidas
- WHEN se envía la solicitud
- THEN la respuesta incluye `access_token`, `token_type` (Bearer) y `expires_in` (segundos)
- AND opcionalmente `refresh_token`, `.issued` y `.expires`

#### Scenario: Respuesta malformada

- GIVEN el backend responde con un body vacío o malformado
- WHEN se procesa la respuesta
- THEN se muestra "Respuesta inválida del servidor" y el botón se re-habilita

### Requirement: Estado de carga y anti doble envío

El sistema DEBE deshabilitar el botón y mostrar el `ActivityIndicator` durante la carga, impidiendo solicitudes duplicadas.

#### Scenario: Carga en curso

- GIVEN el usuario toca "Iniciar sesión"
- WHEN la solicitud está en vuelo
- THEN el botón está deshabilitado y el spinner visible
- AND un segundo toque NO genera una solicitud duplicada

### Requirement: Persistencia híbrida del token

El sistema DEBE persistir el token cifrado en `SecureStorage` y DEBE inicializar SQLite (`sqlite-net-pcl`) listo para datos futuros (sin tablas de negocio en este cambio). Debe almacenar: `access_token`, `token_type`, `expires_at` (UTC) y `username`.

#### Scenario: Token persistido tras login exitoso

- GIVEN el login es exitoso
- WHEN se persiste la sesión
- THEN el token se guarda cifrado en SecureStorage y la fila de sesión en SQLite
- AND la contraseña se limpia de memoria

### Requirement: Regla de expiración del token

El sistema DEBE calcular `expires_at = now_utc + expires_in - margen`, con margen por defecto **20 minutos** configurable en una única constante/configuración. Si `expires_in` falta o es ≤ margen, DEBE tratar el token como válido por `expires_in` sin margen (nunca negativo).

#### Scenario: Cálculo con margen

- GIVEN `expires_in` = 3600 segundos y margen = 1200 segundos
- WHEN se calcula la expiración
- THEN `expires_at` = ahora UTC + 2400 segundos

#### Scenario: Fallback sin margen

- GIVEN `expires_in` falta o es ≤ margen
- WHEN se calcula la expiración
- THEN el token es válido por `expires_in` sin margen (nunca negativo)

### Requirement: Flujo de arranque (auto-login)

El sistema DEBE evaluar la sesión al arrancar: si hay token válido → ir a Home; si expiró o no existe → mostrar login.

#### Scenario: Token válido al arrancar

- GIVEN existe un token con `expires_at` futuro
- WHEN la app arranca
- THEN se salta el login y navega directo a Home

#### Scenario: Token expirado o ausente

- GIVEN no hay token o `expires_at` venció
- WHEN la app arranca
- THEN se muestra la pantalla de login

### Requirement: Sesión expirada en uso (401)

El sistema DEBE manejar la expiración del token mientras la app está abierta: la próxima solicitud protegida con 401 DEBE limpiar la sesión y regresar al login.

#### Scenario: 401 en solicitud protegida

- GIVEN el token expira mientras la app está en uso
- WHEN una solicitud protegida responde 401
- THEN se limpia la sesión (SecureStorage + SQLite) y se regresa al login

### Requirement: Manejo de errores de autenticación

El sistema DEBE mostrar mensajes de error en español, detener el spinner y re-habilitar el botón en todos los casos de error.

| Condición | Mensaje |
|-----------|---------|
| 400 / 401 credenciales inválidas | "Usuario o contraseña incorrectos" |
| timeout / sin conexión | "No se pudo conectar con el servidor" |
| 5xx | "Ocurrió un error en el servidor, intenta de nuevo" |
| respuesta vacía/malformada | "Respuesta inválida del servidor" |

#### Scenario: Credenciales inválidas

- GIVEN las credenciales son inválidas
- WHEN el backend responde 400 o 401
- THEN se muestra "Usuario o contraseña incorrectos", el spinner se detiene y el botón se re-habilita

#### Scenario: Error de red

- GIVEN no hay conexión o hay timeout
- WHEN se envía la solicitud
- THEN se muestra "No se pudo conectar con el servidor" y el botón se re-habilita

#### Scenario: Error del servidor

- GIVEN el backend responde 5xx
- WHEN se envía la solicitud
- THEN se muestra "Ocurrió un error en el servidor, intenta de nuevo" y el botón se re-habilita

### Requirement: Cierre de sesión

El sistema DEBE permitir cerrar sesión: limpiar el token de SecureStorage y la fila de sesión de SQLite, y regresar al login.

#### Scenario: Logout

- GIVEN hay una sesión activa
- WHEN el usuario cierra sesión
- THEN se limpian SecureStorage y SQLite y se muestra el login
