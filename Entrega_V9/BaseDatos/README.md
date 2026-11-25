# Persistencia de NEXUS

## Preparar SQL Server

1. Instala o inicia SQL Server LocalDB, SQL Server Express o una instancia compatible.
2. Abre `01_Crear_NexusMemoria.sql` en SQL Server Management Studio o Azure Data Studio y ejecútalo con una cuenta que pueda crear la base `NexusMemoria`.
3. Comprueba la cadena `NexusDb` en `nexus el amanecer de los gigantes/App.config`. El valor inicial usa `localhost`, autenticación integrada y confía en el certificado local. Si usas otra instancia, cambia solo `Data Source`.
4. Abre la solución con Visual Studio 2022 y compílala en Release.

La aplicación no crea bases de datos silenciosamente. Si SQL Server no está disponible, muestra el error y deja la partida en pantalla para que se pueda corregir la conexión y volver a guardar.

## Modelo relacional

El modelo registra perfiles, el cadete asociado, varias partidas por perfil, los objetos de cada partida y un historial de guardados. Las claves foráneas eliminan inventario e historial al borrar una partida y eliminan sus partidas al borrar un perfil.

```mermaid
erDiagram
    PERFILES ||--|| CADETES : tiene
    PERFILES ||--o{ PARTIDAS : crea
    PARTIDAS ||--o{ INVENTARIO : contiene
    PARTIDAS ||--o{ EVENTOS_PARTIDA : registra

    PERFILES {
        int PerfilId PK
        nvarchar Nombre UK
        datetime FechaCreacion
    }
    CADETES {
        int CadeteId PK
        int PerfilId FK_UK
        nvarchar Nombre
        int Salud
        int Energia
        int Ataque
        int Experiencia
        int Escudos
        int Materiales
        int Suministros
        datetime FechaActualizacion
    }
    PARTIDAS {
        int PartidaId PK
        int PerfilId FK
        varchar Muralla
        int Salud
        int Energia
        int Ataque
        int Experiencia
        int Escudos
        int Materiales
        int Suministros
        int Nivel
        int Eliminaciones
        int ExperienciaOleada
        int TiempoOleada
        int Fila
        int Columna
        bit JefeDerrotado
        bit Terminada
        nvarchar CasillasProcesadas
        datetime FechaCreacion
        datetime FechaActualizacion
    }
    INVENTARIO {
        int InventarioId PK
        int PartidaId FK
        nvarchar Nombre
        datetime FechaObtencion
    }
    EVENTOS_PARTIDA {
        int EventoId PK
        int PartidaId FK
        nvarchar Evento
        datetime Fecha
    }
```

## Guardado y recuperación

- Cada nueva expedición crea una fila en `Partidas`.
- Los movimientos, las acciones de juego y el cierre de la aplicación actualizan el progreso con consultas parametrizadas y una transacción.
- Se guardan salud, energía, ataque, experiencia, escudos, materiales, raciones, muralla, nivel, eliminaciones, posición, objetivo de salida, casillas exploradas e inventario.
- Continuar restaura esos datos y reconstruye el mapa. Los enemigos transitorios del combate se regeneran; el contador del nivel, los recursos y las casillas exploradas se recuperan.
- La pantalla principal permite elegir perfil, continuar una partida activa y eliminar partidas.
- `EventosPartida` recibe un registro por guardado como evidencia de la operación.

Las consultas de lectura y escritura usan parámetros SQL. No se almacenan credenciales en el repositorio; se usa autenticación integrada de Windows.
