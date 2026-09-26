namespace BodegaInteligente
{
    /// <summary>
    /// Categorías de token que reconoce el analizador léxico.
    /// Los nombres van en mayúsculas para que coincidan con la salida esperada
    /// (COMANDO, PARENTESIS_IZQ, CADENA, ...) y para que el Parser los lea fácil.
    /// </summary>
    public enum TipoToken
    {
        COMANDO,          // REGISTRAR, ASIGNAR, ALMACENAR, AUTORIZAR_RETIRO, RETIRAR, CONSULTAR, APAGAR_ALARMA
        PARENTESIS_IZQ,   // (
        PARENTESIS_DER,   // )
        COMA,             // ,
        PUNTO_COMA,       // ;  (terminador opcional de instrucción)
        CADENA,           // "P01", "FRAGIL", "MEDIANO"  (Valor NO incluye las comillas)
        ID,               // identificador de paquete/espacio sin comillas: P01, ESP_3
        NUMERO            // entero: 25, 100
    }

    /// <summary>
    /// Modelo de un token: qué es (Tipo), qué texto lo formó (Valor)
    /// y dónde apareció en la entrada (Fila y Columna, ambas empezando en 1).
    /// </summary>
    public class Token
    {
        public TipoToken Tipo { get; private set; }
        public string Valor { get; private set; }
        public int Fila { get; private set; }
        public int Columna { get; private set; }

        public Token(TipoToken tipo, string valor, int fila, int columna)
        {
            Tipo = tipo;
            Valor = valor;
            Fila = fila;
            Columna = columna;
        }

        /// <summary>
        /// Los tokens que llevan contenido útil se muestran como "TIPO: valor"
        /// (COMANDO: REGISTRAR); los símbolos solo como "TIPO" (COMA).
        /// </summary>
        public override string ToString()
        {
            switch (Tipo)
            {
                case TipoToken.COMANDO:
                case TipoToken.CADENA:
                case TipoToken.ID:
                case TipoToken.NUMERO:
                    return Tipo + ": " + Valor;
                default:
                    return Tipo.ToString();
            }
        }

        /// <summary>Versión con posición, útil para depurar en la GUI.</summary>
        public string ToStringConPosicion()
        {
            return ToString() + "  [fila " + Fila + ", col " + Columna + "]";
        }
    }
}
