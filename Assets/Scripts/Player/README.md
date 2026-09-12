# Where Cats Wait — Scripts de Personaje (Vertical Slice)

Paquete de mecánicas para Yui: movimiento/salto/gravedad y las dos primeras
habilidades felinas (Vista y Oído), basado en el GDD del TFM. HUD, rueda
radial e interfaces se quedan fuera a propósito — están pensados para que
los conectéis vosotros a los eventos que expone cada script.

## Estructura

```
Scripts/
  Movement/
    CharacterCollisionDetector.cs   → suelo, escalones y paredes (raycasts 3D)
    PlayerMovement.cs               → movimiento horizontal, salto, gravedad
  Abilities/
    FelineEvents.cs                 → tipos de UnityEvent para el Inspector
    FelineAbilityInterfaces.cs      → enum de estados + interfaces IFelineRevealable / IFelineHearable
    FelineAbilityBase.cs            → máquina de estados común (Recarga/Disponible/Activa)
    FelineVisionAbility.cs          → Vista Felina
    FelineHearingAbility.cs         → Oído Felino
    FelineAbilityController.cs      → input y selección de habilidad (rueda)
    Examples/
      FelineDetectableObject.cs     → ejemplo de huella/fragmento revelable
      FelineHearableSource.cs       → ejemplo de fuente sonora detectable
  Visuals/
    SpriteBillboard.cs              → opcional, orienta el sprite hacia cámara
```

## Por qué este diseño

- **Sin CharacterController**: tal y como comentaste, todo el movimiento es
  manual sobre un `Rigidbody` kinematic + `CapsuleCollider`, usando
  raycasts propios (`CharacterCollisionDetector`). Esto es justo la
  continuación en 3D de lo que ya veníamos depurando en la versión 2D.
- **Sprites en vez de rotar el modelo**: `PlayerMovement` cambia de
  dirección con `spriteRenderer.flipX`, no rotando el transform. Esto evita
  de raíz el bug que vimos antes (raycasts colapsando al centro de la
  cápsula al rotar el modelo para cambiar de dirección).
- **Ya incluye los fixes de gravedad y salto** que depuramos juntos: la
  gravedad se resetea siempre que hay suelo (no solo al caer), y el salto
  se calcula después de la gravedad en `FixedUpdate`.
- **Habilidades desacopladas del movimiento**: `FelineAbilityBase` es la
  única pieza que sabe de temporizadores y estados; `FelineVisionAbility` y
  `FelineHearingAbility` sólo implementan QUÉ detectan. Añadir garra,
  equilibrio, sigilo o salto felino más adelante es crear una nueva
  subclase de `FelineAbilityBase`, sin tocar el resto.
- **Todo lo que necesita el HUD sale por `UnityEvent`**: estado de cada
  habilidad, progreso de recarga, apertura/cierre de la rueda, selección
  actual. Ningún script de esta carpeta dibuja UI.

## Configuración en Unity

### 1. Input Actions necesarias
Añade estas acciones a tu Input Actions asset (o los mapas que ya tengas) y
arrástralas a los campos `InputActionReference` correspondientes:

| Acción            | Tipo    | Control sugerido      | Se usa en                  |
|--------------------|---------|------------------------|-----------------------------|
| Movement           | Axis 1D | A / D                  | PlayerMovement              |
| Jump                | Button  | Espacio                | PlayerMovement               |
| ActivateAbility     | Button  | Click izquierdo        | FelineAbilityController      |
| ToggleAbilityWheel  | Button  | Shift izq. / Click der.| FelineAbilityController      |
| CycleAbility        | Axis 1D | Rueda del ratón        | FelineAbilityController      |

(La tecla `E` de interacción y el movimiento de ratón para puzles no están
en este paquete — los dejamos fuera porque no eran parte de este pedido de
movimiento + vista/oído; si los queréis modularizados igual, decidme y os
paso `PlayerInteraction.cs` aparte.)

### 2. Capas (Layers)
- **Solid** (o el nombre que ya uséis para suelo): asígnala a suelo,
  paredes y escaleras, y ponla en `solidLayer` de
  `CharacterCollisionDetector`.
- **Detectable**: para huellas y fragmentos ocultos, en `detectableLayer`
  de `FelineVisionAbility`.
- **Hearable**: para fuentes sonoras ocultas, en `hearableLayer` de
  `FelineHearingAbility`. Puede ser la misma capa que Detectable si os
  resulta más simple gestionar una sola.

### 3. GameObject del jugador
```
Yui (Rigidbody, CapsuleCollider, CharacterCollisionDetector,
     PlayerMovement, FelineVisionAbility, FelineHearingAbility,
     FelineAbilityController, Animator)
 └── Sprite (SpriteRenderer [+ SpriteBillboard opcional])
```
En `PlayerMovement`, arrastra el `SpriteRenderer` del hijo. En
`FelineAbilityController`, arrastra `FelineVisionAbility` y
`FelineHearingAbility` a la lista `abilities` (en ese orden, así el índice
0 = Vista y 1 = Oído para `UnlockAbility`).

### 4. Objetos detectables de ejemplo
- Huella / fragmento: añade `FelineDetectableObject`, ponlo en la capa
  Detectable, y arrastra el objeto visual (partícula, glow...) a
  `visualEffect`.
- Fuente sonora oculta: añade `FelineHearableSource` (requiere
  `AudioSource`), ponlo en la capa Hearable, y asigna el `AudioClip` en el
  propio `AudioSource`.

### 5. Progresión de habilidades
Cuando restauréis el vínculo de un gato y corresponda desbloquear una
habilidad, llamad a `felineAbilityController.UnlockAbility(0)` (Vista) o
`UnlockAbility(1)` (Oído) desde vuestro sistema de narrativa/quests.

## Pendiente / fuera de alcance de este paquete
- Garra, equilibrio, sigilo y salto felino: la arquitectura ya está lista
  para ellas (nuevas subclases de `FelineAbilityBase`), pero no las he
  implementado porque pediste centrarnos solo en Vista y Oído para este
  vertical slice.
- HUD, rueda radial visual, indicador de ondas sonoras en pantalla: por
  diseño, os lo dejo a vosotros — los eventos (`OnStateChanged`,
  `OnProgressChanged`, `OnWheelToggled`, `OnAbilitySelected`,
  `OnFelineHearingDetected`...) ya están expuestos para engancharlos.
