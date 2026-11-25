# Análisis técnico de NEXUS 2.0

## 1. Introducción

NEXUS es un videojuego desarrollado en C# que contiene mecánicas de exploración, combate, inventario, administración de recursos y progreso de misiones. El código de consola analizado utiliza programación orientada a objetos para representar al cadete, las entidades, los objetos, las zonas y las misiones.

El objetivo de este análisis es evaluar las alternativas tecnológicas que pueden utilizarse para evolucionar el proyecto hacia una aplicación gráfica, organizada y preparada para almacenar partidas.

Las recomendaciones se basan en el código analizado y en documentación técnica oficial. Las tecnologías propuestas deberán verificarse durante su implementación.

## 2. Estado técnico actual

En el código analizado se identifican las siguientes características:

- Lenguaje C#.
- Aplicación de consola con interacción mediante texto.
- Clases abstractas, herencia y polimorfismo.
- Uso de colecciones genéricas para administrar objetos y zonas.
- Reglas de exploración, combate y progreso de misiones.
- Eventos aleatorios mediante la clase `Random`.
- Validaciones de entradas y condiciones de juego.

No se identifica en el código analizado una interfaz gráfica Windows Forms, una conexión implementada con SQL Server ni un sistema de guardado y recuperación de partidas.

Estas funcionalidades se consideran objetivos de evolución y no características terminadas.

## 3. Alternativas tecnológicas

### 3.1. Lenguaje de programación

**Alternativa A: C#**

Ventajas:
- Permite reutilizar las clases y la lógica existentes.
- Ofrece herramientas de programación orientada a objetos.
- Se integra con Windows Forms y con tecnologías de acceso a SQL Server.
- Cuenta con herramientas de desarrollo y depuración en Visual Studio.

Desventajas:
- La aplicación gráfica requiere reorganizar parte del código actual.
- La conexión con una base de datos requiere configuración y gestión de errores.

**Alternativa B: Java**

Ventajas:
- Ofrece programación orientada a objetos.
- Permite desarrollar aplicaciones gráficas y conectarse con bases de datos.
- Cuenta con herramientas de desarrollo y depuración.

Desventajas:
- Sería necesario trasladar las clases y adaptar la lógica del proyecto.
- La migración implicaría mantener dos implementaciones durante la transición.

**Alternativa C: Python**

Ventajas:
- Permite crear prototipos rápidamente.
- Cuenta con bibliotecas para interfaces gráficas y acceso a datos.

Desventajas:
- Requeriría reescribir la implementación actual.
- No ofrece la misma continuidad directa con las clases C# existentes.

**Decisión recomendada:** mantener C#, ya que permite aprovechar el trabajo existente y reducir el esfuerzo de migración.

## 4. Selección de la interfaz gráfica

### 4.1. Windows Forms

Windows Forms es una tecnología de interfaz gráfica para aplicaciones de escritorio en Windows. Permite utilizar formularios, botones, etiquetas, paneles y otros controles visuales, además de responder a eventos como los clics del usuario. La documentación oficial de Microsoft explica estas capacidades y el uso del diseñador visual de Visual Studio.

Ventajas:
- Integración con C# y Visual Studio.
- Creación visual de formularios.
- Manejo de eventos y controles.
- Adecuado para desarrollar una aplicación de escritorio con pantallas de inventario, misiones y estado del personaje.

Desventajas:
- Está orientado a Windows.
- El diseño y la lógica deben organizarse correctamente para evitar formularios con demasiadas responsabilidades.

### 4.2. Aplicación de consola

Ventajas:
- Es la interfaz utilizada por el código analizado.
- Permite ejecutar y probar las mecánicas sin construir formularios.
- Tiene una estructura sencilla para el prototipo inicial.

Desventajas:
- La interacción visual es limitada.
- No ofrece por sí sola la experiencia gráfica prevista para la siguiente etapa.

**Decisión recomendada:** utilizar Windows Forms para la evolución gráfica y conservar la lógica del juego en clases separadas de los formularios.

