# Delta for inicio-y-navegacion

## MODIFIED Requirements

### Requirement: Contenido del Flyout (IN-R2)

El sistema DEBE mostrar en el Flyout: el logo de la app, el nombre de usuario autenticado, al menos los ítems "Inicio", "Campañas" y "Cerrar sesión". El ítem "Campañas" DEBE ser visible solo cuando hay una sesión activa.
(Previously: el Flyout mostraba logo, usuario, "Inicio" y "Cerrar sesión"; no existía el ítem "Campañas".)

#### Scenario: Flyout muestra logo, usuario y acciones

- GIVEN hay una sesión activa
- WHEN se abre el menú hamburguesa
- THEN se ven el logo, el nombre de usuario autenticado, y los ítems "Inicio", "Campañas" y "Cerrar sesión"

## ADDED Requirements

### Requirement: Navegación a la lista de campañas (IN-R5)

El sistema DEBE registrar las rutas Shell de las vistas de campañas y DEBE navegar a la lista de campañas al tocar el ítem "Campañas" del Flyout. La navegación hacia atrás NO DEBE regresar a la pantalla de login.

#### Scenario: Tocar "Campañas" abre la lista

- GIVEN hay una sesión activa y el usuario abre el Flyout
- WHEN toca el ítem "Campañas"
- THEN se navega a la pantalla de lista de campañas

#### Scenario: Back no regresa al login

- GIVEN el usuario está en las vistas de campañas (lista/detalle/destinatarios)
- WHEN presiona el botón de retroceso
- THEN la app NO regresa a la pantalla de login
