using System.Collections.Generic;

namespace BodegaInteligente
{
    /// <summary>
    /// PARSER PROVISIONAL, solo para probar el pipeline completo mientras llega el
    /// Parser.cs de Manolo. Implementa la gramática mínima:
    ///
    ///   programa    ::= instruccion+
    ///   instruccion ::= COMANDO "(" [ argumento { "," argumento } ] ")" [ ";" ]
    ///   argumento   ::= CADENA | ID | NUMERO
    ///
    /// Cuando exista el Parser real (devuelve bool), se cambia el bloque marcado con
    /// "PUNTO DE CAMBIO" en Form1.btnAnalizar_Click. Este archivo se puede borrar
    /// entonces (y quitarlo del .csproj).
    /// </summary>
    public class ParserProvisional
    {
        public List<string> Errores { get; private set; }
        public bool HayErrores { get { return Errores.Count > 0; } }

        public ParserProvisional()
        {
            Errores = new List<string>();
        }

        /// <summary>Misma forma que el Parser real del PDF: true si la sintaxis es válida; si no, ver Errores.</summary>
        public bool Validar(List<Token> tokens)
        {
            Analizar(tokens);
            return !HayErrores;
        }

        public List<Instruccion> Analizar(List<Token> tokens)
        {
            Errores.Clear();
            var instrucciones = new List<Instruccion>();
            int i = 0;

            while (i < tokens.Count)
            {
                Token cmd = tokens[i];
                if (cmd.Tipo != TipoToken.COMANDO)
                {
                    Error("Se esperaba un comando", cmd);
                    i = SaltarHastaSiguienteInstruccion(tokens, i + 1);
                    continue;
                }
                i++;

                if (!Es(tokens, i, TipoToken.PARENTESIS_IZQ))
                {
                    Error("Se esperaba '(' después de " + cmd.Valor, Cerca(tokens, i, cmd));
                    i = SaltarHastaSiguienteInstruccion(tokens, i);
                    continue;
                }
                i++;

                var args = new List<Token>();
                bool bien = true;

                if (!Es(tokens, i, TipoToken.PARENTESIS_DER))
                {
                    while (true)
                    {
                        if (!(Es(tokens, i, TipoToken.CADENA) || Es(tokens, i, TipoToken.ID) || Es(tokens, i, TipoToken.NUMERO)))
                        {
                            Error("Se esperaba un argumento (cadena, id o número)", Cerca(tokens, i, cmd));
                            bien = false;
                            break;
                        }
                        args.Add(tokens[i]);
                        i++;
                        if (Es(tokens, i, TipoToken.COMA)) { i++; continue; }
                        break;
                    }
                }

                if (bien && !Es(tokens, i, TipoToken.PARENTESIS_DER))
                {
                    Error("Se esperaba ',' o ')'", Cerca(tokens, i, cmd));
                    bien = false;
                }
                if (!bien)
                {
                    i = SaltarHastaSiguienteInstruccion(tokens, i);
                    continue;
                }
                i++; // ')'

                if (Es(tokens, i, TipoToken.PUNTO_COMA)) i++; // terminador opcional
                instrucciones.Add(new Instruccion(cmd, args));
            }
            return instrucciones;
        }

        private static bool Es(List<Token> t, int i, TipoToken tipo)
        {
            return i < t.Count && t[i].Tipo == tipo;
        }

        // Token donde reportar: el actual, o el último conocido si la entrada terminó.
        private static Token Cerca(List<Token> t, int i, Token respaldo)
        {
            return i < t.Count ? t[i] : (t.Count > 0 ? t[t.Count - 1] : respaldo);
        }

        // Recuperación de errores: avanza hasta el siguiente COMANDO para seguir analizando.
        private static int SaltarHastaSiguienteInstruccion(List<Token> t, int i)
        {
            while (i < t.Count && t[i].Tipo != TipoToken.COMANDO) i++;
            return i;
        }

        private void Error(string mensaje, Token donde)
        {
            Errores.Add("Error sintáctico: " + mensaje + " (fila " + donde.Fila + ", columna " + donde.Columna + ")");
        }
    }
}