Referencia técnica: [Microsoft Learn — Windows Forms overview](https://learn.microsoft.com/en-us/dotnet/desktop/winforms/overview/).

## 5. Selección de la base de datos

### 5.1. SQL Server

SQL Server es la opción propuesta para almacenar las partidas, los personajes, los inventarios y el progreso de las misiones.

Ventajas:
- Permite organizar los datos mediante tablas y relaciones.
- Admite claves primarias y foráneas.
- Permite realizar operaciones transaccionales.
- Puede utilizarse desde C# mediante ADO.NET.

Desventajas:
- Requiere instalar o configurar una instancia de base de datos.
- Es necesario diseñar las tablas, relaciones y consultas.
- La aplicación debe gestionar errores de conexión, permisos y persistencia.

### 5.2. Archivos locales

Otra alternativa sería almacenar el progreso en archivos locales.

Ventajas:
- Puede resultar más sencilla para una primera prueba.
- No requiere necesariamente un servidor de base de datos.

Desventajas:
- La gestión de relaciones entre partidas, inventarios y misiones requiere lógica adicional.
- Las actualizaciones de varios datos relacionados deben controlarse cuidadosamente.

**Decisión recomendada:** utilizar SQL Server como objetivo de persistencia porque el diseño previsto requiere relaciones entre distintas entidades y operaciones de guardado y recuperación.

Referencia técnica: [Microsoft Learn — Transacciones y concurrencia en ADO.NET](https://learn.microsoft.com/en-us/sql/connect/ado-net/transactions-and-concurrency?view=sql-server-ver17).

## 6. Programación orientada a objetos

El código analizado ya utiliza principios de programación orientada a objetos:

- **Abstracción:** `Personaje` y `Entidad` representan estructuras comunes.
- **Encapsulamiento:** algunas propiedades, como la energía y los escudos del cadete, limitan la modificación directa mediante sus accesores.
- **Herencia:** `Cadete` y `Entidad` heredan de `Personaje`; `Robot` y `Enemigo` heredan de `Entidad`.
- **Polimorfismo:** los métodos virtuales y sobrescritos permiten comportamientos específicos en las clases derivadas.

Se recomienda conservar estos principios durante la evolución del proyecto. También conviene incorporar interfaces cuando existan contratos compartidos entre componentes que no necesiten pertenecer a la misma jerarquía de herencia.

Referencia técnica: [Microsoft Learn — Programación orientada a objetos en C#](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/tutorials/oop).

## 7. Separación de responsabilidades

Se recomienda organizar la solución en tres áreas:

1. **Presentación:** formularios Windows Forms y controles visuales.
2. **Lógica del juego:** reglas de exploración, combate, inventario, recursos y misiones.
3. **Acceso a datos:** conexión, consultas y operaciones de guardado y recuperación en SQL Server.

Esta organización permitirá modificar la interfaz sin tener que reescribir las reglas del juego y cambiar la persistencia sin introducir consultas SQL dentro de los formularios.

## 8. Seguridad y calidad

La futura implementación deberá considerar las siguientes medidas:

- Validar las entradas del usuario.
- Utilizar consultas SQL parametrizadas.
- Evitar publicar credenciales y cadenas de conexión con contraseñas en GitHub.
- Controlar las excepciones de las operaciones de base de datos.
- Utilizar transacciones para guardar conjuntos de datos relacionados.
- Probar las reglas de combate, inventario y progreso de las misiones.
- Verificar el guardado y la recuperación de una partida completa.
- Mantener mensajes de error comprensibles para el usuario.

Las pruebas deberán comprobar tanto los casos normales como las situaciones límite, por ejemplo, energía insuficiente, inventario vacío, selección inválida y fallos de conexión.

## 9. Plan de implementación recomendado

### Fase 1. Revisión y organización

- Confirmar los archivos y la solución de Visual Studio.
- Revisar las clases actuales y sus dependencias.
- Identificar las reglas que deben conservarse.

### Fase 2. Separación de la lógica

- Reducir la dependencia de las instrucciones de consola.
- Mantener las reglas del juego en clases independientes.
- Comprobar que la lógica siga funcionando.

### Fase 3. Interfaz gráfica

- Crear el formulario principal.
- Incorporar las pantallas de exploración, inventario, misiones y estado.
- Conectar los controles con la lógica del juego.

### Fase 4. Persistencia

- Crear la base de datos y sus tablas.
- Implementar el acceso a datos.
- Incorporar el guardado y la recuperación de partidas.
- Verificar la integridad de los datos.

### Fase 5. Pruebas y documentación

- Probar las mecánicas principales.
- Corregir errores.
- Actualizar el manual técnico y el README.
- Registrar los cambios mediante commits descriptivos en Git.

## 10. Verificación de la información técnica

Las recomendaciones se contrastaron con documentación oficial de Microsoft Learn sobre Windows Forms, programación orientada a objetos en C# y transacciones en ADO.NET.

Las fuentes permiten respaldar las capacidades generales de las tecnologías seleccionadas. No demuestran por sí mismas que NEXUS ya esté migrado, conectado a SQL Server o probado. Esas condiciones deben comprobarse sobre el código y el entorno real del proyecto.

## 11. Conclusión

Se recomienda continuar NEXUS con C#, Windows Forms y SQL Server. Esta combinación aprovecha el código existente, permite construir una interfaz gráfica para Windows y proporciona una opción relacional para la futura persistencia de las partidas.

La evolución debe realizarse progresivamente, separando la presentación, la lógica del videojuego y el acceso a los datos. El éxito de la propuesta dependerá de implementar y probar cada componente, manteniendo documentada la diferencia entre las funcionalidades existentes y las que todavía están planificadas.

## 12. Fuentes técnicas

1. Microsoft Learn. *Windows Forms overview*. https://learn.microsoft.com/en-us/dotnet/desktop/winforms/overview/
2. Microsoft Learn. *Object-oriented programming — C#*. https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/tutorials/oop
3. Microsoft Learn. *Transactions and concurrency — ADO.NET Provider for SQL Server*. https://learn.microsoft.com/en-us/sql/connect/ado-net/transactions-and-concurrency?view=sql-server-ver17

Fecha de consulta: 26 de octubre de 2026.

