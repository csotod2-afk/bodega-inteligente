using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace BodegaInteligente
{
    // Formulario principal: caja de comandos, consola de mensajes/alertas y el
    // mapa visual de la bodega. El mapa se carga en vivo desde MySQL (ConexionBD);
    // si la base de datos no está disponible al abrir, no truena: lo avisa en la
    // consola y deja el botón "Actualizar" para reintentar.
    // El Parser y el Semántico (entrega final) se conectan donde se indica en
    // btnAnalizar_Click.
    public partial class Form1 : Form
    {
        private readonly Lexer _lexer = new Lexer();
        private readonly ConexionBD _bd = new ConexionBD();

        // Cada espacio (cuadro del mapa) por su id, para poder recolorearlo sin
        // reconstruir todo el panel.
        private readonly Dictionary<string, Panel> _espacios = new Dictionary<string, Panel>();

        public Form1()
        {
            InitializeComponent();

            // La conexión puede no estar disponible al abrir el formulario (MySQL
            // apagado, credenciales distintas, etc.); por eso va en try/catch y
            // nunca impide que la ventana se abra.
            try
            {
                CargarMapa();
            }
            catch (Exception ex)
            {
                lstConsola.Items.Add("No se pudo cargar el mapa al iniciar: " + ex.Message);
            }
        }

        // ---------------- Mapa de la bodega (datos reales de MySQL) ----------------

        /// <summary>Lee vista_estado_espacios y reconstruye los cuadros del mapa.</summary>
        private void CargarMapa()
        {
            List<EspacioEstado> espacios;
            string mensaje;
            bool ok = _bd.ObtenerEstadoEspacios(out espacios, out mensaje);

            lstConsola.Items.Add(mensaje);
            if (!ok) return;

            pnlMapa.Controls.Clear();
            _espacios.Clear();

            foreach (EspacioEstado e in espacios)
            {
                Panel panel = CrearPanelEspacio(e.IdEspacio);
                pnlMapa.Controls.Add(panel);
                _espacios[e.IdEspacio] = panel;
                PintarEspacio(e);
            }
        }

        /// <summary>Crea el cuadro (Panel + Label) de un espacio y engancha el clic.</summary>
        private Panel CrearPanelEspacio(string idEspacio)
        {
            var panel = new Panel
            {
                Width = 125,
                Height = 100,
                Margin = new Padding(5),
                BorderStyle = BorderStyle.FixedSingle,
                Tag = idEspacio
            };
            var etiqueta = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold)
            };
            panel.Controls.Add(etiqueta);
            panel.Click += Espacio_Click;
            etiqueta.Click += Espacio_Click; // el clic en el texto también cuenta
            return panel;
        }

        /// <summary>Pinta un espacio según su estado real: verde = libre, amarillo = parcial, rojo = lleno.</summary>
        private void PintarEspacio(EspacioEstado e)
        {
            Panel panel;
            if (!_espacios.TryGetValue(e.IdEspacio, out panel)) return;

            Color color;
            switch (e.Estado)
            {
                case "LIBRE": color = Color.FromArgb(120, 200, 120); break;
                case "LLENO": color = Color.FromArgb(230, 100, 100); break;
                default: color = Color.FromArgb(240, 210, 90); break; // PARCIAL
            }

            panel.BackColor = color;
            panel.Tag = e; // guarda el objeto completo para el clic
            panel.Controls[0].Text = e.IdEspacio + "\r\n" + e.Estado +
                                      "\r\n" + e.UnidadesOcupadas + "/" + e.CapacidadTotal;
        }

        // Clic en un espacio: muestra su detalle en la consola.
        private void Espacio_Click(object sender, EventArgs e)
        {
            var control = sender as Control;
            Panel panel = (control is Panel) ? (Panel)control : control?.Parent as Panel;
            var estado = panel?.Tag as EspacioEstado;
            if (estado == null) return;

            lstConsola.Items.Add(
                "Espacio " + estado.IdEspacio + " (zona " + estado.Zona + "): " + estado.Estado +
                " — " + estado.UnidadesOcupadas + "/" + estado.CapacidadTotal + " unidades" +
                (estado.SoportaFragil ? ", apto para frágiles" : ""));
        }

        // ---------------- Botones ----------------

        // Tokeniza el texto de entrada y muestra tokens y errores léxicos.
        private void btnAnalizar_Click(object sender, EventArgs e)
        {
            lstTokens.Items.Clear();

            List<Token> tokens = _lexer.Tokenizar(txtEntrada.Text);

            foreach (Token t in tokens)
                lstTokens.Items.Add(t.ToStringConPosicion());

            if (_lexer.HayErrores)
            {
                foreach (ErrorLexico err in _lexer.Errores)
                    lstConsola.Items.Add(err.ToString());
                return;
            }

            lstConsola.Items.Add("Análisis léxico correcto: " + tokens.Count + " token(s).");
            // Aquí se conecta: new Parser().Analizar(tokens) -> Semantico -> ConexionBD
        }

        // Prueba de conexión a MySQL (SELECT 1).
        private void btnProbarBD_Click(object sender, EventArgs e)
        {
            string mensaje;
            _bd.ProbarConexion(out mensaje);
            lstConsola.Items.Add(mensaje);
        }

        // Vuelve a leer el mapa desde MySQL (útil si la conexión falló al abrir).
        private void btnActualizarMapa_Click(object sender, EventArgs e)
        {
            CargarMapa();
        }

        // Agrega un espacio nuevo a la bodega y refresca el mapa.
        private void btnAgregarEspacio_Click(object sender, EventArgs e)
        {
            string mensaje;
            _bd.AgregarEspacio(out mensaje);
            lstConsola.Items.Add(mensaje);
            CargarMapa();
        }

        // Elimina el último espacio, solo si está libre, y refresca el mapa.
        private void btnEliminarEspacio_Click(object sender, EventArgs e)
        {
            string mensaje;
            _bd.EliminarUltimoEspacioLibre(out mensaje);
            lstConsola.Items.Add(mensaje);
            CargarMapa();
        }
    }
}
