# branding Specification

## Purpose

Establece la identidad visual de la app: ícono de aplicación, pantalla splash y paleta de colores de marca.

## Requirements

### Requirement: Ícono de aplicación y splash

El sistema DEBE reemplazar el ícono y el splash de la plantilla MAUI por defecto con el logo `logo.png` de la marca.

#### Scenario: Ícono de app usa el logo

- GIVEN el asset `logo.png` (600×600, sin alpha) existe en `C:\Git\InsigniaConsultoresAxaPaginaWeb\imagenes\marca\logo.png`
- WHEN se compila e instala la app
- THEN el ícono de aplicación muestra el logo, sin artefactos de la plantilla

#### Scenario: Splash usa el mismo logo

- GIVEN el asset `logo.png` está disponible
- WHEN la app arranca
- THEN la pantalla splash muestra el logo de marca

#### Scenario: Recorte del adaptive icon en Android (riesgo)

- GIVEN el logo es full-bleed, sin alpha y está centrado (no existe versión vectorial)
- WHEN Android recorta los bordes del adaptive icon (~10%)
- THEN el logo permanece centrado y legible, y el recorte es aceptable (riesgo documentado)

### Requirement: Paleta de colores de marca

El sistema DEBE definir la siguiente paleta en los recursos de estilos (`Colors.xaml`):

| Token | Valor |
|-------|-------|
| Primary | `#0F4C81` |
| Dark | `#0A3A63` |
| Light | `#1B6BA8` |
| Orange | `#E8952B` |
| OrangeDark | `#C97A18` |
| Background | `#FDF8F1` |
| TextBody | `#4A5568` |
| InputBackground | `#fbfdff` |
| InputBorder | `#dde5ee` |
| Surface | `#FFFFFF` |
| CardBorder | `#eef2f7` |

#### Scenario: Colores disponibles en toda la app

- GIVEN la app arranca
- WHEN cualquier vista resuelve un color de marca desde los recursos
- THEN el color definido en la paleta está disponible sin valores hardcodeados

### Requirement: Colores nativos de Android (barra de estado)

El sistema DEBE reemplazar los colores nativos de la plantilla MAUI en `Platforms/Android/Resources/values/colors.xml` (púrpura `#512BD4`) por la paleta de marca: `colorPrimary` `#0F4C81`, `colorPrimaryDark` `#0A3A63` y `colorAccent` `#E8952B`, para evitar el flash púrpura en la barra de estado/splash.

#### Scenario: Sin flash púrpura en Android

- GIVEN la app se compila e instala en Android
- WHEN arranca la app o se muestra la barra de estado
- THEN no aparece ningún color púrpura de plantilla (`#512BD4`) en la barra de estado ni en el splash

### Requirement: Sistema visual de componentes

El sistema DEBE usar los siguientes valores de diseño de marca:

| Elemento | Valor |
|----------|-------|
| Radio (componentes) | `14px` |
| Radio (pills) | `999px` |
| Sombra | `0 8px 28px rgba(15,76,129,.12)` |
| Separador de input | línea inferior `1px #dde5ee` (inputs sin caja/borde) |
| Foco de input | línea inferior `2px #E8952B` |
| Hero gradient | `linear-gradient(120deg,#0A3A63 0%,#0F4C81 55%,#1B6BA8 100%)` |
| CTA gradient | `linear-gradient(120deg,#E8952B 0%,#C97A18 100%)` |
| Fuente | Segoe UI / system stack |

#### Scenario: Componentes usan los valores de marca

- GIVEN una vista usa un componente con los estilos de marca
- WHEN se renderiza
- THEN se aplican el radio, la sombra y la tipografía definidos en el sistema visual
