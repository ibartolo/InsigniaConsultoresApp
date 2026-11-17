# indicadores-home Specification

## Purpose

Sección de indicadores (KPIs) en la pantalla Home: 5 tarjetas con conteos del negocio para un rango de fechas, con selector Desde/Hasta, estados de carga/error y refresco. Datos de `GET api/Reports/Indicadores?fechaInicio=YYYY-MM-DD&fechaFin=YYYY-MM-DD` (Bearer).

## Contract

`GET api/Reports/Indicadores` responde `{ clientesNuevos, polizasNuevas, polizasRenovadas, polizasVencidas, polizasCanceladas, fechaInicio, fechaFin }` (conteos enteros). Defaults del servidor: inicio = primer día del mes actual, fin = hoy.

## Requirements

| ID | Requisito | Fuerza |
|----|-----------|--------|
| IH-R1 | Carga al entrar | DEBE |
| IH-R2 | Las 5 tarjetas | DEBE |
| IH-R3 | Período | DEBE |
| IH-R4 | Selector de fechas | DEBE |
| IH-R5 | Estados | DEBE |
| IH-R6 | Refresco y sesión | DEBE |

### Requirement: Carga al entrar (IH-R1)

El sistema DEBE cargar los indicadores al aparecer Home usando el rango de fechas seleccionado. DEBE mostrar un estado de carga mientras la petición está en curso y NO DEBE bloquear el menú Flyout.

#### Scenario: Carga inicial

- GIVEN hay una sesión activa
- WHEN aparece Home
- THEN se llama `GET api/Reports/Indicadores` con el rango seleccionado y se muestra un spinner

#### Scenario: Flyout no bloqueado

- GIVEN la carga está en curso
- WHEN el usuario abre el menú hamburguesa
- THEN el Flyout se abre normalmente

### Requirement: Las 5 tarjetas (IH-R2)

El sistema DEBE mostrar exactamente 5 tarjetas KPI con etiqueta en español y valor entero formateado es-MX: "Clientes nuevos", "Pólizas nuevas", "Pólizas renovadas", "Pólizas vencidas", "Pólizas canceladas". Las tarjetas NO DEBEN ofrecer detalle ni navegación al tocarlas.

#### Scenario: Tarjetas con etiqueta y valor

- GIVEN los indicadores cargados
- THEN se muestran 5 tarjetas, cada una con su etiqueta en español y su número formateado es-MX

#### Scenario: Sin detalle ni tap-through

- GIVEN las tarjetas visibles
- WHEN el usuario toca una tarjeta
- THEN no hay detalle ni navegación

### Requirement: Período (IH-R3)

El sistema DEBE mostrar la etiqueta "Del {dd/MM/yyyy} al {dd/MM/yyyy}" usando `fechaInicio` y `fechaFin` devueltas por la API (no las fechas del selector).

#### Scenario: Etiqueta de periodo

- GIVEN la respuesta incluye `fechaInicio` y `fechaFin`
- THEN se muestra "Del {fechaInicio dd/MM/yyyy} al {fechaFin dd/MM/yyyy}"

### Requirement: Selector de fechas (IH-R4)

El sistema DEBE mostrar dos selectores de fecha (Desde / Hasta). Por defecto: Desde = primer día del mes actual y Hasta = hoy. Al cambiar una fecha DEBE recargar los indicadores. DEBE validar que Hasta no sea futura y que Desde no sea posterior a Hasta; si el rango es inválido, NO DEBE llamar a la API y DEBE mostrar un mensaje en español.

#### Scenario: Valores por defecto

- GIVEN Home aparece
- THEN Desde = primer día del mes actual y Hasta = hoy

#### Scenario: Cambio recarga

- GIVEN un rango válido
- WHEN el usuario cambia Desde o Hasta
- THEN se recargan los indicadores con el nuevo rango

#### Scenario: Hasta futura

- GIVEN el usuario selecciona Hasta posterior a hoy
- THEN no se llama a la API y se muestra un mensaje en español

#### Scenario: Desde posterior a Hasta

- GIVEN el usuario selecciona Desde posterior a Hasta
- THEN no se llama a la API y se muestra un mensaje en español

### Requirement: Estados (IH-R5)

El sistema DEBE manejar los estados de carga (spinner), error (mensaje en español con reintentar) y datos. El valor 0 DEBE mostrarse normalmente, no tratarse como vacío.

#### Scenario: Carga y error

- GIVEN la petición está en curso, THEN se muestra un spinner
- GIVEN falla la petición, THEN se muestra un mensaje en español con acción de reintentar

#### Scenario: Valor cero

- GIVEN un indicador vale 0
- THEN se muestra "0" formateado, no un espacio vacío

### Requirement: Refresco y sesión (IH-R6)

El sistema DEBE recargar con pull-to-refresh. Un 401 DEBE limpiar la sesión almacenada y regresar a `//login`. Errores de red/timeout/5xx DEBEN mostrar mensaje en español y nunca colapsar la app.

#### Scenario: Pull-to-refresh

- GIVEN Home visible
- WHEN el usuario hace pull-to-refresh
- THEN se recargan los indicadores

#### Scenario: Sesión expirada (401)

- GIVEN la API responde 401
- THEN se limpia la sesión y se regresa a `//login`

#### Scenario: Error de red/servidor

- GIVEN una falla de red, timeout o 5xx
- THEN se muestra un mensaje en español, sin colapsar la app
