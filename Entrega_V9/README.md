# NEXUS V9 — Operación Memoria

Proyecto WinForms en C# con persistencia de perfiles y partidas en Microsoft SQL Server.

## Contenido

- `BaseDatos/01_Crear_NexusMemoria.sql`: crea la base `NexusMemoria` y sus tablas relacionadas.
- `BaseDatos/README.md`: modelo relacional, configuración y detalles del guardado.
- `nexus el amanecer de los gigantes/`: código fuente C# y recursos del juego.
- `Evidencias/`: capturas del perfil, partidas guardadas e inventario recuperado.
- `LEEME.txt`: pasos para abrir y compilar el proyecto.

## Requisitos

- Motor de Microsoft SQL Server ejecutándose en el equipo.
- Visual Studio 2022 y paquete de desarrollo de .NET Framework 4.7.2 para compilar el código fuente.
- SSMS es opcional para administrar SQL Server y ejecutar el script inicial.

## Preparar la base de datos

1. Abre `BaseDatos/01_Crear_NexusMemoria.sql` en SSMS y ejecútalo.
2. La aplicación se conecta por autenticación integrada de Windows a `localhost`, configurado en `nexus el amanecer de los gigantes/App.config`.
3. Si la instancia tiene otro nombre, cambia `Data Source` en ese archivo.

## Prueba de persistencia

1. Crea un perfil e inicia una partida.
2. Explora, obtiene recursos y guarda progreso mediante las acciones del juego y al cerrar la aplicación.
3. Abre NEXUS de nuevo, selecciona el mismo perfil y continúa la partida.
4. Compara el inventario y el estado recuperado con las capturas de `Evidencias/`.

La recuperación guarda estado del cadete, recursos, inventario, muralla, nivel, posición y casillas exploradas. Los enemigos transitorios del combate se regeneran al continuar.

## Entrega académica

- Publica `Presentacion/NEXUS_Operacion_Memoria_SQLServer.pptx` en el foro del curso.
- Sube el código, `BaseDatos/`, `Evidencias/` y este README al repositorio individual de GitHub.
- Adjunta una descripción a cada evidencia y conserva commits descriptivos.
