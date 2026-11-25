# NEXUS V9 — El Amanecer de los Gigantes

## Entrega final: Operación Memoria

La versión actual es una aplicación de escritorio en C# (Windows Forms) con perfiles e historial de partidas guardados en Microsoft SQL Server.

- **Código, base de datos, capturas y presentación:** [Entrega_V9/](Entrega_V9/)
- **Guía de instalación y ejecución:** [Entrega_V9/README.md](Entrega_V9/README.md)
- **Script de base de datos:** [Entrega_V9/BaseDatos/01_Crear_NexusMemoria.sql](Entrega_V9/BaseDatos/01_Crear_NexusMemoria.sql)
- **Evidencias de guardado y recuperación:** [Entrega_V9/Evidencias/](Entrega_V9/Evidencias/)
- **Presentación de la actividad:** [Entrega_V9/Presentacion/NEXUS_Operacion_Memoria_SQLServer.pptx](Entrega_V9/Presentacion/NEXUS_Operacion_Memoria_SQLServer.pptx)

## Para ejecutar la versión actual

1. Instala y ejecuta el motor de SQL Server.
2. Abre Entrega_V9/BaseDatos/01_Crear_NexusMemoria.sql en SSMS y ejecútalo una vez.
3. Para compilar el juego, abre Entrega_V9/nexus el amanecer de los gigantes.slnx con Visual Studio 2022 y el paquete de .NET Framework 4.7.2.
4. Si la instancia SQL tiene otro nombre distinto de localhost, actualiza Data Source en App.config.

SSMS sirve para administrar y preparar la base de datos; el proyecto C# se abre en Visual Studio. Las instrucciones completas están en el README de la entrega.

## Material anterior

El código de consola y los documentos que ya estaban en la raíz y en docs/ se conservan como material de la versión anterior. Para revisar o compilar la entrega final, entra en Entrega_V9/.
