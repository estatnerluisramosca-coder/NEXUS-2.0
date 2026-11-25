# Arquitectura actual de NEXUS H3

## Alcance y evidencia revisada

Este diagnóstico se basa en los archivos de la entrega H3 incluida en `NEXUS_entrega_inicial.zip`: `Program.cs`, `nexus 2.0.sln`, `nexus 2.0.csproj`, `App.config` y `Properties/AssemblyInfo.cs`. El repositorio ahora incorpora esos archivos en la raíz para que puedan inspeccionarse. Las características descritas aquí son observables en ese código; WinForms, SQL Server y guardado son propuestas futuras.

## Tecnologías y forma de ejecución

- C# como aplicación de consola.
- Solución de Visual Studio `nexus 2.0.sln`, un proyecto `nexus 2.0.csproj`.
- El proyecto declara `.NET Framework 4.8`, `OutputType=Exe`, `AnyCPU`, y compila `Program.cs` y `Properties/AssemblyInfo.cs`.
- Usa `System` y `System.Collections.Generic`; el proyecto referencia también `System.Data`, pero la entrega no contiene consultas ni conexión a base de datos.
- La entrada/salida se implementa con `Console.ReadLine/WriteLine`; no hay formularios ni recursos gráficos.
- No se identificaron proyectos de pruebas automatizadas ni configuración de persistencia en el código revisado.

## Clases y responsabilidades reales

| Clase | Responsabilidad observada en `Program.cs` |
|---|---|
| `Program` | `Main` establece el título de consola, crea `JuegoNexus` y llama `Iniciar()`. |
| `Personaje` (abstracta) | Nombre, salud, ataque, indicador `Vivo` y recepción de daño común. |
| `Cadete : Personaje` | Energía, experiencia y escudos; gastar/recuperar energía, curarse, mejorar ataque, ganar experiencia, activar escudo y mostrar estado. |
| `Entidad : Personaje` (abstracta) | Descripción y contrato de presentar/atacar al cadete. |
| `Robot : Entidad`, `Enemigo : Entidad` | Implementan presentación y ataque específicos de dos tipos de encuentro. |
| `Objeto` | Nombre y descripción. No contiene un catálogo externo ni identificador persistente. |
| `Inventario` | Mantiene `List<Objeto>`; agrega, consulta por índice/nombre, elimina, consume y muestra objetos. |
| `Zona` | Nombre y descripción de cada ubicación. |
| `Mision` | Banderas de anomalía investigada, núcleo encontrado, encuentro sobrevivido y misión completada; valida requisitos, otorga recompensa y muestra objetivos. |
| `JuegoNexus` | Contiene el bucle principal y coordina cadete, inventario, misión, zonas, posición, aleatoriedad, resonancia y estado de la expedición. Sus métodos privados ejecutan exploración, desplazamiento, encuentros, inventario, análisis y cierre. |

No hay interfaces C# ni separación de clases por archivo en el proyecto analizado. La lógica del juego y los mensajes de consola están mezclados dentro de `JuegoNexus`, `Cadete`, `Inventario`, `Mision` y entidades.

## Flujo real de partida

1. `Program.Main` crea e inicia `JuegoNexus`.
2. `Iniciar` presenta la historia, pide nombre y crea el cadete.
3. El bucle continúa mientras `jugando`, el cadete siga vivo y la misión no esté completa.
4. Muestra cadete, zona, salud, energía y resonancia; acepta explorar, desplazarse, administrar inventario, ver estado, revisar objetivos o analizar zona.
5. Las acciones actualizan objetos y estado en memoria.
6. La victoria se intenta al llegar de vuelta al Punto de Extracción. Si falta una condición, se muestran objetivos pendientes.
7. Al terminar, `MostrarFinal` diferencia misión completada, cadete sin vida o expedición interrumpida.

## Reglas y datos confirmados

- Zonas creadas en orden: Punto de Extracción, Bosque de Cristal, Laboratorio Abandonado, Grieta de Resonancia y Núcleo de la Anomalía.
- El movimiento solo permite destino adyacente y bloquea el núcleo hasta investigar la grieta.
- Explorar consume 10 de energía; atacar o escapar consume 5.
- La exploración puede registrar anomalía (zona 4) y núcleo (zona 5) como eventos deterministas de avance. La resonancia se acota a 100.
- Eventos aleatorios pueden dar objetos, iniciar combate, quitar energía, dañar, entregar información/10 XP o no hallar novedades. Con resonancia de 70 o más, el código eleva la rama que produce daño.
- El combate permite atacar, escapar, analizar si se tiene Analizador o usar un objeto; neutralizar entidad otorga 20 XP.
- El inventario puede tener más de un objeto del mismo nombre, porque almacena instancias en una lista. Célula de energía recupera 25; módulo de ataque sube 10; escudo se activa; núcleo y módulo desconocido no se consumen. El método de eliminación protege explícitamente el núcleo.
- Misión “La anomalía perdida”: requiere anomalía, núcleo y encuentro sobrevivido; al completarse otorga 100 XP y agrega “Módulo desconocido”.

## Limitaciones y puntos por confirmar

1. **Sin interfaz gráfica:** el menú y todas las acciones dependen de texto y entradas de consola.
2. **Concentración de responsabilidades:** `JuegoNexus` coordina navegación, reglas, azar y salida visual, lo cual acoplaría una futura UI si los eventos de botones reprodujeran esa lógica.
3. **Sin persistencia:** estado, inventario y progreso viven en memoria y se pierden cuando termina el proceso.
4. **Datos estáticos en código:** catálogo y zonas se construyen directamente en `CrearZonas` y `EncontrarObjeto`; no tienen identificadores estables ni carga externa.
5. **Regla ambigua a resolver:** al terminar un encuentro, si el cadete está vivo, se llama `RegistrarEncuentro()`; esto incluye la rama de escapar. La historia de “sobrevivir” puede coincidir con eso, pero debe decidirse y probarse.
6. **Inventario y misión:** `Inventario.Eliminar` impide eliminar el núcleo; `UsarObjeto` también bloquea el módulo de recompensa. Estas reglas están repartidas entre inventario y coordinador.
7. **Pruebas no incluidas:** el ZIP revisado no contiene proyecto de tests; por tanto, no se afirma cobertura automatizada.
8. **Aleatoriedad no inyectada:** `JuegoNexus` crea su propio `Random`, dificultando reproducir eventos en pruebas.
9. **Destino tecnológico:** el csproj dirige a .NET Framework 4.8; no es todavía un proyecto WinForms ni una versión moderna de .NET.

## Conclusión

H3 ya aporta una base orientada a objetos reutilizable: jerarquía de personajes, inventario, zonas, misión y coordinador de partida. La transición viable es conservar esas reglas, mover la lógica independiente de consola a una capa de aplicación/dominio, y reemplazar paulatinamente entrada/salida por formularios. La persistencia SQL deberá añadirse detrás de repositorios y no en los eventos de controles.
