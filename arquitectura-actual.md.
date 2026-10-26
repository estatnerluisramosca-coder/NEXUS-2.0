# Arquitectura actual de NEXUS

## 1. Introducción

Este documento describe la arquitectura actual de NEXUS a partir del código fuente C# analizado del videojuego de consola *Ecos del Vacío*. Se identifican las clases principales, sus responsabilidades, el funcionamiento general y las limitaciones que deben considerarse para una futura evolución del proyecto.

El análisis se basa en el código proporcionado. Las funcionalidades que no aparecen implementadas en ese código se identifican como pendientes, sin asumir que ya existen en otras versiones del proyecto.

## 2. Tecnologías utilizadas

La versión analizada utiliza las siguientes tecnologías y recursos:

- **Lenguaje:** C#.
- **Entorno de ejecución:** aplicación de consola.
- **Entrada y salida:** instrucciones de la clase `Console`.
- **Colecciones:** `List<T>` para almacenar objetos y zonas.
- **Programación orientada a objetos:** clases, herencia, abstracción, encapsulamiento y polimorfismo.
- **Control del programa:** condicionales, ciclos, selección de opciones y validaciones.
- **Aleatoriedad:** clase `Random` para generar eventos durante la exploración y los encuentros.

En el código analizado, la interacción con el jugador ocurre mediante texto en la consola. No se observa una interfaz gráfica de Windows Forms ni una conexión implementada con SQL Server.

## 3. Clases principales y responsabilidades

### 3.1. `Program`

Es el punto de entrada de la aplicación. Su método `Main()` establece el título de la consola, crea una instancia de `JuegoNexus` e inicia la ejecución mediante `Iniciar()`.

### 3.2. `Personaje`

Es una clase abstracta que representa las características comunes de los personajes. Contiene propiedades como `Nombre`, `Salud`, `Ataque` y `Vivo`, además de un constructor y el método virtual `RecibirDanio()`.

Esta clase permite compartir comportamientos entre el cadete y las entidades que aparecen en el juego.

### 3.3. `Cadete`

Hereda de `Personaje` y representa al protagonista controlado por el jugador. Incorpora energía, experiencia y escudos, además de operaciones para recuperar energía, curarse, mejorar el ataque, ganar experiencia y utilizar escudos.

También redefine el comportamiento de recepción de daño para considerar la protección proporcionada por los escudos.

### 3.4. `Entidad`

Es una clase abstracta derivada de `Personaje`. Añade una descripción y declara los métodos abstractos `Presentarse()` y `Atacar(Cadete)`.

Su propósito es establecer una estructura común para los distintos tipos de entidades del juego.

### 3.5. `Robot` y `Enemigo`

Ambas clases heredan de `Entidad` y representan encuentros diferentes.

- `Robot`: representa al robot VIGIA-07.
- `Enemigo`: representa al ACECHADOR DEL VACÍO.

Las clases implementan sus propios comportamientos de presentación y ataque. Esto permite utilizar el polimorfismo para tratar diferentes entidades mediante una estructura común.

### 3.6. `Objeto`

Representa los objetos que el jugador puede encontrar o utilizar. Contiene información como el nombre y la descripción del objeto.

### 3.7. `Inventario`

Administra la colección de objetos del cadete mediante una lista genérica. Incluye operaciones para agregar, obtener, comprobar, eliminar, consumir y mostrar objetos.

El código también establece restricciones para impedir la eliminación o el consumo de determinados objetos importantes, como el núcleo energético.

### 3.8. `Zona`

Representa una zona explorable del videojuego mediante su nombre y descripción. Las zonas sirven para organizar el escenario y determinar dónde pueden ocurrir determinados eventos.

### 3.9. `Mision`

Administra el progreso de la misión mediante indicadores que registran si se investigó la anomalía, se encontró el núcleo y se sobrevivió al encuentro.

El método `PuedeCompletar()` comprueba los requisitos establecidos. Cuando se completa la misión, el método correspondiente registra la finalización y otorga experiencia.

### 3.10. `JuegoNexus`

Es la clase que coordina la ejecución general del juego. Gestiona al cadete, el inventario, la misión, las zonas, la zona actual, la inestabilidad y el estado de la partida.

También administra el menú principal, la exploración, los desplazamientos, los encuentros, el análisis de zonas, la visualización del estado y la finalización de la partida.

## 4. Funcionamiento general del programa

El flujo de ejecución puede resumirse de la siguiente manera:

