using System;
using System.Collections;
using System.Collections.Generic;

namespace BodegaInteligente
{
    public class Parser
    {
        // Aquí debes reemplazar "Token" por el nombre real de tu clase de token
        private List<Token> _tokens;
        private int _posicionActual;
        public List<string> Errores { get; private set; }
        public bool HayErrores => Errores.Count > 0;

        public Parser()
        {
            Errores = new List<string>();
        }

        /// <summary>
        /// Método principal que inicia el análisis sintáctico.
        /// </summary>
        public bool Analizar(List<Token> tokens)
        {
            _tokens = tokens ?? new List<Token>();
            _posicionActual = 0;
            Errores.Clear();

            // Eliminar tokens de espacios o nulos
            _tokens.RemoveAll(t => t.Tipo.ToString().ToUpper() == "ESPACIO" || t.Tipo.ToString().ToUpper() == "NULO");

            if (_tokens.Count == 0)
            {
                Errores.Add("Error sintáctico: No se recibieron tokens para analizar.");
                return false;
            }

            string valorInicial = ObtenerValorActual().ToUpper();
            int colInicial = ObtenerColumnaActual();

            switch (valorInicial)
            {
                case "REGISTRAR":
                    AnalizarComandoRegistrar();
                    break;
                case "ASIGNAR":
                case "ALMACENAR":
                case "AUTORIZAR_RETIRO":
                case "RETIRAR":
                case "APAGAR_ALARMA":
                    AnalizarComandoUnParametro(valorInicial);
                    break;
                case "CONSULTAR":
                    AnalizarComandoSinParametros();
                    break;
                default:
                    Errores.Add($"Error sintáctico: Comando desconocido o no válido '{valorInicial}' en la posición {colInicial}.");
                    return false;
            }

            // Consumir ';' opcional al final si está presente
            if (_posicionActual < _tokens.Count && ObtenerTipoActual() == "PUNTO_COMA")
            {
                Avanzar();
            }

            // Verificar si quedaron tokens extra sin consumir al final
            if (!HayErrores && _posicionActual < _tokens.Count)
            {
                string valorExtra = ObtenerValorActual();
                int colExtra = ObtenerColumnaActual();
                Errores.Add($"Error sintáctico: Elemento no esperado '{valorExtra}' al final del comando en la columna {colExtra}.");
            }

            return !HayErrores;
        }

        /// <summary>
        /// Valida: REGISTRAR ( param1 , param2 , param3 )
        /// </summary>
        private void AnalizarComandoRegistrar()
        {
            Avanzar(); // Consumir 'REGISTRAR'

            if (!Coincidir("PARENTESIS_IZQ", "("))
            {
                Errores.Add("Error sintáctico: Se esperaba '(' después del comando REGISTRAR.");
                return;
            }

            if (!EsParametroValido())
            {
                Errores.Add("Error sintáctico: Se esperaba el primer parámetro en el comando REGISTRAR.");
                return;
            }
            Avanzar();

            if (!Coincidir("COMA", ","))
            {
                Errores.Add("Error sintáctico: Se esperaba ',' separando los parámetros del comando REGISTRAR.");
                return;
            }

            if (!EsParametroValido())
            {
                Errores.Add("Error sintáctico: Se esperaba el segundo parámetro en el comando REGISTRAR.");
                return;
            }
            Avanzar();

            if (!Coincidir("COMA", ","))
            {
                Errores.Add("Error sintáctico: Se esperaba ',' entre el segundo y tercer parámetro.");
                return;
            }

            if (!EsParametroValido())
            {
                Errores.Add("Error sintáctico: Se esperaba el tercer parámetro en el comando REGISTRAR.");
                return;
            }
            Avanzar();

            if (!Coincidir("PARENTESIS_DER", ")"))
            {
                Errores.Add("Error sintáctico: Se esperaba ')' de cierre al final del comando REGISTRAR.");
                return;
            }
        }

        /// <summary>
        /// Valida comandos de un solo parámetro: COMANDO ( param )
        /// </summary>
        private void AnalizarComandoUnParametro(string nombreComando)
        {
            Avanzar(); // Consumir el comando

            if (!Coincidir("PARENTESIS_IZQ", "("))
            {
                Errores.Add($"Error sintáctico: Se esperaba '(' después del comando {nombreComando}.");
                return;
            }

            if (!EsParametroValido())
            {
                Errores.Add($"Error sintáctico: Se esperaba un parámetro válido dentro del comando {nombreComando}.");
                return;
            }
            Avanzar();

            if (!Coincidir("PARENTESIS_DER", ")"))
            {
                Errores.Add($"Error sintáctico: Se esperaba ')' al cerrar el comando {nombreComando}.");
                return;
            }
        }

        /// <summary>
        /// Valida: CONSULTAR ( )
        /// </summary>
        private void AnalizarComandoSinParametros()
        {
            Avanzar(); // Consumir 'CONSULTAR'

            if (!Coincidir("PARENTESIS_IZQ", "("))
            {
                Errores.Add("Error sintáctico: Se esperaba '(' después del comando CONSULTAR.");
                return;
            }

            if (!Coincidir("PARENTESIS_DER", ")"))
            {
                Errores.Add("Error sintáctico: Se esperaba ')' de cierre en el comando CONSULTAR().");
                return;
            }
        }

        #region Métodos Auxiliares
        private string ObtenerTipoActual()
        {
            if (_posicionActual < _tokens.Count)
                return _tokens[_posicionActual].Tipo.ToString().ToUpper();
            return "FIN";
        }

        private string ObtenerValorActual()
        {
            if (_posicionActual < _tokens.Count)
                return _tokens[_posicionActual].Valor ?? "";
            return "";
        }

        private int ObtenerColumnaActual()
        {
            if (_posicionActual < _tokens.Count)
                return _tokens[_posicionActual].Columna;
            return 0;
        }

        private void Avanzar()
        {
            _posicionActual++;
        }

        private bool Coincidir(string tipoEsperado, string valorEsperado)
        {
            string tipo = ObtenerTipoActual();
            string valor = ObtenerValorActual();

            if (tipo == tipoEsperado || valor == valorEsperado)
            {
                Avanzar();
                return true;
            }
            return false;
        }

        private bool EsParametroValido()
        {
            string tipo = ObtenerTipoActual();
            string valor = ObtenerValorActual();

            return tipo == "IDENTIFICADOR" ||
                   tipo == "CADENA" ||
                   tipo == "TEXTO" ||
                   tipo == "COMANDO" ||
                   !string.IsNullOrEmpty(valor);
        }
        #endregion
    }
}