using System;

namespace BodegaInteligente
{
    /// <summary>
    /// Describe un error léxico encontrado durante el análisis.
    /// No es una excepción lanzada: el Lexer los acumula en una lista para poder
    /// seguir analizando y reportar todos los errores de una sola pasada.
    /// </summary>
    public class ErrorLexico
    {
        /// <summary>Texto que causó el error (un carácter inválido, o una cadena sin cerrar).</summary>
        public string Lexema { get; private set; }
        public string Mensaje { get; private set; }
        public int Fila { get; private set; }
        public int Columna { get; private set; }

        public ErrorLexico(string lexema, string mensaje, int fila, int columna)
        {
            Lexema = lexema;
            Mensaje = mensaje;
            Fila = fila;
            Columna = columna;
        }

        public override string ToString()
        {
            return String.Format("Error léxico: {0} '{1}' (fila {2}, columna {3})",
                                 Mensaje, Lexema, Fila, Columna);
        }
    }
}
