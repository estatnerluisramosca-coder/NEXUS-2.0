# Análisis técnico de la evolución de NEXUS

## Método y base del análisis

Se inspeccionó el código fuente H3 incluido en el ZIP publicado: `Program.cs`, `nexus 2.0.csproj`, `nexus 2.0.sln` y `App.config`. Las afirmaciones sobre clases y comportamiento se contrastaron con el código extraído, no solo con la documentación previa. Las decisiones técnicas se contrastaron con documentación primaria de Microsoft Learn. La búsqueda y revisión de fuentes se realizó el **25 de noviembre de 2026**; los enlaces quedan como referencia para volver a comprobarlos antes de implementar.

## Situación de partida observada

H3 compila como consola C# para .NET Framework 4.8. Un único `Program.cs` contiene `Program`, `Personaje`, `Cadete`, `Entidad`, `Robot`, `Enemigo`, `Objeto`, `Inventario`, `Zona`, `Mision` y `JuegoNexus`. La última clase coordina el menú, las reglas, el azar, la lectura y los mensajes de consola. El estado se pierde al terminar la ejecución. El proyecto referencia `System.Data`, pero no contiene conexión SQL ni formularios.

Las mecánicas existentes ofrecen un alcance realista para evolucionar: cinco zonas, exploración con energía y eventos, movimiento adyacente, inventario, encuentro por turnos y una misión final de recuperación. El diagnóstico detallado está en [arquitectura-actual.md](arquitectura-actual.md).

## Alternativas evaluadas

| Decisión | Alternativas | Evaluación para este proyecto | Recomendación |
|---|---|---|---|
| Interfaz | Mantener consola / Windows Forms / WPF | La consola ya ejecuta las reglas, pero no satisface el objetivo gráfico. WinForms proporciona formularios, controles, gráficos básicos y diseñador visual integrado con Visual Studio; coincide con la tecnología obligatoria. WPF ofrece otra plataforma visual y un modelo distinto que elevaría el cambio sin necesidad del juego actual. | WinForms con ventana principal y paneles/UserControls; mantener el flujo por turnos y no prometer gráficos 3D. |
| Plataforma .NET | Conservar .NET Framework 4.8 al añadir WinForms / migrar simultáneamente a .NET moderno | El `.csproj` actual ya declara v4.8. Mantenerlo evita combinar refactor de reglas, interfaz y migración de runtime. La documentación de WinForms cubre tanto .NET moderno como .NET Framework; conviene tratar una actualización como fase separada. | Primera etapa compatible con .NET Framework 4.8, salvo requisito del laboratorio que indique otra versión. |
| Separación de capas | Eventos de botones con todas las reglas / dominio y servicios separados | Poner mecánicas y SQL en eventos mantendría el acoplamiento que ya existe en `JuegoNexus`. Separar presentación, servicios de aplicación, dominio y datos permite reutilizar y verificar reglas sin UI. | Extraer acciones puras primero; formularios solo presentan estado y envían comandos. |
| Acceso a SQL | Serializar a JSON / ADO.NET con SQL Server / ORM | JSON facilita prototipo local pero no cumple persistencia relacional con SQL Server. ADO.NET permite consultas y transacciones explícitas y encaja con el proyecto Framework. Un ORM podría reducir mapeo pero añade dependencia y abstracción antes de estabilizar el modelo. | SQL Server con repositorios delgados y ADO.NET en primera etapa; reconsiderar ORM tras fijar esquema y necesidades. |
| Guardado | Guardar cada evento por separado / snapshot transaccional | Varios cambios componen una sola partida; si se actualizan por separado se puede dejar estado parcial. | Guardar snapshot completo de cadete, zona, inventario y misión dentro de una transacción local. |
| Catálogo | Usar nombres como claves / códigos estables | H3 identifica objetos por texto (p. ej. `"Nucleo energetico"`) y zonas por índice; cambiar una etiqueta rompería la identidad persistente. | Asignar `ItemCode`, `Zone.Code` y `MissionCode` estables al migrar. |

## Decisiones recomendadas y razones

### Windows Forms

La meta del curso y la arquitectura prevista exigen una app de escritorio C#. La guía oficial describe WinForms como UI de escritorio Windows con controles, gráficos, enlace de datos, entrada y diseñador visual. Eso permite presentar el mapa como nodos, controles de estado, una lista de eventos y acciones del encuentro sin rehacer la mecánica por turnos. Mantener .NET Framework 4.8 inicialmente coincide con el csproj y reduce riesgos de migración simultánea.

### Dominio desacoplado

