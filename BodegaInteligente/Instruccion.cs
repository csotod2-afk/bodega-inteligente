using System.Collections.Generic;

namespace BodegaInteligente
{
    /// <summary>
    /// Una instrucción ya validada gramaticalmente: el contrato entre el Parser y el Semántico.
    /// Ejemplo: REGISTRAR("FRAGIL", "P01", "MEDIANO");
    ///   Comando    = TokenComando (Valor "REGISTRAR")
    ///   Argumentos = [CADENA FRAGIL, CADENA P01, CADENA MEDIANO]
    /// </summary>
    public class Instruccion
    {
        public Token TokenComando { get; private set; }
        public List<Token> Argumentos { get; private set; }

        public string Comando { get { return TokenComando.Valor; } }
        public int Fila { get { return TokenComando.Fila; } }
        public int Columna { get { return TokenComando.Columna; } }

        public Instruccion(Token comando, List<Token> argumentos)
        {
            TokenComando = comando;
            Argumentos = argumentos ?? new List<Token>();
        }

        /// <summary>
        /// Agrupa una lista de tokens QUE EL PARSER YA APROBÓ (devuelve true) en instrucciones:
        /// cada COMANDO con los argumentos (CADENA, ID o NUMERO) que hay antes de su ")".
        /// Así el Parser real solo tiene que devolver true/false, como pide el PDF del proyecto.
        /// </summary>
        public static List<Instruccion> Dividir(List<Token> tokens)
        {
            var resultado = new List<Instruccion>();
            Token comando = null;
            List<Token> args = null;

            foreach (Token t in tokens)
            {
                switch (t.Tipo)
                {
                    case TipoToken.COMANDO:
                        comando = t;
                        args = new List<Token>();
                        break;
                    case TipoToken.CADENA:
                    case TipoToken.ID:
                    case TipoToken.NUMERO:
                        if (args != null) args.Add(t);
                        break;
                    case TipoToken.PARENTESIS_DER:
                        if (comando != null)
                        {
                            resultado.Add(new Instruccion(comando, args));
                            comando = null;
                            args = null;
                        }
                        break;
                }
            }
            return resultado;
        }
    }
}