1. Se ejecuta `Program.Main()`.
2. Se crea una instancia de `JuegoNexus`.
3. Se inicializan los elementos principales de la partida y las zonas disponibles.
4. Se muestra el menú de la consola.
5. El jugador selecciona una acción.
6. El sistema comprueba las condiciones necesarias para ejecutar esa acción.
7. Dependiendo de la opción, se realiza una exploración, un desplazamiento, una consulta del inventario, una revisión del estado, un análisis de la zona o un encuentro.
8. Se actualizan los recursos, el progreso de la misión y las condiciones de la partida.
9. El ciclo continúa mientras la partida permanezca activa.
10. Al finalizar, el sistema informa el resultado de la partida y la experiencia obtenida.

## 5. Mecánicas identificadas

### Exploración

El jugador puede explorar las zonas disponibles. La exploración requiere un mínimo de energía y puede producir diferentes eventos, como encontrar objetos, sufrir daños, perder energía, descubrir información o enfrentarse a una entidad.

### Movimiento entre zonas

El sistema valida la opción seleccionada, comprueba que el destino exista y limita el desplazamiento a las zonas permitidas. También establece una condición para acceder a la zona del núcleo de la anomalía.

### Inventario

El jugador puede consultar sus objetos y ejecutar acciones permitidas sobre ellos. El sistema comprueba la existencia de los objetos y aplica restricciones a determinados elementos.

### Encuentros

Durante algunos eventos pueden aparecer robots o enemigos. El sistema ofrece opciones de combate, escape, análisis y uso de objetos, sujetas a las condiciones correspondientes.

### Progreso de la misión

La misión depende de varias condiciones relacionadas con la investigación de la anomalía, el hallazgo del núcleo y la supervivencia al encuentro. Al cumplirse los requisitos, se puede completar y otorgar experiencia.

### Inestabilidad

El juego utiliza una variable de inestabilidad que influye en las probabilidades de determinados eventos y puede aumentar durante la exploración. Este mecanismo contribuye a que los resultados no sean completamente predecibles.

## 6. Validaciones y control del programa

En el código analizado se observan mecanismos de control como:

- Comprobación de energía antes de explorar.
- Validación de opciones y posiciones de destino.
- Uso de `int.TryParse()` para comprobar entradas numéricas.
- Condiciones para limitar el acceso a determinadas zonas.
- Comprobaciones del estado del personaje.
- Restricciones sobre objetos que no pueden eliminarse o consumirse.
- Ciclos y condicionales para controlar el menú y los eventos.
- Uso de `Random` para introducir variabilidad en la partida.

Estas validaciones permiten controlar acciones no permitidas y reducir errores derivados de las entradas del jugador.

## 7. Limitaciones de la arquitectura actual

La versión analizada presenta las siguientes limitaciones:

1. **Interfaz de consola:** la interacción depende de texto y opciones numéricas, por lo que todavía no ofrece una experiencia gráfica.
2. **Responsabilidades concentradas:** `JuegoNexus` coordina gran parte de las mecánicas, lo que puede dificultar su mantenimiento si aumenta la complejidad.
3. **Persistencia no implementada en el código analizado:** no se identifica una base de datos ni un mecanismo de guardado y restauración de partidas.
4. **Separación de capas limitada:** la lógica del juego y la interacción con el usuario están estrechamente relacionadas.
5. **Pruebas automatizadas no identificadas:** en el código proporcionado no se observan pruebas unitarias.
6. **Interfaces no identificadas:** no se observan interfaces de C# implementadas en el código analizado.
7. **LINQ no identificado:** las operaciones sobre colecciones se realizan mediante los mecanismos presentes en el código, sin uso visible de LINQ.

Además, la lógica que registra la supervivencia al encuentro debe revisarse: en el código analizado, la misión registra el encuentro si el cadete continúa vivo después del evento. Por ello, escapar podría contabilizarse como supervivencia. Es necesario confirmar si ese comportamiento coincide con las reglas deseadas para la misión.

Estas observaciones se limitan al código revisado y no permiten afirmar que las funcionalidades mencionadas estén ausentes de todas las versiones del proyecto.

## 8. Conclusión

La arquitectura actual de NEXUS cuenta con una base funcional de programación orientada a objetos. El uso de clases abstractas, herencia, encapsulamiento y polimorfismo permite organizar personajes y entidades, mientras que las listas genéricas facilitan la administración del inventario y las zonas.

La clase `JuegoNexus` integra las mecánicas principales y coordina el flujo de la partida. Sin embargo, para evolucionar el proyecto hacia una aplicación gráfica más mantenible, conviene separar la interfaz, las reglas del juego y el acceso a los datos.

La siguiente etapa propuesta consiste en diseñar una arquitectura por capas que permita incorporar Windows Forms y, posteriormente, persistencia con SQL Server sin mezclar estas responsabilidades con la lógica principal del videojuego.

## 9. Alcance de este documento

Este documento describe la versión de código de consola analizada. Las funcionalidades de interfaz gráfica, conexión con SQL Server y guardado o recuperación de partidas se consideran propuestas de evolución, no características implementadas que se hayan verificado en esta versión.
