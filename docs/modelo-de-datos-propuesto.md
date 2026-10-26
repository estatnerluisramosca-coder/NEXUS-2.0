# Modelo de datos propuesto para NEXUS 2.0

## 1. Introducción

El presente documento define una propuesta de modelo de datos para NEXUS 2.0, cuyo objetivo es permitir el almacenamiento y la recuperación del progreso del jugador mediante SQL Server.

En la versión de consola analizada, el estado de la partida se administra principalmente mediante objetos y colecciones en memoria. Por ello, se propone incorporar una base de datos relacional que permita conservar la información entre ejecuciones del programa.

Este modelo es una propuesta técnica y deberá implementarse y probarse antes de considerarse funcional.

## 2. Objetivos

- Guardar y recuperar partidas del videojuego.
- Registrar los recursos y las características del cadete.
- Conservar los objetos del inventario.
- Almacenar la zona actual y el estado de la misión.
- Mantener relaciones consistentes entre los datos.
- Facilitar la ampliación del juego con nuevas zonas, objetos y misiones.

## 3. Tecnología propuesta

**Sistema gestor de base de datos:** Microsoft SQL Server.

**Acceso desde C#:** ADO.NET mediante conexiones y comandos SQL parametrizados.

Se propone utilizar una base de datos relacional porque las entidades del juego tienen relaciones definidas. Por ejemplo, una partida tiene un cadete, puede contener varios objetos en el inventario y registra el progreso de una misión.

## 4. Entidades principales

### 4.1. Partidas

Almacena la información general de cada partida guardada.

Campos propuestos:

- `PartidaId`: identificador único de la partida.
- `NombrePartida`: nombre asignado a la partida guardada.
- `FechaCreacion`: fecha de creación del registro.
- `FechaActualizacion`: última fecha de actualización.
- `ZonaActualId`: zona en la que se encuentra el jugador.
- `Inestabilidad`: valor de inestabilidad del sistema.
- `EstadoPartida`: estado general, como activa, completada o interrumpida.

### 4.2. Personajes

Almacena las características del cadete asociado a una partida.

Campos propuestos:

- `PersonajeId`: identificador del personaje.
- `PartidaId`: partida a la que pertenece.
- `Nombre`: nombre del cadete.
- `Salud`: salud actual.
- `Ataque`: capacidad de ataque.
- `Energia`: energía disponible.
- `Experiencia`: experiencia acumulada.
- `Escudos`: cantidad de escudos disponibles.

La relación con `Partidas` permite identificar a qué partida corresponde cada personaje.

### 4.3. Zonas

Contiene la información de las zonas del videojuego.

Campos propuestos:

- `ZonaId`: identificador de la zona.
- `Nombre`: nombre de la zona.
- `Descripcion`: información descriptiva.
- `OrdenZona`: posición de la zona dentro de la progresión del juego.

Esta tabla permite organizar las zonas sin duplicar sus nombres y descripciones en cada partida.

### 4.4. Objetos

Registra los objetos disponibles en el videojuego.

Campos propuestos:

- `ObjetoId`: identificador del objeto.
- `Nombre`: nombre del objeto.
- `Descripcion`: descripción del objeto.
- `TipoObjeto`: categoría del objeto.
- `EsObjetoClave`: indica si el objeto tiene una función importante para la misión.

Entre los objetos que deben contemplarse se encuentran las células de energía, los escudos, los módulos de ataque, los analizadores y el núcleo energético, según las mecánicas identificadas en el código analizado.

### 4.5. InventarioPartida

Relaciona los objetos con cada partida y registra las cantidades disponibles.

Campos propuestos:

- `InventarioId`: identificador del registro.
- `PartidaId`: partida propietaria del inventario.
- `ObjetoId`: objeto almacenado.
- `Cantidad`: cantidad disponible.

Esta tabla resuelve la relación de muchos a muchos entre partidas y objetos: una partida puede tener varios tipos de objetos y un mismo tipo de objeto puede aparecer en muchas partidas.

