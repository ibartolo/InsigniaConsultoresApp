# inicio-y-navegacion Specification

## Purpose

Pantalla Home post-autenticación con menú hamburguesa (Shell Flyout) y navegación que no regresa al login.

## Requirements

### Requirement: Home con menú hamburguesa

El sistema DEBE mostrar, tras la autenticación, una página en blanco con menú hamburguesa (`Shell Flyout`, `FlyoutBehavior="Flyout"`).

#### Scenario: Home tras login exitoso

- GIVEN el login es exitoso
- WHEN se navega al Home
- THEN se muestra una página en blanco con menú hamburguesa (Flyout)

### Requirement: Contenido del Flyout

El sistema DEBE mostrar en el Flyout: el logo de la app, el nombre de usuario autenticado, al menos los ítems "Inicio", "Campañas" y "Cerrar sesión". El ítem "Campañas" DEBE ser visible solo cuando hay una sesión activa.

#### Scenario: Flyout muestra logo, usuario y acciones

- GIVEN hay una sesión activa
- WHEN se abre el menú hamburguesa
- THEN se ven el logo, el nombre de usuario autenticado, y los ítems "Inicio", "Campañas" y "Cerrar sesión"

### Requirement: Área de contenido placeholder

El sistema DEBE mostrar en el área de contenido del Home la sección de indicadores (KPIs) definida en la spec `indicadores-home`, en lugar del área en blanco sin funcionalidad de negocio.

#### Scenario: Home muestra la sección de indicadores

- GIVEN el usuario está en Home
- WHEN observa el área de contenido
- THEN se muestra la sección de indicadores (KPIs) en lugar del placeholder en blanco

### Requirement: Navegación sin regreso al login

El sistema DEBE impedir que la navegación hacia atrás regrese a la pantalla de login después de autenticar.

#### Scenario: Back no regresa al login

- GIVEN el usuario ya está autenticado y en Home
- WHEN presiona el botón de retroceso
- THEN la app NO regresa a la pantalla de login

### Requirement: Navegación a la lista de campañas

El sistema DEBE registrar las rutas Shell de las vistas de campañas y DEBE navegar a la lista de campañas al tocar el ítem "Campañas" del Flyout. La navegación hacia atrás NO DEBE regresar a la pantalla de login.

#### Scenario: Tocar "Campañas" abre la lista

- GIVEN hay una sesión activa y el usuario abre el Flyout
- WHEN toca el ítem "Campañas"
- THEN se navega a la pantalla de lista de campañas

#### Scenario: Back no regresa al login

- GIVEN el usuario está en las vistas de campañas (lista/detalle/destinatarios)
- WHEN presiona el botón de retroceso
- THEN la app NO regresa a la pantalla de login
