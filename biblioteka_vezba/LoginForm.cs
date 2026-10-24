using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace biblioteka_vezba
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        private void lblIme_Click(object sender, EventArgs e)
        {

        }

        private void btnPrijavi_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string lozinka = txtLozinka.Text;

            if (email == "" || lozinka == "")
            {
                MessageBox.Show("Unesi email i lozinku.");
                return;
            }

            try
            {
                using (var conn = new MySqlConnection(Db.ConnStr))
                {
                    conn.Open();
                    string sql = "SELECT id, ime, lozinka, uloga FROM korisnici WHERE email = @email";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@email", email);

                        using (var r = cmd.ExecuteReader())
                        {
                            bool ok = false;

                            if (r.Read())
                            {
                                try
                                {
                                    ok = BCrypt.Net.BCrypt.Verify(lozinka, r.GetString("lozinka"));
                                }
                                catch (Exception)
                                {
                                    ok = false;
                                }

                                if (ok)
                                {
                                    Sesija.Id = r.GetInt32("id");
                                    Sesija.Ime = r.GetString("ime");
                                    Sesija.Uloga = r.GetString("uloga");
                                }
                            }

                            if (!ok)
                            {
                                MessageBox.Show("Pogrešan email ili lozinka.");
                                return;
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Greška sa bazom: " + ex.Message);
                return;
            }

            Form sledeca = Sesija.Uloga == "admin" ? (Form)new FrmAdmin() : new FrmKorisnik();
            sledeca.FormClosed += (s, a) => this.Close();
            this.Hide();
            sledeca.Show();
        }

        // dvoklik na lnkRegistracija
        private void lnkRegistracija_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var reg = new FrmRegistracija();
            reg.ShowDialog();   // login čeka dok se registracija ne zatvori
        }

        private void lblRegistracija_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var reg = new FrmRegistracija();
            reg.ShowDialog();   // login ostaje otvoren i čeka dok se registracija ne zatvori

            // kad se vratiš sa registracije, očisti polja i vrati fokus na email
            txtEmail.Clear();
            txtLozinka.Clear();
            txtEmail.Focus();
        }

        private void txtEmail_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
