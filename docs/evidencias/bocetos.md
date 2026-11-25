# Evidencias de diseño — wireframes de NEXUS

Los siguientes bocetos son esquemas de baja fidelidad para explicar qué verá el jugador y cómo se moverá por la interfaz. No son capturas ni formularios implementados. Propuesta de ventana de escritorio redimensionable; el panel central cambia de vista, mientras el estado básico permanece visible.

## 1. Menú principal

```text
┌──────────────────────────── NEXUS ──────────────────────────────┐
│                    ECOS DEL VACÍO · AÑO 2297                   │
│                                                                 │
│              Una anomalía abrió una puerta a otra realidad.    │
│              Recupera el núcleo y vuelve a extracción.         │
│                                                                 │
│                       [ Nueva expedición ]                      │
│                       [ Cargar partida   ]                      │
│                       [ Opciones         ]                      │
│                       [ Salir            ]                      │
│                                                                 │
│  Estado: sin partida activa                     Versión: H3→UI  │
└─────────────────────────────────────────────────────────────────┘
```

**Acciones:** Nueva expedición pide nombre de cadete y crea estado inicial. Cargar abre ranuras con nombre y fecha. Opciones controla tamaño de texto/sonido cuando exista. Salir confirma si hay progreso no guardado. Desde menú se entra a Exploración, y desde una partida se vuelve al menú con guardado confirmado.

## 2. Exploración / mapa

```text
┌ NEXUS ─ EXPEDICIÓN ─────────────────────────────────────────────┐
│ Cadete: LUIS      Salud ████████░░ 80/100  Energía ████░░ 42/100│
│ Resonancia: 45/100 · moderada                    XP: 30        │
├──────────────────────┬─────────────────────────────────────────┤
│ MAPA                 │ GRIETA DE RESONANCIA                     │
│ [Extracción]         │ La realidad se fragmenta por la anomalía.│
│       │              │                                         │
│ [Bosque]             │ Registro                                 │
│       │              │  ▸ Señal anómala localizada (+objetivo)  │
│ [Laboratorio]        │  ▸ Resonancia +20                        │
│       │              │                                         │
│ >[Grieta]<           │ [ Explorar · 10 energía ] [ Analizar ]   │
│       │              │ [ Viajar a Laboratorio ]                │
│ [Núcleo: bloqueado]  │                                         │
├──────────────────────┴─────────────────────────────────────────┤
│ [Misiones] [Inventario] [Guardar] [Menú]                        │
└─────────────────────────────────────────────────────────────────┘
```

**Elementos:** estado persistente; cinco nodos y conexiones; descripción de zona; registro de eventos; costo de explorar; botones de explorar/analizar/viajar. El destino no adyacente o bloqueado aparece deshabilitado con explicación. Viajar actualiza la zona; explorar actualiza recursos y registro. En el Núcleo se informa que la anomalía debe investigarse antes de habilitarlo.

## 3. Misiones

```text
┌ NEXUS ─ OBJETIVOS DE EXPEDICIÓN ────────────────────────────────┐
│ La anomalía perdida                         Progreso: 2 de 3    │
│ “Recupera el núcleo energético y regresa con vida.”             │
│                                                                 │
│  ✓ Investigar la Grieta de Resonancia                           │
│  ✓ Recuperar el núcleo en el centro de la anomalía              │
│  ○ Sobrevivir al encuentro con una entidad                      │
│                                                                 │
│  Siguiente paso: explora hasta encontrar un encuentro y vuelve  │
│  al Punto de Extracción con el núcleo.                           │
│                                                                 │
│ Recompensa al completar: 100 XP + Módulo desconocido             │
│ [ Volver al mapa ]                          [ Guardar partida ]  │
└─────────────────────────────────────────────────────────────────┘
```

**Navegación y retroalimentación:** se abre desde la barra inferior. Cada objetivo muestra pendiente/completo con icono y texto; el porcentaje nunca es el único indicador. El botón de siguiente paso lleva de vuelta al mapa y resalta la zona sugerida. La recompensa se presenta como futura mientras esté pendiente y como recibida al completar.

## 4. Exploradores / inventario

```text
┌ NEXUS ─ CADETE E INVENTARIO ────────────────────────────────────┐
│ CADETE LUIS                  SALUD 80/100   ENERGÍA 42/100      │
│ Ataque 30   Escudos 1        Experiencia 30 XP                  │
├───────────────────────┬────────────────────────────────────────┤
│ OBJETOS (4)           │ DETALLE: Célula de energía              │
│ [◇] Célula de energía x2│ Recupera 25 puntos de energía.       │
│ [⬡] Escudo x1         │                                         │
│ [△] Analizador x1     │ [ Usar · recupera hasta 25 ]             │
│ [◆] Núcleo energético│                                         │
│                       │ Objetos de misión no consumibles.        │
│                       │ Núcleo protegido contra eliminación.     │
├───────────────────────┴────────────────────────────────────────┤
│ [ Misiones ] [ Mapa / Exploración ] [ Guardar ] [ Volver ]      │
└─────────────────────────────────────────────────────────────────┘
```

**Elementos e interacción:** atributos del cadete y experiencia arriba; lista de objetos con cantidad, símbolo y nombre; detalle/efecto a la derecha; usar objeto muestra costo/resultado. El núcleo y módulo desconocido se distinguen como elementos de misión y no ofrecen “Usar” o “Eliminar”. “Volver” retorna a la pantalla anterior.

## Navegación global

- Menú → Nueva expedición → Exploración.
- Exploración ↔ Misiones / Inventario; las vistas laterales conservan la expedición y permiten volver al mapa.
- Exploración → Encuentro cuando aparece una entidad; atacar, escapar, analizar o usar objeto devuelven el resultado al registro.
- Guardar desde la barra abre confirmación de ranura; resultado visible de guardado y error accionable si SQL no está disponible.
- Menú/Salir desde partida confirma guardado o abandono para proteger el progreso.
