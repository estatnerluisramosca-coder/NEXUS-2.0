# Testing & Debugging - NEXUS V9

Rama: `testing-debugging`  
Versión revisada: `Entrega_V9`  
Fecha de preparación: 2026-11-25

## Estado

Este documento define pruebas reproducibles para completar en Visual Studio y SQL Server. **Los casos aún no se ejecutaron en esta preparación.** Rellena resultado real, captura, fecha y commit después de correr cada caso. No marques como aprobado un resultado esperado que no hayas observado.

## Casos de prueba

| ID | Área | Procedimiento | Resultado esperado | Resultado real / evidencia |
|---|---|---|---|---|
| TD-01 | Entrada inválida de perfil | En Crear perfil, probar vacío, solo espacios, 61 caracteres y un nombre duplicado. | El vacío se rechaza; el campo limita a 60 caracteres; el duplicado informa el error sin cerrar NEXUS. | Pendiente de ejecución. Añadir captura y observación. |
| TD-02 | Curación inválida | En `CadeteNexus.Curar(int cantidad)`, partir de salud 50. Probar -5, 0, 10 y `Int32.MaxValue`. Repetir después de la mejora. | La versión corregida ignora cantidades no positivas y nunca deja Salud fuera del intervalo 0-100, incluso ante desbordamiento. | Pendiente. Registrar valor antes y después para cada entrada. |
| TD-03 | Guardado y recuperación | Crear perfil y partida, cambiar salud/energía, recoger objetos y avanzar de casilla; guardar, cerrar NEXUS, volver a abrir y continuar esa partida. | Se recuperan el perfil, estadísticas, inventario, muralla, nivel, posición y casillas exploradas. | Pendiente de ejecución en una copia limpia. Adjuntar capturas antes y después y, si es posible, consulta de SQL. |
| TD-04 | Fallo de conexión SQL | En una copia de evaluación, configura temporalmente un Data Source inválido. Intenta cargar perfiles y guardar. Luego restaura la cadena correcta y reintenta. | NEXUS muestra un error legible, permanece abierto y permite reintentar cuando se restablece la conexión. No se pierde silenciosamente el estado en memoria. | Pendiente. Adjuntar captura del error y de la recuperación. |
| TD-05 | Reconstrucción de esquema | Ejecutar `BaseDatos/01_Crear_NexusMemoria.sql` dos veces en la instancia de evaluación. | La primera ejecución crea la base y las tablas; la segunda termina sin duplicar objetos ni fallar. | Pendiente. Registrar instancia, fecha y resultado de ambas ejecuciones. |

## Revisión estática previa

Al leer `SistemasNexus.cs`, la implementación actual de `Curar` suma cualquier entero y solo limita el máximo con `Math.Min(100, Salud + cantidad)`. Una cantidad negativa puede reducir la salud y una suma entera grande puede desbordarse. Esto es un riesgo detectado por lectura de código, **no un resultado de prueba en ejecución**.

Mejora sugerida para que el estudiante la implemente y explique:

```csharp
public void Curar(int cantidad)
{
    if (cantidad <= 0) return;
    Salud = (int)Math.Min(100L, (long)Salud + cantidad);
}
```

Después de modificar el método, ejecuta TD-02 antes y después y registra el commit hecho desde Visual Studio. La propuesta no forma parte de este commit de documentación.

## Registro de defectos

| ID | Defecto | Corrección | Verificación posterior | Estado |
|---|---|---|---|---|
| BUG-01 | `Curar` acepta valores negativos y puede desbordar al sumar. | Pendiente de implementación personal. | Repetir TD-02 con los mismos datos. | Abierto |

## ActividadWatch

El usuario confirmó que ActivityWatch está activo durante la preparación de este documento. Se activó después de la revisión inicial, por lo que el reporte exportado no cubrirá el trabajo previo a su activación. Exporta el intervalo disponible al terminar y adjunta el archivo; no se atribuyen datos retroactivos.
