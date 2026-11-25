# NEXUS — Ecos del Vacío

NEXUS es una aventura de exploración y supervivencia en la que un cadete investiga una anomalía dimensional, recupera su núcleo energético y regresa al punto de extracción. Esta entrega conserva la base de H3, actualmente una aplicación de consola en C# para .NET Framework 4.8, y documenta la evolución propuesta a escritorio con Windows Forms y guardado en SQL Server.

## Estado actual

**Implementado en H3:** crear cadete; salud, energía, ataque, escudos y experiencia; cinco zonas con movimiento adyacente; eventos aleatorios; encuentros con Robot y Enemigo; inventario; análisis de zona; objetivos y misión “La anomalía perdida”.

**Propuesto, aún no implementado:** interfaz Windows Forms, base de datos SQL Server, guardado/carga persistente, formularios visuales, repositorios y scripts SQL. La entrega H3 no guarda partidas después de cerrar el proceso.

## Tecnologías y archivos

- C# y .NET Framework 4.8.
- Solución Visual Studio: `nexus 2.0.sln`.
- Proyecto: `nexus 2.0.csproj`.
- Código H3: `Program.cs`; el código se conserva en un solo archivo y las responsabilidades están separadas en clases.
- `NEXUS_entrega_inicial.zip`: copia original de la entrega fuente recibida.
- `.gitignore`: excluye resultados de compilación, archivos locales de Visual Studio y temporales.

## Cómo ejecutar H3

1. Instala Visual Studio y el Developer/Targeting Pack de .NET Framework 4.8.
2. Abre `nexus 2.0.sln`.
3. Selecciona `Debug` y `Any CPU`.
4. Ejecuta con F5 o Ctrl+F5.

La aplicación actual se ejecuta en consola. No necesita SQL Server. La evolución gráfica descrita en `docs/` todavía debe implementarse antes de poder ejecutarse como WinForms.

## Documentación

- [Diseño de la experiencia y reglas](docs/diseno-del-juego.md)
- [Arquitectura actual basada en H3](docs/arquitectura-actual.md)
- [Arquitectura propuesta WinForms / SQL Server](docs/arquitectura-propuesta.md)
- [Modelo de datos propuesto](docs/modelo-de-datos-propuesto.md)
- [Análisis técnico, decisiones y fuentes](docs/analisis-tecnico.md)
- [Evidencias: cuatro wireframes y navegación](docs/evidencias/bocetos.md)
- [Diagramas Mermaid de ciclo, arquitectura y modelo](docs/evidencias/diagramas.mmd)

## Alcance de esta entrega

Se documenta una propuesta técnica y de experiencia para la siguiente etapa. Las pantallas y persistencia son diseños de referencia, no funcionalidades ya ejecutables. El diagnóstico refleja el contenido del ZIP H3; los puntos que requieren decisión se señalan dentro de los documentos.

Repositorio público: https://github.com/estatnerluisramosca-coder/NEXUS-AMANECER-DE-LOS-GIGANTES-
