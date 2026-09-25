using System;
using System.Windows.Forms;

namespace BodegaInteligente
{
    // Formulario principal. Responsable: Selvin (GUI).
    // Contiene un botón de prueba del módulo léxico (Christian).
    public partial class Form1 : Form
    {
        private readonly Lexer _lexer = new Lexer();

        public Form1()
        {
            InitializeComponent();
        }

        // Prueba del Lexer: tokeniza el texto y muestra tokens y errores.
        private void btnAnalizar_Click(object sender, EventArgs e)
        {
            lstTokens.Items.Clear();
            lstErrores.Items.Clear();

            var tokens = _lexer.Tokenizar(txtEntrada.Text);

            foreach (Token t in tokens)
                lstTokens.Items.Add(t.ToStringConPosicion());

            foreach (ErrorLexico err in _lexer.Errores)
                lstErrores.Items.Add(err.ToString());

            // Aquí Selvin/Dafnee conectan: new Parser(tokens) solo si !_lexer.HayErrores
        }
    }
}