### 4.6. Misiones

Contiene la definición de las misiones del videojuego.

Campos propuestos:

- `MisionId`: identificador de la misión.
- `Nombre`: nombre de la misión.
- `Descripcion`: objetivo general.
- `ExperienciaRecompensa`: experiencia otorgada al completarla.

### 4.7. ProgresoMision

Registra el avance de las misiones dentro de una partida específica.

Campos propuestos:

- `ProgresoId`: identificador del progreso.
- `PartidaId`: partida correspondiente.
- `MisionId`: misión asociada.
- `AnomaliaInvestigada`: indica si se investigó la anomalía.
- `NucleoEncontrado`: indica si se encontró el núcleo.
- `EncuentroSobrevivido`: registra el cumplimiento de la condición del encuentro.
- `Completada`: indica si se completó la misión.

Esta entidad permite conservar los indicadores que la clase `Mision` administra en memoria.

## 5. Relaciones entre las tablas

Las relaciones propuestas son las siguientes:

- Una partida puede tener un personaje asociado.
- Una partida registra una zona actual.
- Una partida puede contener varios objetos mediante `InventarioPartida`.
- Un objeto puede aparecer en los inventarios de muchas partidas.
- Una partida puede registrar el progreso de varias misiones.
- Una misión puede tener registros de progreso en distintas partidas.
- La zona actual de una partida referencia una zona existente.

Las claves primarias identifican cada registro y las claves foráneas permiten mantener la integridad de las relaciones.

## 6. Diagrama entidad-relación

```text
ZONAS
  |
  | 1
  | 
  | N
PARTIDAS -------- 1:1 -------- PERSONAJES
  |
  | 1
  |
  | N
INVENTARIO_PARTIDA
  |
  | N
  |
  | 1
OBJETOS


PARTIDAS
  |
  | 1
  |
  | N
PROGRESO_MISION
  |
  | N
  |
  | 1
MISIONES
```

El diagrama representa las relaciones previstas. La cardinalidad de una partida y su personaje se propone como uno a uno, mientras que las relaciones con objetos y el progreso de misiones son de uno a muchos mediante sus tablas correspondientes.

## 7. Ejemplo de estructura SQL propuesta

El siguiente código ilustra cómo podría definirse una parte del modelo en SQL Server. No representa una base de datos ya creada ni conectada al videojuego.

```sql
CREATE DATABASE NexusDB;
GO

USE NexusDB;
GO

CREATE TABLE Zonas (
    ZonaId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(500),
    OrdenZona INT NOT NULL
);

CREATE TABLE Partidas (
    PartidaId INT IDENTITY(1,1) PRIMARY KEY,
    NombrePartida NVARCHAR(100) NOT NULL,
    FechaCreacion DATETIME2 NOT NULL
        DEFAULT SYSDATETIME(),
    FechaActualizacion DATETIME2 NOT NULL
        DEFAULT SYSDATETIME(),
    ZonaActualId INT NOT NULL,
    Inestabilidad INT NOT NULL DEFAULT 0,
    EstadoPartida NVARCHAR(30) NOT NULL,
    CONSTRAINT FK_Partidas_Zonas
        FOREIGN KEY (ZonaActualId)
        REFERENCES Zonas(ZonaId)
);

CREATE TABLE Personajes (
    PersonajeId INT IDENTITY(1,1) PRIMARY KEY,
    PartidaId INT NOT NULL UNIQUE,
    Nombre NVARCHAR(100) NOT NULL,
    Salud INT NOT NULL,
    Ataque INT NOT NULL,
    Energia INT NOT NULL,
    Experiencia INT NOT NULL DEFAULT 0,
    Escudos INT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Personajes_Partidas
        FOREIGN KEY (PartidaId)
        REFERENCES Partidas(PartidaId)
);

CREATE TABLE Objetos (
    ObjetoId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(500),
    TipoObjeto NVARCHAR(50),
    EsObjetoClave BIT NOT NULL DEFAULT 0
);

CREATE TABLE InventarioPartida (
    InventarioId INT IDENTITY(1,1) PRIMARY KEY,
    PartidaId INT NOT NULL,
    ObjetoId INT NOT NULL,
    Cantidad INT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Inventario_Partidas
        FOREIGN KEY (PartidaId)
        REFERENCES Partidas(PartidaId),
    CONSTRAINT FK_Inventario_Objetos
        FOREIGN KEY (ObjetoId)
        REFERENCES Objetos(ObjetoId),
    CONSTRAINT UQ_Inventario_Partida_Objeto
        UNIQUE (PartidaId, ObjetoId)
);

CREATE TABLE Misiones (
    MisionId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(500),
    ExperienciaRecompensa INT NOT NULL DEFAULT 0
);

CREATE TABLE ProgresoMision (
    ProgresoId INT IDENTITY(1,1) PRIMARY KEY,
    PartidaId INT NOT NULL,
    MisionId INT NOT NULL,
    AnomaliaInvestigada BIT NOT NULL DEFAULT 0,
    NucleoEncontrado BIT NOT NULL DEFAULT 0,
    EncuentroSobrevivido BIT NOT NULL DEFAULT 0,
    Completada BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Progreso_Partidas
        FOREIGN KEY (PartidaId)
        REFERENCES Partidas(PartidaId),
    CONSTRAINT FK_Progreso_Misiones
        FOREIGN KEY (MisionId)
        REFERENCES Misiones(MisionId),
    CONSTRAINT UQ_Progreso_Partida_Mision
        UNIQUE (PartidaId, MisionId)
);
```