La clase `JuegoNexus` mezcla coordinación, salida de texto, entrada y reglas. Una migración directa de cada `Console.WriteLine` a una etiqueta o MessageBox reproduciría ese acoplamiento. La separación de aplicación y dominio hace que “explorar” devuelva resultado y cambios; la UI decide cómo mostrarlos. Las entidades siguen representando el cadete y los rivales. La fuente aleatoria inyectable se recomienda para permitir secuencias repetibles en verificaciones futuras.

### ADO.NET y SQL Server

El proyecto actual ya tiene referencia a `System.Data`; ADO.NET tiene menos piezas nuevas que introducir para un guardado local pequeño en Framework 4.8. Los repositorios encapsulan conexión y consultas; el modelo propuesto guarda snapshot consistente y usa claves foráneas. Para valores dinámicos deben emplearse parámetros nombrados y tipos explícitos. La guía de Microsoft advierte que los parámetros representan valores y no identificadores SQL, por lo que los nombres dinámicos deberán salir de una lista permitida o no ser dinámicos.

Las escrituras del guardado deben ejecutarse en una transacción local: si falla una parte del snapshot, revertir todas para evitar que el jugador cargue una expedición parcialmente guardada. La selección de SQL Server (por ejemplo, instancia local disponible en laboratorio) y la cadena de conexión quedan pendientes de la instalación real.

## Riesgos y mitigaciones

- **Regla “sobrevivido”:** el código cuenta escape como encuentro sobrevivido. Acordar el significado narrativo y codificarlo como regla única antes de diseñar pruebas.
- **Mezcla de cambios:** migrar simultáneamente plataforma, UI y datos dificulta localizar errores. Mantener etapas pequeñas y conservar una versión de consola que permita comparar reglas.
- **Estado parcial:** guardar objetos, misión y personaje en operaciones separadas puede producir inconsistencias. Usar transacción y validar relaciones.
- **Dependencia de instancia:** la aplicación no funcionará si SQL Server está detenido o mal configurado. Mostrar mensaje accionable y permitir reintentar; definir durante instalación cómo se inicializa el esquema.
- **Conexiones y credenciales:** no versionar contraseñas ni dejar recursos abiertos. Configurar el proveedor de conexión en el entorno y disponer siempre de `using`/liberación de recursos.
- **Alcance visual:** WinForms no aporta automáticamente arte de juego. Priorizar claridad del mapa, estado y opciones antes de ilustración y efectos.

## Fuentes técnicas primarias

1. Microsoft Learn, [Windows Forms overview](https://learn.microsoft.com/en-us/dotnet/desktop/winforms/overview/?view=netdesktop-9.0): propósito de WinForms y capacidades de controles, gráficos, enlace y diseñador. Consultada el 25-11-2026.
2. Microsoft Learn, [Windows Forms documentation](https://learn.microsoft.com/en-us/dotnet/desktop/winforms/): guías de controles, layout, entrada de teclado y ratón. Consultada el 25-11-2026.
3. Microsoft Learn, [Configure parameters — ADO.NET Provider for SQL Server](https://learn.microsoft.com/en-us/sql/connect/ado-net/configure-parameters?view=sql-server-ver17): parámetros nombrados, tipos y límites de sustitución de valores. Consultada el 25-11-2026.
4. Microsoft Learn, [Local transactions — ADO.NET Provider for SQL Server](https://learn.microsoft.com/en-us/sql/connect/ado-net/local-transactions?view=sql-server-ver17): agrupar escrituras y revertirlas si falla una parte. Consultada el 25-11-2026.
5. Microsoft Learn, [Configuring parameters and parameter data types — .NET Framework 4.8](https://learn.microsoft.com/en-us/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types?view=netframework-4.8): parámetros en ADO.NET clásico para el runtime de H3. Consultada el 25-11-2026.

## Verificación de información obtenida con IA

La IA se utilizó como apoyo para estructurar alternativas y proponer nombres/capas; no se aceptó su descripción del proyecto sin contraste. La verificación realizada para esta documentación fue:

- Abrir y extraer el ZIP versionado en el repositorio; inspeccionar directamente declaraciones de clases y métodos de `Program.cs`.
- Confirmar en `nexus 2.0.csproj` el runtime v4.8, tipo ejecutable y archivos compilados; no afirmar que ya es WinForms por referenciar `System.Data`.
- Seguir el código de movimiento, exploración, combate, inventario y `Mision.PuedeCompletar()`; documentar expresamente la condición real de escape.
- Contrastar decisiones sobre WinForms, parámetros y transacciones con las páginas oficiales enlazadas de Microsoft Learn.
- Marcar todas las capas, tablas y pantallas no presentes en H3 como propuestas, no como funcionalidades implementadas.

La documentación no declara pruebas de compilación, de UI o de conexión SQL: no se ejecutó la aplicación en este análisis ni hay instancia de base de datos confirmada. Esas verificaciones corresponden a la implementación.
