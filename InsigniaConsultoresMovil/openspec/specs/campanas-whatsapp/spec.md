# campanas-whatsapp Specification

## Purpose

Módulo de campañas WhatsApp: listar campañas WhatsApp (`Canal == 2`), ver su detalle, preparar y leer destinatarios, y enviar/deshacer envíos 1 a 1 por WhatsApp registrando el estado contra el backend. Base `https://webapi.insigniaconsultores.com.mx` con `Authorization: Bearer {access_token}` en todos los endpoints.

## Requirements

| ID | Requisito | Fuerza |
|----|-----------|--------|
| CW-R1 | Listado de campañas WhatsApp | DEBE |
| CW-R2 | Detalle de campaña | DEBE |
| CW-R3 | Preparación y carga de destinatarios | DEBE |
| CW-R4 | Búsqueda local de destinatarios | DEBE |
| CW-R5 | Normalización de teléfono | DEBE |
| CW-R6 | Enviar por WhatsApp | DEBE |
| CW-R7 | Deshacer envío | DEBE |
| CW-R8 | Autenticación y errores transversales | DEBE |

### Requirement: Listado de campañas WhatsApp (CW-R1)

El sistema DEBE llamar `GET api/Campana/List`, conservar SOLO las campañas con `Canal == 2` y mostrar únicamente `Nombre` y fecha de creación. DEBE usar `CreatedDt` como fecha de creación; si la API no lo devuelve, DEBE usar `FechaInicio`. Formato `dd/MM/yyyy` (es-MX).

#### Scenario: Lista filtrada de campañas WhatsApp

- GIVEN hay una sesión activa
- WHEN el usuario abre la pantalla de campañas
- THEN se llama `GET api/Campana/List` y se muestran solo las campañas con `Canal == 2`, cada una con `Nombre` y fecha de creación

#### Scenario: Fecha de creación con respaldo

- GIVEN la API devuelve la campaña
- WHEN se renderiza la fecha de creación
- THEN se usa `CreatedDt`; si no existe, se usa `FechaInicio`, formateada `dd/MM/yyyy`

#### Scenario: Carga, vacío y error

- GIVEN la petición está en curso, THEN se muestra un spinner
- GIVEN no hay campañas WhatsApp, THEN se muestra "No hay campañas de WhatsApp"
- GIVEN falla red/timeout/5xx, THEN se muestra un mensaje en español con acción de reintentar

#### Scenario: Actualización manual (SHOULD)

- GIVEN la lista está visible, THEN el sistema DEBERÍA permitir pull-to-refresh para recargar

### Requirement: Detalle de campaña (CW-R2)

El sistema DEBE abrir el detalle con la campaña seleccionada y DEBE llamar `GET api/Campana/{id}` para datos frescos. DEBE mostrar `Nombre`, `Descripcion`, `Asunto`, `Estatus` (mapeado a etiqueta en español), `FechaInicio`, `FechaFin`, `Total`, `Enviados`, `Pendientes`. El botón "Comenzar campaña" DEBE navegar a la pantalla de destinatarios.

#### Scenario: Detalle con datos de la API

- GIVEN el usuario selecciona una campaña
- WHEN se abre el detalle
- THEN se llama `GET api/Campana/{id}` y se muestran los campos disponibles de la API, con `Estatus` mapeado a su etiqueta en español

#### Scenario: Comenzar campaña

- GIVEN el detalle está visible
- WHEN el usuario toca "Comenzar campaña"
- THEN se navega a la pantalla de destinatarios con la campaña seleccionada

### Requirement: Preparación y carga de destinatarios (CW-R3)

Al entrar, si `Campana.Preparada == false` el sistema DEBE llamar `POST api/Campana/{id}/WhatsApp/Preparar` y luego `GET api/Campana/{id}/WhatsApp/Destinatarios`; si `Preparada == true` DEBE llamar SOLO el GET (para evitar filas duplicadas por Preparar repetido). Cada fila DEBE mostrar SOLO `Nombre`, `Telefono`, `Estado` ("Pendiente"/"Enviado") y `FechaEnvio` si fue enviado. NO DEBE mostrar fecha de nacimiento, dirección ni enriquecer al cliente.

#### Scenario: Preparar y leer destinatarios

- GIVEN una campaña con `Preparada == false`
- WHEN el usuario entra a la pantalla de destinatarios
- THEN se llama `POST .../WhatsApp/Preparar` y después `GET .../WhatsApp/Destinatarios`

#### Scenario: Campaña ya preparada

- GIVEN una campaña con `Preparada == true`
- WHEN el usuario entra a la pantalla de destinatarios
- THEN se llama SOLO `GET .../WhatsApp/Destinatarios`, sin duplicar filas

#### Scenario: Contenido de cada fila

- GIVEN los destinatarios cargados
- THEN cada fila muestra solo `Nombre`, `Telefono`, `Estado` y `FechaEnvio` (solo si enviado), sin datos de cliente adicionales

