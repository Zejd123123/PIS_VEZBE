namespace biblioteka_vezba
{
    partial class LoginForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblNaslov = new System.Windows.Forms.Label();
            this.lblIme = new System.Windows.Forms.Label();
            this.lblLozinka = new System.Windows.Forms.Label();
            this.lblPodnaslov = new System.Windows.Forms.Label();
            this.lblLinija = new System.Windows.Forms.Label();
            this.txtEmail = new System.Windows.Forms.TextBox();
            this.txtLozinka = new System.Windows.Forms.TextBox();
            this.btnPrijavi = new System.Windows.Forms.Button();
            this.lblRegistracija = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.lblNemas = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblNaslov
            // 
            this.lblNaslov.AutoSize = true;
            this.lblNaslov.Font = new System.Drawing.Font("Georgia", 19.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNaslov.Location = new System.Drawing.Point(267, 9);
            this.lblNaslov.Name = "lblNaslov";
            this.lblNaslov.Size = new System.Drawing.Size(192, 38);
            this.lblNaslov.TabIndex = 0;
            this.lblNaslov.Text = "Biblioteka";
            // 
            // lblIme
            // 
            this.lblIme.AutoSize = true;
            this.lblIme.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblIme.Location = new System.Drawing.Point(271, 93);
            this.lblIme.Name = "lblIme";
            this.lblIme.Size = new System.Drawing.Size(47, 20);
            this.lblIme.TabIndex = 1;
            this.lblIme.Text = "Email";
            this.lblIme.Click += new System.EventHandler(this.lblIme_Click);
            // 
            // lblLozinka
            // 
            this.lblLozinka.AutoSize = true;
            this.lblLozinka.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLozinka.Location = new System.Drawing.Point(270, 183);
            this.lblLozinka.Name = "lblLozinka";
            this.lblLozinka.Size = new System.Drawing.Size(62, 20);
            this.lblLozinka.TabIndex = 2;
            this.lblLozinka.Text = "Lozinka";
            // 
            // lblPodnaslov
            // 
            this.lblPodnaslov.AutoSize = true;
            this.lblPodnaslov.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPodnaslov.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(91)))), ((int)(((byte)(104)))), ((int)(((byte)(119)))));
            this.lblPodnaslov.Location = new System.Drawing.Point(272, 59);
            this.lblPodnaslov.Name = "lblPodnaslov";
            this.lblPodnaslov.Size = new System.Drawing.Size(193, 18);
            this.lblPodnaslov.TabIndex = 3;
            this.lblPodnaslov.Text = "Prijavi se da pozajmis knjigu";
            // 
            // lblLinija
            // 
            this.lblLinija.AutoSize = true;
            this.lblLinija.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLinija.Location = new System.Drawing.Point(288, 318);
            this.lblLinija.Name = "lblLinija";
            this.lblLinija.Size = new System.Drawing.Size(143, 22);
            this.lblLinija.TabIndex = 4;
            this.lblLinija.Text = "-------------------";
            // 
            // txtEmail
            // 
            this.txtEmail.Location = new System.Drawing.Point(274, 138);
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.Size = new System.Drawing.Size(173, 22);
            this.txtEmail.TabIndex = 5;
            this.txtEmail.TextChanged += new System.EventHandler(this.txtEmail_TextChanged);
            // 
            // txtLozinka
            // 
            this.txtLozinka.Location = new System.Drawing.Point(274, 225);
            this.txtLozinka.Name = "txtLozinka";
            this.txtLozinka.Size = new System.Drawing.Size(173, 22);
            this.txtLozinka.TabIndex = 6;
            this.txtLozinka.UseSystemPasswordChar = true;
            // 
            // btnPrijavi
            // 
            this.btnPrijavi.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.btnPrijavi.FlatAppearance.BorderSize = 0;
            this.btnPrijavi.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPrijavi.ForeColor = System.Drawing.SystemColors.Control;
            this.btnPrijavi.Location = new System.Drawing.Point(274, 268);
            this.btnPrijavi.Name = "btnPrijavi";
            this.btnPrijavi.Size = new System.Drawing.Size(173, 47);
            this.btnPrijavi.TabIndex = 7;
            this.btnPrijavi.Text = "Prijavi se\r\n";
            this.btnPrijavi.UseVisualStyleBackColor = false;
            this.btnPrijavi.Click += new System.EventHandler(this.btnPrijavi_Click);
            // 
            // lblRegistracija
            // 
            this.lblRegistracija.AutoSize = true;
            this.lblRegistracija.Location = new System.Drawing.Point(369, 354);
            this.lblRegistracija.Name = "lblRegistracija";
            this.lblRegistracija.Size = new System.Drawing.Size(78, 16);
            this.lblRegistracija.TabIndex = 8;
            this.lblRegistracija.TabStop = true;
            this.lblRegistracija.Text = "Registruj se";
            this.lblRegistracija.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lblRegistracija_LinkClicked);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(318, 354);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(0, 16);
            this.label1.TabIndex = 9;
            // 
            // lblNemas
            // 
            this.lblNemas.AutoSize = true;
            this.lblNemas.Location = new System.Drawing.Point(271, 354);
            this.lblNemas.Name = "lblNemas";
            this.lblNemas.Size = new System.Drawing.Size(95, 16);
            this.lblNemas.TabIndex = 10;
            this.lblNemas.Text = "Nemas nalog?";
            // 
            // LoginForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.lblNemas);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblRegistracija);
            this.Controls.Add(this.btnPrijavi);
            this.Controls.Add(this.txtLozinka);
            this.Controls.Add(this.txtEmail);
            this.Controls.Add(this.lblLinija);
            this.Controls.Add(this.lblPodnaslov);
            this.Controls.Add(this.lblLozinka);
            this.Controls.Add(this.lblIme);
            this.Controls.Add(this.lblNaslov);
            this.Name = "LoginForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblNaslov;
        private System.Windows.Forms.Label lblIme;
        private System.Windows.Forms.Label lblLozinka;
        private System.Windows.Forms.Label lblPodnaslov;
        private System.Windows.Forms.Label lblLinija;
        private System.Windows.Forms.TextBox txtEmail;
        private System.Windows.Forms.TextBox txtLozinka;
        private System.Windows.Forms.Button btnPrijavi;
        private System.Windows.Forms.LinkLabel lblRegistracija;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblNemas;
    }
}

