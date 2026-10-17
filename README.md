# Bodega Inteligente

Proyecto integrador de **Autómatas y Lenguajes Formales**: mini compilador para un sistema de gestión de bodega inteligente.

**Tecnologías:** C# (.NET Framework 4.8), Windows Forms, MySQL (`MySql.Data`).

## Pipeline

```
Form1 (GUI) → Lexer → Parser → Semantico → ConexionBD (MySQL)
```

El usuario escribe comandos como `REGISTRAR("P01", "FRAGIL", "MEDIANO");`, el Lexer los convierte en tokens, el Parser valida la gramática, el analizador semántico valida las reglas de negocio y `ConexionBD` ejecuta la operación en MySQL.

El pipeline completo ya está ensamblado en `Form1.btnAnalizar_Click`: Lexer → Parser →
Semántico → `ConexionBD.Ejecutar` → refresco del mapa. Por ahora usa `ParserProvisional`
(gramática mínima, mismo contrato bool que pide el PDF) mientras se fusiona el Parser real
de Manolo desde la rama `demo-lexer`; cuando eso pase, solo hay que cambiar esa línea en
`btnAnalizar_Click` (está marcada con "PUNTO DE CAMBIO").

## Reparto del equipo

| Integrante | Responsabilidad |
|---|---|
| **Selvin** | `Form1` / interfaz gráfica |
| **Christian** | `Token.cs`, `Lexer.cs` (módulo léxico) |
| **Manolo** | `Parser.cs` y gramática BNF |
| **Manuel** | `ConexionBD.cs` y `bodega_db.sql` |
| **Dafnee** | `Semantico.cs` y ensamble final |

## Estructura

```
BodegaInteligente.sln
BodegaInteligente/       Proyecto Windows Forms
  Token.cs               Enum TipoToken y clase Token
  ErrorLexico.cs         Modelo de error léxico
  Lexer.cs               Analizador léxico
  ConexionBD.cs          Acceso a MySQL: probar conexión, leer/agregar/eliminar espacios
  ConexionBD.Operaciones.cs  Ejecuta los comandos del lenguaje (REGISTRAR, ASIGNAR, ...) con historial
  EspacioEstado.cs       Modelo de un renglón de vista_estado_espacios
  Instruccion.cs         Agrupa los tokens que el Parser aprobó en instrucciones (comando + argumentos)
  ParserProvisional.cs   Parser mínimo temporal, mientras se fusiona el Parser real de Manolo
  Semantico.cs           Reglas de negocio (ciclo de vida del paquete, asignación automática, alarmas)
  Form1.cs               GUI: caja de comandos, consola, mapa en vivo (MySQL) y el pipeline completo
db/
  bodega_db.sql          Script de la base de datos (espacios, paquetes, historial, vista)
docs/
  TablaTokens.md         Tabla de tokens y expresiones regulares
  DER.md                 Diagrama entidad-relación
```

## Interfaz del Lexer (para Form1 y Parser)

```csharp
var lexer = new Lexer();
List<Token> tokens = lexer.Tokenizar(texto);
if (lexer.HayErrores)
    foreach (ErrorLexico err in lexer.Errores)   // Lexema, Mensaje, Fila, Columna
        Console.WriteLine(err);
```

## Base de datos

1. Instala MySQL y ejecuta el script: `mysql -u root -p < db/bodega_db.sql`
2. Ajusta usuario y contraseña en `ConexionBD.CadenaPorDefecto`.
3. Compila (Visual Studio restaura `MySql.Data` por NuGet) y abre Form1.

El mapa de la derecha se carga en vivo desde la vista `vista_estado_espacios` al abrir
el formulario; si MySQL no está disponible, no truena: lo avisa en la consola y el
botón **Actualizar** permite reintentar. **+ Espacio** y **- Espacio** agregan o quitan
un espacio (solo se puede eliminar uno que esté libre), y un clic en cualquier cuadro
del mapa muestra su detalle en la consola.

## Cómo agregar tu archivo

Al crear `Parser.cs`, `Semantico.cs` o `ConexionBD.cs`, agrégalo en Visual Studio (Add → Existing Item) o en el `.csproj` dentro del `ItemGroup` de `Compile`. Manuel instala `MySql.Data` con NuGet.

## Convención de commits

Un commit por cambio lógico, mensaje en español y en imperativo, por ejemplo `Agrega Parser: regla de REGISTRAR`.
