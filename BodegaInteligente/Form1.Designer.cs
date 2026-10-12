namespace BodegaInteligente
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblComando = new System.Windows.Forms.Label();
            this.txtEntrada = new System.Windows.Forms.TextBox();
            this.btnAnalizar = new System.Windows.Forms.Button();
            this.btnProbarBD = new System.Windows.Forms.Button();
            this.lblTokens = new System.Windows.Forms.Label();
            this.lstTokens = new System.Windows.Forms.ListBox();
            this.lblConsola = new System.Windows.Forms.Label();
            this.lstConsola = new System.Windows.Forms.ListBox();
            this.grpMapa = new System.Windows.Forms.GroupBox();
            this.pnlMapa = new System.Windows.Forms.FlowLayoutPanel();
            this.lblLeyenda = new System.Windows.Forms.Label();
            this.btnActualizarMapa = new System.Windows.Forms.Button();
            this.btnAgregarEspacio = new System.Windows.Forms.Button();
            this.btnEliminarEspacio = new System.Windows.Forms.Button();
            this.grpMapa.SuspendLayout();
            this.SuspendLayout();
            // lblComando
            this.lblComando.AutoSize = true;
            this.lblComando.Location = new System.Drawing.Point(12, 12);
            this.lblComando.Name = "lblComando";
            this.lblComando.Text = "Comando";
            // txtEntrada
            this.txtEntrada.Location = new System.Drawing.Point(12, 30);
            this.txtEntrada.Multiline = true;
            this.txtEntrada.Name = "txtEntrada";
            this.txtEntrada.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtEntrada.Size = new System.Drawing.Size(560, 72);
            this.txtEntrada.Text = "REGISTRAR(\"P01\", \"FRAGIL\", \"MEDIANO\");";
            // btnAnalizar
            this.btnAnalizar.Location = new System.Drawing.Point(12, 108);
            this.btnAnalizar.Name = "btnAnalizar";
            this.btnAnalizar.Size = new System.Drawing.Size(120, 28);
            this.btnAnalizar.Text = "Analizar";
            this.btnAnalizar.UseVisualStyleBackColor = true;
            this.btnAnalizar.Click += new System.EventHandler(this.btnAnalizar_Click);
            // btnProbarBD
            this.btnProbarBD.Location = new System.Drawing.Point(140, 108);
            this.btnProbarBD.Name = "btnProbarBD";
            this.btnProbarBD.Size = new System.Drawing.Size(160, 28);
            this.btnProbarBD.Text = "Probar conexión BD";
            this.btnProbarBD.UseVisualStyleBackColor = true;
            this.btnProbarBD.Click += new System.EventHandler(this.btnProbarBD_Click);
            // lblTokens
            this.lblTokens.AutoSize = true;
            this.lblTokens.Location = new System.Drawing.Point(12, 146);
            this.lblTokens.Name = "lblTokens";
            this.lblTokens.Text = "Tokens";
            // lstTokens
            this.lstTokens.FormattingEnabled = true;
            this.lstTokens.Location = new System.Drawing.Point(12, 162);
            this.lstTokens.Name = "lstTokens";
            this.lstTokens.Size = new System.Drawing.Size(560, 160);
            // lblConsola
            this.lblConsola.AutoSize = true;
            this.lblConsola.Location = new System.Drawing.Point(12, 332);
            this.lblConsola.Name = "lblConsola";
            this.lblConsola.Text = "Consola de mensajes y alertas";
            // lstConsola
            this.lstConsola.FormattingEnabled = true;
            this.lstConsola.HorizontalScrollbar = true;
            this.lstConsola.Location = new System.Drawing.Point(12, 348);
            this.lstConsola.Name = "lstConsola";
            this.lstConsola.Size = new System.Drawing.Size(560, 134);
            // grpMapa
            this.grpMapa.Controls.Add(this.lblLeyenda);
            this.grpMapa.Controls.Add(this.pnlMapa);
            this.grpMapa.Location = new System.Drawing.Point(590, 12);
            this.grpMapa.Name = "grpMapa";
            this.grpMapa.Size = new System.Drawing.Size(290, 420);
            this.grpMapa.TabStop = false;
            this.grpMapa.Text = "Mapa de la bodega (datos en vivo de MySQL)";
            // pnlMapa
            this.pnlMapa.Location = new System.Drawing.Point(10, 22);
            this.pnlMapa.Name = "pnlMapa";
            this.pnlMapa.Size = new System.Drawing.Size(270, 300);
            // lblLeyenda
            this.lblLeyenda.Location = new System.Drawing.Point(10, 334);
            this.lblLeyenda.Name = "lblLeyenda";
            this.lblLeyenda.Size = new System.Drawing.Size(270, 76);
            this.lblLeyenda.Text = "Verde: libre\r\nAmarillo: ocupado parcialmente\r\nRojo: lleno\r\nClic en un espacio: ver su detalle";
            // btnActualizarMapa
            this.btnActualizarMapa.Location = new System.Drawing.Point(590, 438);
            this.btnActualizarMapa.Name = "btnActualizarMapa";
            this.btnActualizarMapa.Size = new System.Drawing.Size(90, 28);
            this.btnActualizarMapa.Text = "Actualizar";
            this.btnActualizarMapa.UseVisualStyleBackColor = true;
            this.btnActualizarMapa.Click += new System.EventHandler(this.btnActualizarMapa_Click);
            // btnAgregarEspacio
            this.btnAgregarEspacio.Location = new System.Drawing.Point(686, 438);
            this.btnAgregarEspacio.Name = "btnAgregarEspacio";
            this.btnAgregarEspacio.Size = new System.Drawing.Size(90, 28);
            this.btnAgregarEspacio.Text = "+ Espacio";
            this.btnAgregarEspacio.UseVisualStyleBackColor = true;
            this.btnAgregarEspacio.Click += new System.EventHandler(this.btnAgregarEspacio_Click);
            // btnEliminarEspacio
            this.btnEliminarEspacio.Location = new System.Drawing.Point(782, 438);
            this.btnEliminarEspacio.Name = "btnEliminarEspacio";
            this.btnEliminarEspacio.Size = new System.Drawing.Size(98, 28);
            this.btnEliminarEspacio.Text = "- Espacio";
            this.btnEliminarEspacio.UseVisualStyleBackColor = true;
            this.btnEliminarEspacio.Click += new System.EventHandler(this.btnEliminarEspacio_Click);
            // Form1
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(892, 494);
            this.Controls.Add(this.lblComando);
            this.Controls.Add(this.txtEntrada);
            this.Controls.Add(this.btnAnalizar);
            this.Controls.Add(this.btnProbarBD);
            this.Controls.Add(this.lblTokens);
            this.Controls.Add(this.lstTokens);
            this.Controls.Add(this.lblConsola);
            this.Controls.Add(this.lstConsola);
            this.Controls.Add(this.grpMapa);
            this.Controls.Add(this.btnActualizarMapa);
            this.Controls.Add(this.btnAgregarEspacio);
            this.Controls.Add(this.btnEliminarEspacio);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Bodega Inteligente";
            this.grpMapa.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblComando;
        private System.Windows.Forms.TextBox txtEntrada;
        private System.Windows.Forms.Button btnAnalizar;
        private System.Windows.Forms.Button btnProbarBD;
        private System.Windows.Forms.Label lblTokens;
        private System.Windows.Forms.ListBox lstTokens;
        private System.Windows.Forms.Label lblConsola;
        private System.Windows.Forms.ListBox lstConsola;
        private System.Windows.Forms.GroupBox grpMapa;
        private System.Windows.Forms.FlowLayoutPanel pnlMapa;
        private System.Windows.Forms.Label lblLeyenda;
        private System.Windows.Forms.Button btnActualizarMapa;
        private System.Windows.Forms.Button btnAgregarEspacio;
        private System.Windows.Forms.Button btnEliminarEspacio;
    }
}
