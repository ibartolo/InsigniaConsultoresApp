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

El sistema DEBE mostrar en el Flyout: el logo de la app, el nombre de usuario autenticado, al menos un ítem "Inicio" y "Cerrar sesión".

#### Scenario: Flyout muestra logo, usuario y acciones

- GIVEN hay una sesión activa
- WHEN se abre el menú hamburguesa
- THEN se ven el logo, el nombre de usuario autenticado, el ítem "Inicio" y "Cerrar sesión"

### Requirement: Área de contenido placeholder

El sistema DEBE mostrar un área de contenido en blanco (placeholder), sin funcionalidad de negocio.

#### Scenario: Home sin contenido de negocio

- GIVEN el usuario está en Home
- WHEN observa el área de contenido
- THEN el área está en blanco (placeholder), sin contenido de negocio

### Requirement: Navegación sin regreso al login

El sistema DEBE impedir que la navegación hacia atrás regrese a la pantalla de login después de autenticar.

#### Scenario: Back no regresa al login

- GIVEN el usuario ya está autenticado y en Home
- WHEN presiona el botón de retroceso
- THEN la app NO regresa a la pantalla de login
