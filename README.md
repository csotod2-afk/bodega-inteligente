# Bodega Inteligente

Proyecto integrador de **Autómatas y Lenguajes Formales**: mini compilador para un sistema de gestión de bodega inteligente.

**Tecnologías:** C# (.NET Framework 4.8), Windows Forms, MySQL (`MySql.Data`).

## Pipeline

```
Form1 (GUI) → Lexer → Parser → Semantico → ConexionBD (MySQL)
```

El usuario escribe comandos como `REGISTRAR("P01", "FRAGIL", "MEDIANO");`, el Lexer los convierte en tokens, el Parser valida la gramática, el analizador semántico valida las reglas de negocio y `ConexionBD` ejecuta la operación en MySQL.

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
  Form1.cs               Cascarón de la GUI (Selvin)
docs/
  TablaTokens.md         Tabla de tokens y expresiones regulares
```

## Interfaz del Lexer (para Form1 y Parser)

```csharp
var lexer = new Lexer();
List<Token> tokens = lexer.Tokenizar(texto);
if (lexer.HayErrores)
    foreach (ErrorLexico err in lexer.Errores)   // Lexema, Mensaje, Fila, Columna
        Console.WriteLine(err);
```

## Cómo agregar tu archivo

Al crear `Parser.cs`, `Semantico.cs` o `ConexionBD.cs`, agrégalo en Visual Studio (Add → Existing Item) o en el `.csproj` dentro del `ItemGroup` de `Compile`. Manuel instala `MySql.Data` con NuGet.

## Convención de commits

Un commit por cambio lógico, mensaje en español y en imperativo, por ejemplo `Agrega Parser: regla de REGISTRAR`.