#### Scenario: Estados y virtualización

- GIVEN carga/error/lista vacía, THEN spinner / mensaje en español con reintentar / mensaje de vacío
- GIVEN volumen grande, THEN la lista DEBERÍA virtualizarse con `CollectionView`

### Requirement: Búsqueda local de destinatarios (CW-R4)

El sistema DEBE ofrecer una caja de búsqueda que filtre la lista ya cargada localmente por `Nombre` OR `Telefono`, sin distinguir mayúsculas y por "contiene". NO DEBE llamar a la API. Si no hay resultados DEBE mostrar "Sin resultados para «{texto}»". Limpiar la caja DEBE restaurar la lista completa.

#### Scenario: Filtrado local

- GIVEN la lista de destinatarios cargada y el usuario escribe en la caja
- THEN se filtra localmente por `Nombre` o `Telefono` (case-insensitive, contiene), sin llamada a la API

#### Scenario: Sin resultados

- GIVEN el texto de búsqueda no coincide
- THEN se muestra "Sin resultados para «{texto}»"

#### Scenario: Limpiar búsqueda

- GIVEN hay un filtro activo
- WHEN el usuario limpia la caja
- THEN se restaura la lista completa

### Requirement: Normalización de teléfono (CW-R5)

El sistema DEBE quitar los no-dígitos del teléfono; si quedan más de 10 dígitos DEBE conservar los ÚLTIMOS 10; si quedan menos de 10 el destinatario NO es enviable (botón de envío deshabilitado + nota visible en la fila).

#### Scenario: Normalización a 10 dígitos

- GIVEN un teléfono con formato, espacios o símbolos
- WHEN se normaliza
- THEN se conservan solo dígitos y, si hay más de 10, los últimos 10

#### Scenario: Teléfono insuficiente

- GIVEN un teléfono con menos de 10 dígitos
- THEN el botón "Enviar" queda deshabilitado y se muestra una nota visible en la fila

### Requirement: Enviar por WhatsApp (CW-R6)

El botón "Enviar" por destinatario DEBE abrir `https://wa.me/52{TEN_DIGITS}?text={Uri.EscapeDataString(Descripcion)}`. Inmediatamente tras lanzar WhatsApp, DEBE llamar `POST api/Campana/WhatsApp/Enviado/{destinatarioId}` y actualizar la fila a Enviado (actualización optimista con `FechaEnvio` local; si la llamada falla, revertir la fila y mostrar el error en español). DEBE impedir doble toque mientras el envío/marcado está en curso. Si el deep link no se puede lanzar (WhatsApp no instalado), DEBE mostrar un mensaje en español y NO marcar como enviado.

#### Scenario: Envío exitoso

- GIVEN un destinatario enviable
- WHEN el usuario toca "Enviar"
- THEN se abre `wa.me/52{10dígitos}?text={Descripcion escapada}` y se llama `POST .../WhatsApp/Enviado/{id}`, marcando la fila como Enviado con `FechaEnvio` local

#### Scenario: Falla al marcar enviado

- GIVEN la llamada `Enviado/{id}` falla
- THEN se revierte la fila a su estado previo y se muestra el error en español

#### Scenario: Protección de doble toque

- GIVEN un envío/marcado en curso
- WHEN el usuario toca "Enviar" de nuevo
- THEN se ignora el segundo toque

#### Scenario: WhatsApp no instalado

- GIVEN el deep link no se puede lanzar
- THEN se muestra un mensaje en español y NO se marca como enviado

### Requirement: Deshacer envío (CW-R7)

La acción "Deshacer" DEBE ser visible SOLO en filas en estado Enviado y DEBE llamar `POST api/Campana/WhatsApp/Pendiente/{id}`; en éxito la fila vuelve a Pendiente y se limpia `FechaEnvio`; en fallo revertir y mostrar error.

#### Scenario: Deshacer exitoso

- GIVEN una fila en estado Enviado
- WHEN el usuario toca "Deshacer"
- THEN se llama `POST .../WhatsApp/Pendiente/{id}` y la fila vuelve a Pendiente con `FechaEnvio` limpio

#### Scenario: Falla al deshacer

- GIVEN la llamada `Pendiente/{id}` falla
- THEN se revierte la fila y se muestra el error en español

### Requirement: Autenticación y errores transversales (CW-R8)

Cualquier 401 de los endpoints de campañas DEBE limpiar la sesión almacenada y regresar a la pantalla de login. Errores de red/timeout/5xx DEBEN mostrar mensaje en español con reintentar, nunca fallar en silencio ni colapsar la app.

#### Scenario: Sesión expirada (401)

- GIVEN cualquier endpoint de campañas responde 401
- THEN se limpia la sesión almacenada y se regresa a la pantalla de login

#### Scenario: Error de red/servidor

- GIVEN una falla de red, timeout o 5xx
- THEN se muestra un mensaje en español con reintentar, sin fallo silencioso ni crash
