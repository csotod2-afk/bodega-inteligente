# Tabla de Tokens y Expresiones Regulares

**Módulo Léxico** · Christian Soto · Entrega parcial: 26 de septiembre

## 1. Tabla de tokens

| Tipo de token | Lexema / ejemplo | Expresión regular | Descripción |
|---|---|---|---|
| COMANDO | `REGISTRAR`, `ASIGNAR`, `ALMACENAR`, `AUTORIZAR_RETIRO`, `RETIRAR` | `(REGISTRAR\|ASIGNAR\|ALMACENAR\|AUTORIZAR_RETIRO\|RETIRAR)(?![A-Za-z0-9_])` | Palabras reservadas que indican la operación. Sensibles a mayúsculas. No coinciden si siguen más letras (`REGISTRARX` es un ID). |
| PARENTESIS_IZQ | `(` | `\(` | Abre la lista de argumentos. |
| PARENTESIS_DER | `)` | `\)` | Cierra la lista de argumentos. |
| COMA | `,` | `,` | Separa argumentos. |
| PUNTO_COMA | `;` | `;` | Terminador de instrucción. |
| CADENA | `"P01"`, `"FRAGIL"`, `"MEDIANO"` | `"[^"\r\n]*"` | Texto entre comillas dobles en una sola línea. El valor del token se guarda sin comillas. |
| ID | `P01`, `ESP_3` | `[A-Za-z][A-Za-z0-9_]*` | Identificador sin comillas de paquete o espacio. Empieza con letra. |
| NUMERO | `25`, `100` | `[0-9]+` | Entero sin signo. |
| *(ignorado)* Espacio | ` `, tab, `\r` | `[ \t\r]+` | No genera token. Solo avanza la columna. |
| *(ignorado)* Salto de línea | `\n` | `\n` | No genera token. Incrementa la fila y reinicia la columna a 1. |
| **ERROR** Carácter no válido | `#`, `$`, `%`, `@` | `.` | Cualquier carácter que ninguna regla anterior aceptó. Se registra con carácter, fila y columna; el análisis continúa. |
| **ERROR** Cadena sin cerrar | `"P01` | `"[^"\r\n]*` (sin comilla final) | Comilla de apertura sin cierre en la línea. Se reporta como error léxico. |

## 2. Funcionamiento de la regex maestra

El Lexer combina todas las reglas en una sola expresión con grupos con nombre:

- `\G` ancla cada coincidencia a la posición donde terminó la anterior.
- El orden es la prioridad: `COMANDO` va antes que `ID`, y `CADENA` antes que la cadena sin cerrar.
- `(?![A-Za-z0-9_])` es un lookahead negativo: impide que una palabra reservada sea prefijo de un identificador más largo.
- `[^"\r\n]` significa "cualquier carácter excepto comilla o salto de línea".
- `.` al final garantiza que siempre hay coincidencia, así el lexer nunca se detiene ante un carácter inesperado.

## 3. Ejemplo

Entrada: `REGISTRAR("P01", "FRAGIL", "MEDIANO");`

```
COMANDO: REGISTRAR   (fila 1, col 1)
PARENTESIS_IZQ       (fila 1, col 10)
CADENA: P01          (fila 1, col 11)
COMA                 (fila 1, col 16)
CADENA: FRAGIL       (fila 1, col 18)
COMA                 (fila 1, col 26)
CADENA: MEDIANO      (fila 1, col 28)
PARENTESIS_DER       (fila 1, col 37)
PUNTO_COMA           (fila 1, col 38)
```

Entrada con errores: `RETIRAR(P01) % ;` seguido de una segunda línea con `$`:

```
Error léxico: Carácter no válido '%' (fila 1, columna 14)
Error léxico: Carácter no válido '$' (fila 2, columna 1)
```

## 4. Limitaciones

- Una comilla sin cerrar consume el resto de la línea, por lo que un carácter inválido dentro de ese resto se reporta como parte de la cadena sin cerrar.
- Los números son enteros sin signo y los IDs no pueden empezar con dígito.
