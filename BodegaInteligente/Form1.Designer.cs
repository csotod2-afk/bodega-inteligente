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
            this.txtEntrada = new System.Windows.Forms.TextBox();
            this.btnAnalizar = new System.Windows.Forms.Button();
            this.lstTokens = new System.Windows.Forms.ListBox();
            this.lstErrores = new System.Windows.Forms.ListBox();
            this.lblTokens = new System.Windows.Forms.Label();
            this.lblErrores = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // txtEntrada
            this.txtEntrada.Location = new System.Drawing.Point(12, 12);
            this.txtEntrada.Multiline = true;
            this.txtEntrada.Name = "txtEntrada";
            this.txtEntrada.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtEntrada.Size = new System.Drawing.Size(760, 100);
            this.txtEntrada.Text = "REGISTRAR(\"P01\", \"FRAGIL\", \"MEDIANO\");";
            // btnAnalizar
            this.btnAnalizar.Location = new System.Drawing.Point(12, 118);
            this.btnAnalizar.Name = "btnAnalizar";
            this.btnAnalizar.Size = new System.Drawing.Size(120, 28);
            this.btnAnalizar.Text = "Analizar";
            this.btnAnalizar.UseVisualStyleBackColor = true;
            this.btnAnalizar.Click += new System.EventHandler(this.btnAnalizar_Click);
            // lblTokens
            this.lblTokens.AutoSize = true;
            this.lblTokens.Location = new System.Drawing.Point(12, 154);
            this.lblTokens.Text = "Tokens";
            // lstTokens
            this.lstTokens.FormattingEnabled = true;
            this.lstTokens.Location = new System.Drawing.Point(12, 170);
            this.lstTokens.Name = "lstTokens";
            this.lstTokens.Size = new System.Drawing.Size(760, 160);
            // lblErrores
            this.lblErrores.AutoSize = true;
            this.lblErrores.Location = new System.Drawing.Point(12, 338);
            this.lblErrores.Text = "Errores léxicos";
            // lstErrores
            this.lstErrores.FormattingEnabled = true;
            this.lstErrores.Location = new System.Drawing.Point(12, 354);
            this.lstErrores.Name = "lstErrores";
            this.lstErrores.Size = new System.Drawing.Size(760, 82);
            // Form1
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 450);
            this.Controls.Add(this.txtEntrada);
            this.Controls.Add(this.btnAnalizar);
            this.Controls.Add(this.lblTokens);
            this.Controls.Add(this.lstTokens);
            this.Controls.Add(this.lblErrores);
            this.Controls.Add(this.lstErrores);
            this.Name = "Form1";
            this.Text = "Bodega Inteligente";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.TextBox txtEntrada;
        private System.Windows.Forms.Button btnAnalizar;
        private System.Windows.Forms.ListBox lstTokens;
        private System.Windows.Forms.ListBox lstErrores;
        private System.Windows.Forms.Label lblTokens;
        private System.Windows.Forms.Label lblErrores;
    }
}
