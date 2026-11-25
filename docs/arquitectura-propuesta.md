# Arquitectura propuesta de NEXUS 2.0

## 1. Introducción

La arquitectura propuesta para NEXUS 2.0 busca evolucionar el videojuego de consola hacia una aplicación gráfica desarrollada en C# con Windows Forms y preparada para almacenar los datos de las partidas en SQL Server.

La propuesta conserva las mecánicas principales del juego, como la exploración, los encuentros, el inventario, la gestión de recursos y el progreso de las misiones. Al mismo tiempo, organiza el código en componentes separados para facilitar su mantenimiento, ampliación y prueba.

Esta arquitectura representa un diseño futuro. Su implementación deberá realizarse y verificarse durante las siguientes etapas del proyecto.

## 2. Objetivos de la arquitectura

- Incorporar una interfaz gráfica más intuitiva.
- Separar la presentación visual de las reglas del videojuego.
- Reutilizar las clases y mecánicas existentes siempre que sea posible.
- Facilitar la incorporación de nuevas misiones, zonas, objetos y enemigos.
- Preparar la conexión con SQL Server para guardar y recuperar partidas.
- Mejorar el mantenimiento, la organización y las pruebas del código.

## 3. Arquitectura por capas

Se propone dividir el sistema en tres capas principales.

### 3.1. Capa de presentación

**Tecnología propuesta:** Windows Forms.

Será responsable de mostrar la información y recibir las acciones del jugador. La interfaz podrá incluir ventanas, botones, etiquetas, paneles e imágenes.

Formularios propuestos:

- `FrmMenuPrincipal`: acceso a las opciones principales.
- `FrmExploracion`: visualización de las zonas y acciones de exploración.
- `FrmMisiones`: objetivos y progreso de las misiones.
- `FrmInventario`: objetos disponibles y acciones permitidas.
- `FrmEstadoCadete`: salud, energía, experiencia y recursos.
- `FrmCombate`: encuentros con robots y enemigos.

Los formularios comunicarán las acciones del jugador a la lógica del juego, evitando concentrar todas las reglas dentro de los eventos de los botones.

### 3.2. Capa de lógica del juego

Será responsable de aplicar las reglas y actualizar el estado de la partida.

Clases propuestas:

- `JuegoNexus`: coordina el estado general de la partida.
- `Cadete`: administra las características del protagonista.
- `Personaje`: establece los atributos y comportamientos comunes.
- `Entidad`: define la estructura común de las entidades.
- `Robot` y `Enemigo`: implementan comportamientos específicos.
- `Inventario`: administra los objetos del jugador.
- `Zona`: representa las áreas explorables.
- `Mision`: controla los objetivos y las condiciones de finalización.
- `ServicioCombate`: centraliza las reglas de combate.
- `ServicioExploracion`: administra las reglas y los eventos de exploración.

Las clases existentes podrán reutilizarse o reorganizarse según las necesidades de la interfaz gráfica y las pruebas.

### 3.3. Capa de acceso a datos

**Tecnología propuesta:** SQL Server y ADO.NET.

Esta capa gestionará las operaciones de guardado y recuperación de datos. Se propone utilizar componentes como:

- `ConexionBD`: administra la configuración de conexión.
- `PartidaRepository`: guarda y consulta las partidas.
- `PersonajeRepository`: almacena los datos del cadete.
- `InventarioRepository`: administra los objetos asociados a una partida.
- `MisionRepository`: registra el progreso de las misiones.

Estas clases son propuestas de diseño y deberán implementarse antes de considerarse funcionalidades disponibles.

## 4. Diagrama de arquitectura