Antes de ejecutar este script en un entorno real, deberán revisarse los requisitos definitivos, los valores permitidos, las reglas de integridad y la estrategia de guardado.

## 8. Estrategia para guardar y recuperar partidas

### Guardado

1. Obtener el estado actual de la partida desde la lógica del juego.
2. Validar los datos que se van a guardar.
3. Abrir una conexión con SQL Server.
4. Registrar o actualizar la partida y el personaje.
5. Guardar el inventario y el progreso de las misiones.
6. Confirmar las operaciones relacionadas mediante una transacción.
7. Informar al usuario del resultado.

### Recuperación

1. Consultar la partida seleccionada.
2. Obtener los datos del personaje, la zona, el inventario y las misiones.
3. Validar que los registros relacionados existan.
4. Reconstruir el estado de los objetos de C#.
5. Restaurar la interfaz gráfica con los datos recuperados.

Se recomienda utilizar transacciones para evitar que un guardado parcial deje información inconsistente. Las consultas deben parametrizarse y los errores de conexión deben gestionarse adecuadamente.

## 9. Seguridad e integridad

La futura implementación deberá considerar las siguientes medidas:

- Utilizar consultas parametrizadas para reducir el riesgo de inyección SQL.
- No almacenar contraseñas ni credenciales de conexión dentro del repositorio público.
- Validar los valores de salud, energía, cantidades y experiencia.
- Utilizar claves primarias y foráneas.
- Evitar duplicados en los registros del inventario.
- Controlar los errores de conexión y de escritura.
- Utilizar transacciones en las operaciones que actualicen varias tablas.
- Definir permisos adecuados para la cuenta de conexión.

## 10. Conclusión

El modelo propuesto establece una estructura relacional para conservar el progreso de NEXUS 2.0. Las tablas de partidas, personajes, zonas, objetos, inventarios y misiones representan los principales elementos identificados en la lógica actual del videojuego.

La separación de los datos en tablas relacionadas permite reducir duplicaciones, mantener la integridad y preparar la futura recuperación de partidas. La implementación deberá acompañarse de pruebas que verifiquen el guardado, la recuperación y la consistencia de los datos.

## 11. Alcance

Este documento contiene un diseño de base de datos y un ejemplo inicial de SQL Server. La creación efectiva de la base de datos, la conexión desde C#, las operaciones de guardado y la recuperación de partidas quedan pendientes de implementación y verificación.
