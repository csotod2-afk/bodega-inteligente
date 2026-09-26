using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BodegaInteligente
{
    // Formulario principal. Responsable: Selvin (GUI).
    // Integra el Lexer (Christian) y el Parser.
    public partial class Form1 : Form
    {
        private readonly Lexer _lexer = new Lexer();

        public Form1()
        {
            InitializeComponent();
        }

        // Tokeniza el texto de entrada y ejecuta el análisis sintáctico con el Parser.
        private void btnAnalizar_Click(object sender, EventArgs e)
        {
            lstTokens.Items.Clear();
            lstErrores.Items.Clear();

            // 1. Ejecutar Lexer (Análisis Léxico)
            List<Token> tokens = _lexer.Tokenizar(txtEntrada.Text);

            foreach (Token t in tokens)
            {
                lstTokens.Items.Add(t.ToStringConPosicion());
            }

            // Si existen errores léxicos, se muestran y se detiene el proceso
            if (_lexer.Errores.Count > 0)
            {
                foreach (ErrorLexico err in _lexer.Errores)
                {
                    lstErrores.Items.Add($"Error Léxico: {err}");
                }
                return;
            }

            // 2. Ejecutar Parser (Análisis Sintáctico)
            Parser parser = new Parser();
            bool esValido = parser.Analizar(tokens);

            if (esValido)
            {
                lstErrores.Items.Add("¡Sintaxis correcta! El comando es válido.");
            }
            else
            {
                foreach (string err in parser.Errores)
                {
                    lstErrores.Items.Add($"Error {err}");
                }
            }
        }
    }
}