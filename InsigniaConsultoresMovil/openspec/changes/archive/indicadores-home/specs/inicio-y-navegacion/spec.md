# Delta for inicio-y-navegacion

## MODIFIED Requirements

### Requirement: Área de contenido placeholder

El sistema DEBE mostrar en el área de contenido del Home la sección de indicadores (KPIs) definida en la spec `indicadores-home`, en lugar del área en blanco sin funcionalidad de negocio.
(Previously: el área de contenido estaba en blanco (placeholder), sin funcionalidad de negocio.)

#### Scenario: Home muestra la sección de indicadores

- GIVEN el usuario está en Home
- WHEN observa el área de contenido
- THEN se muestra la sección de indicadores (KPIs) en lugar del placeholder en blanco
