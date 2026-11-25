# NEXUS — Entrega basada en H3

NEXUS es una aventura de exploración y supervivencia en la que un cadete investiga una anomalía, recupera el núcleo energético y regresa al punto de extracción. Esta entrega documenta la evolución del código de **NEXUS_entrega_inicial** incluido en H3.

## Estado de la versión inicial

La solución actual es una aplicación de consola en C# para .NET Framework 4.8. Incluye gestión de salud, energía, escudos y experiencia; exploración de cinco zonas y movimiento adyacente; eventos aleatorios; encuentros con Robot y Enemigo; inventario; análisis de zona y una misión con objetivos.

Windows Forms y la persistencia en SQL Server son tecnologías de la evolución propuesta. No están implementadas en el código H3.

## Tecnologías

- C#.
- .NET Framework 4.8.
- Visual Studio solution y proyecto.
- Aplicación de consola.

## Cómo ejecutar

1. Instala Visual Studio y el Developer/Targeting Pack de .NET Framework 4.8.
2. Abre `nexus 2.0.sln`.
3. Selecciona `Debug` y `Any CPU`.
4. Ejecuta con F5 o Ctrl+F5.

La aplicación H3 se ejecuta en consola y no requiere SQL Server.

## Documentación y evidencias

- [Diseño del juego](docs/diseno-del-juego.md)
- [Arquitectura actual](docs/arquitectura-actual.md)
- [Arquitectura propuesta](docs/arquitectura-propuesta.md)
- [Modelo de datos propuesto](docs/modelo-de-datos-propuesto.md)
- [Análisis técnico](docs/analisis-tecnico.md)
- [Bocetos de pantallas](docs/evidencias/bocetos.md)
- [Diagramas](docs/evidencias/diagramas.md)

Los wireframes y diagramas se abren directamente desde GitHub; el archivo de diagramas usa Mermaid.
