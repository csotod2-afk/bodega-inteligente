using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BodegaInteligente
{
    /// <summary>
    /// Analizador léxico de la bodega inteligente.
    ///
    /// Uso desde Form1 / Parser:
    ///     var lexer = new Lexer();
    ///     List&lt;Token&gt; tokens = lexer.Tokenizar(texto);
    ///     if (lexer.HayErrores) { foreach (var e in lexer.Errores) ... }
    ///
    /// Los errores NO detienen el análisis: se acumulan en Errores y el lexer
    /// continúa con el siguiente carácter (recuperación en "modo pánico" mínimo).
    /// </summary>
    public class Lexer
    {
        // ------------------------------------------------------------------
        // Expresión regular maestra. Cada alternativa es un grupo con nombre.
        // El motor prueba las alternativas EN ORDEN, por eso el orden importa.
        // \G ancla cada coincidencia justo donde terminó la anterior, así
        // no se "salta" texto sin analizar.
        // ------------------------------------------------------------------
        private static readonly Regex PatronMaestro = new Regex(
            @"\G(?:" +
            @"(?<SALTO>\n)" +                                   // fin de línea
            @"|(?<ESPACIO>[ \t\r]+)" +                          // espacios, tabs, \r
            @"|(?<COMANDO>(?:REGISTRAR|ASIGNAR|ALMACENAR|AUTORIZAR_RETIRO|RETIRAR|CONSULTAR|APAGAR_ALARMA)(?![A-Za-z0-9_]))" +
            @"|(?<ID>[A-Za-z][A-Za-z0-9_]*)" +                  // identificador
            @"|(?<NUMERO>[0-9]+)" +                             // entero
            @"|(?<CADENA>""[^""\r\n]*"")" +                     // "texto" cerrado
            @"|(?<CADENA_ABIERTA>""[^""\r\n]*)" +               // "texto sin comilla de cierre
            @"|(?<PAR_IZQ>\()" +
            @"|(?<PAR_DER>\))" +
            @"|(?<COMA>,)" +
            @"|(?<PUNTO_COMA>;)" +
            @"|(?<INVALIDO>.)" +                                // cualquier otro carácter
            @")",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private readonly List<ErrorLexico> _errores = new List<ErrorLexico>();

        /// <summary>Errores léxicos de la última llamada a Tokenizar (solo lectura).</summary>
        public IReadOnlyList<ErrorLexico> Errores { get { return _errores; } }

        /// <summary>true si la última pasada encontró al menos un error léxico.</summary>
        public bool HayErrores { get { return _errores.Count > 0; } }

        /// <summary>
        /// Convierte el texto fuente en una lista de tokens.
        /// Limpia los errores de análisis previos en cada llamada.
        /// </summary>
        public List<Token> Tokenizar(string entrada)
        {
            var tokens = new List<Token>();
            _errores.Clear();
            if (String.IsNullOrEmpty(entrada)) return tokens;

            int pos = 0;      // posición actual dentro del string
            int fila = 1;     // 1-based
            int columna = 1;  // 1-based

            while (pos < entrada.Length)
            {
                Match m = PatronMaestro.Match(entrada, pos);
                // El grupo INVALIDO (.) garantiza que siempre hay coincidencia,
                // salvo que el carácter sea '\n' (ya cubierto por SALTO).
                string lexema = m.Value;

                if (m.Groups["SALTO"].Success)
                {
                    fila++;
                    columna = 1;
                    pos += lexema.Length;
                    continue;
                }
                if (m.Groups["ESPACIO"].Success)
                {
                    // Se ignoran, pero la columna avanza.
                    columna += lexema.Length;
                    pos += lexema.Length;
                    continue;
                }

                if (m.Groups["COMANDO"].Success)
                    tokens.Add(new Token(TipoToken.COMANDO, lexema, fila, columna));
                else if (m.Groups["ID"].Success)
                    tokens.Add(new Token(TipoToken.ID, lexema, fila, columna));
                else if (m.Groups["NUMERO"].Success)
                    tokens.Add(new Token(TipoToken.NUMERO, lexema, fila, columna));
                else if (m.Groups["CADENA"].Success)
                    // Se guarda el contenido sin las comillas: "P01" -> P01
                    tokens.Add(new Token(TipoToken.CADENA,
                        lexema.Substring(1, lexema.Length - 2), fila, columna));
                else if (m.Groups["CADENA_ABIERTA"].Success)
                    _errores.Add(new ErrorLexico(lexema, "Cadena sin comilla de cierre", fila, columna));
                else if (m.Groups["PAR_IZQ"].Success)
                    tokens.Add(new Token(TipoToken.PARENTESIS_IZQ, lexema, fila, columna));
                else if (m.Groups["PAR_DER"].Success)
                    tokens.Add(new Token(TipoToken.PARENTESIS_DER, lexema, fila, columna));
                else if (m.Groups["PUNTO_COMA"].Success)
                    tokens.Add(new Token(TipoToken.PUNTO_COMA, lexema, fila, columna));
                else if (m.Groups["COMA"].Success)
                    tokens.Add(new Token(TipoToken.COMA, lexema, fila, columna));
                else
                    _errores.Add(new ErrorLexico(lexema, "Carácter no válido", fila, columna));

                columna += lexema.Length;
                pos += lexema.Length;
            }

            return tokens;
        }
    }
}