```text
+--------------------------------------+
|         CAPA DE PRESENTACIÓN         |
|             Windows Forms            |
|                                      |
| Menú | Exploración | Inventario      |
| Misiones | Estado | Combate          |
+------------------+-------------------+
                   |
                   v
+--------------------------------------+
|          CAPA DE LÓGICA              |
|                                      |
| JuegoNexus                            |
| Cadete / Personaje / Entidad         |
| Robot / Enemigo                      |
| Inventario / Zona / Mision           |
| ServicioCombate / ServicioExploracion|
+------------------+-------------------+
                   |
                   v
+--------------------------------------+
|        CAPA DE ACCESO A DATOS         |
|                                      |
| Repositorios | Conexión | Consultas  |
|                 ADO.NET              |
+------------------+-------------------+
                   |
                   v
+--------------------------------------+
|              SQL SERVER              |
|                                      |
| Partidas | Personajes | Inventarios  |
| Misiones | Progreso                  |
+--------------------------------------+
```

## 5. Comunicación entre las capas

El funcionamiento previsto será el siguiente:

1. El jugador realiza una acción desde un formulario.
2. El formulario solicita la operación correspondiente a la lógica del juego.
3. La lógica valida las condiciones y actualiza el estado de la partida.
4. Si la operación requiere persistencia, se solicita a la capa de acceso a datos que guarde o consulte la información.
5. La capa de datos ejecuta las operaciones necesarias en SQL Server.
6. El resultado vuelve a la lógica y se presenta al jugador mediante la interfaz gráfica.

La interfaz no deberá ejecutar directamente las consultas SQL ni contener todas las reglas del videojuego.

## 6. Flujo propuesto para guardar y recuperar partidas

### Guardar una partida

1. El jugador selecciona la opción de guardar.
2. El sistema obtiene el estado actual de la partida.
3. Se recopilan los datos del cadete, la zona, el inventario y el progreso de la misión.
4. La capa de acceso a datos registra la información en SQL Server.
5. El sistema informa si la operación finalizó correctamente.

### Recuperar una partida

1. El jugador selecciona una partida guardada.
2. El sistema consulta los datos almacenados.
3. Se reconstruyen los objetos del juego y su estado.
4. Se restauran la zona actual, los recursos, el inventario y el progreso.
5. La interfaz presenta la partida recuperada.

La recuperación deberá validar que los datos relacionados correspondan a la misma partida y que puedan reconstruirse correctamente.

## 7. Ventajas de la propuesta

- **Mantenimiento:** cada capa tiene responsabilidades delimitadas.
- **Reutilización:** las clases actuales pueden servir como base de la nueva versión.
- **Escalabilidad:** es posible incorporar mecánicas sin concentrar todo el código en un único componente.
- **Persistencia:** SQL Server permitirá almacenar información entre ejecuciones cuando se implemente.
- **Experiencia de usuario:** Windows Forms permitirá una interacción visual mediante controles gráficos.
- **Pruebas:** la lógica podrá verificarse con mayor independencia de los formularios y la base de datos.

## 8. Consideraciones técnicas

La migración debe realizarse progresivamente. Primero se debe separar la lógica de las instrucciones de consola; después se implementarán los formularios y, finalmente, el acceso a datos y las operaciones de persistencia.

Las operaciones con SQL Server deberán utilizar consultas parametrizadas y un manejo adecuado de errores. Las conexiones y los recursos deben liberarse correctamente, y las credenciales no deben publicarse en el repositorio.

También será necesario definir cómo se identificará cada partida y cómo se relacionarán sus personajes, objetos y misiones para evitar inconsistencias.

## 9. Conclusión

La arquitectura propuesta establece una base para transformar NEXUS en una aplicación gráfica organizada y ampliable. La separación entre presentación, lógica del juego y acceso a datos permitirá reducir el acoplamiento y facilitar futuras modificaciones.

Windows Forms será responsable de la interacción visual; la lógica conservará las reglas principales del videojuego; y SQL Server ofrecerá persistencia cuando se implemente la capa de datos.

La propuesta deberá validarse mediante la implementación progresiva, pruebas funcionales y comprobaciones de guardado y recuperación antes de considerar completada la evolución técnica de NEXUS 2.0.
