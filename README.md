# NEXUS v1.0 — Ecos del Vacío

Juego de exploración, decisiones y supervivencia desarrollado en C# como aplicación de consola para .NET Framework 4.8.

## Estado del proyecto
Esta entrega conserva el código fuente recibido y añade documentación inicial para el Hito 3. La documentación se basa en `Program.cs`; los conceptos que no están implementados se marcan como ejemplos complementarios.

## Funcionalidades identificadas
- Crear un cadete y gestionar salud, energía, ataque, escudos y experiencia.
- Explorar zonas y generar eventos aleatorios.
- Desplazarse entre zonas adyacentes.
- Combatir contra entidades (`Robot` y `Enemigo`).
- Administrar un inventario de objetos.
- Registrar objetivos y completar la misión “La anomalía perdida”.

## Requisitos
- Visual Studio Community.
- .NET Framework 4.8 Developer Pack/Targeting Pack.
- Windows o entorno compatible con aplicaciones de consola .NET Framework.

## Cómo ejecutar
1. Abre `nexus 2.0.sln` en Visual Studio.
2. Comprueba que el proyecto use .NET Framework 4.8.
3. Selecciona la configuración `Debug` y la plataforma `Any CPU`.
4. Ejecuta con `F5` o `Ctrl+F5`.

## Archivos principales
- `Program.cs`: contiene el punto de entrada y las clases del juego.
- `nexus 2.0.csproj`: configuración del proyecto.
- `nexus 2.0.sln`: solución de Visual Studio.
- `docs/manual-tecnico.md`: manual de estudio.
- `docs/diagramas.mmd`: diagramas en sintaxis Mermaid.

## Estructura del diseño
El código está concentrado en un solo archivo (`Program.cs`) pero separa responsabilidades mediante clases: `JuegoNexus` coordina la partida; `Cadete`, `Entidad`, `Robot` y `Enemigo` representan actores; `Inventario` gestiona objetos; `Zona` representa ubicaciones; y `Mision` registra objetivos.

## Limitaciones detectadas para el estudio
- No se encontraron interfaces ni consultas LINQ en el código revisado.
- El inventario utiliza `List<Objeto>` y recorridos tradicionales.
- Se recomienda separar las clases en archivos individuales cuando se realice una siguiente refactorización, sin cambiar el comportamiento.
- Antes de publicar, revisa los metadatos de `Properties/AssemblyInfo.cs` y confirma que la información de autoría/empresa se pueda publicar.

## Documentación
Consulta la carpeta `docs/`. El diagnóstico de aprendizaje incluido es provisional porque debe reflejar respuestas y razonamientos propios del estudiante, no solamente contenido generado por una herramienta.
