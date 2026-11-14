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
    public partial class FrmLogin : Form
    {
        public FrmLogin()
        {
            InitializeComponent();
            UrediIzgled();
        }
        private void UrediIzgled()
        {
            Color navy = Color.FromArgb(23, 43, 77);
            Color krem = Color.FromArgb(242, 239, 233);
            Color tamna = Color.FromArgb(28, 39, 51);
            Color siva = Color.FromArgb(91, 104, 119);

            // kontrole se traže po tipu i položaju, pa ne zavise od imena
            var polja = Controls.OfType<TextBox>().OrderBy(t => t.Top).ToList();                  // email, lozinka
            var labele = Controls.OfType<Label>().Where(l => !(l is LinkLabel))
                                 .OrderBy(l => l.Top).ToList();                                   // 6 natpisa
            var dugme = Controls.OfType<Button>().FirstOrDefault();
            var link = Controls.OfType<LinkLabel>().FirstOrDefault();

            // ako raspored nije kao na slici, ništa ne diramo da se ništa ne pokvari
            if (polja.Count < 2 || labele.Count < 6 || dugme == null || link == null) return;

            // forma
            Text = "Prijava - Biblioteka";
            ClientSize = new Size(420, 470);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = krem;
            Font = new Font("Segoe UI", 10f);

            // bela kartica
            var kartica = new RoundedPanel
            {
                BackColor = krem,
                FillColor = Color.White,
                BorderColor = Color.FromArgb(214, 210, 200),
                Radius = 14,
                Location = new Point(40, 30),
                Size = new Size(340, 410)
            };
            Controls.Add(kartica);

            // naslov i podnaslov
            PostaviNatpis(kartica, labele[0], "Biblioteka", new Font("Georgia", 24f, FontStyle.Bold), tamna, 26, 24);
            PostaviNatpis(kartica, labele[1], "Prijavi se da pozajmiš knjigu.", new Font("Segoe UI", 10f), siva, 29, 76);

            // email
            PostaviNatpis(kartica, labele[2], "Email", new Font("Segoe UI", 9.5f, FontStyle.Bold), tamna, 28, 122);
            PostaviPolje(kartica, polja[0], 146);

            // lozinka
            PostaviNatpis(kartica, labele[3], "Lozinka", new Font("Segoe UI", 9.5f, FontStyle.Bold), tamna, 28, 196);
            PostaviPolje(kartica, polja[1], 220);
            polja[1].UseSystemPasswordChar = true;

            // dugme
            kartica.Controls.Add(dugme);
            dugme.Text = "Prijavi se";
            dugme.Size = new Size(284, 46);
            dugme.Location = new Point(28, 278);
            dugme.FlatStyle = FlatStyle.Flat;
            dugme.FlatAppearance.BorderSize = 0;
            dugme.FlatAppearance.MouseOverBackColor = Color.FromArgb(35, 60, 105);
            dugme.FlatAppearance.MouseDownBackColor = Color.FromArgb(17, 33, 60);
            dugme.UseVisualStyleBackColor = false;
            dugme.BackColor = navy;
            dugme.ForeColor = Color.White;
            dugme.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            dugme.Cursor = Cursors.Hand;
            dugme.Region = new Region(Zaobljeno.Putanja(new Rectangle(0, 0, dugme.Width, dugme.Height), 10));
            AcceptButton = dugme;   // Enter pokreće prijavu

            // tanka linija umesto crtica
            var linija = labele[4];
            kartica.Controls.Add(linija);
            linija.Text = "";
            linija.AutoSize = false;
            linija.BackColor = Color.FromArgb(214, 210, 200);
            linija.Size = new Size(284, 1);
            linija.Location = new Point(28, 348);

            // "Nemaš nalog? Registruj se" centrirano
            var nemas = labele[5];
            Font fNemas = new Font("Segoe UI", 10f);
            Font fLink = new Font("Segoe UI", 10f, FontStyle.Bold);
            PostaviNatpis(kartica, nemas, "Nemaš nalog?", fNemas, siva, 0, 366);

            kartica.Controls.Add(link);
            link.Text = "Registruj se";
            link.AutoSize = true;
            link.Font = fLink;
            link.BackColor = Color.Transparent;
            link.LinkColor = Color.FromArgb(11, 80, 160);
            link.ActiveLinkColor = navy;
            link.VisitedLinkColor = Color.FromArgb(11, 80, 160);
            link.LinkBehavior = LinkBehavior.HoverUnderline;

            int w1 = TextRenderer.MeasureText(nemas.Text, fNemas).Width;
            int w2 = TextRenderer.MeasureText(link.Text, fLink).Width;
            int x0 = (kartica.Width - (w1 + w2 + 4)) / 2;
            nemas.Location = new Point(x0, 366);
            link.Location = new Point(x0 + w1 + 4, 366);

            ActiveControl = polja[0];   // kursor odmah u polju za email
        }

        private void PostaviNatpis(Control roditelj, Label l, string tekst, Font font, Color boja, int x, int y)
        {
            roditelj.Controls.Add(l);
            l.Text = tekst;
            l.AutoSize = true;
            l.Font = font;
            l.ForeColor = boja;
            l.BackColor = Color.Transparent;
            l.Location = new Point(x, y);
        }

        private void PostaviPolje(Control roditelj, TextBox t, int y)
        {
            roditelj.Controls.Add(t);
            t.Font = new Font("Segoe UI", 11f);
            t.BorderStyle = BorderStyle.FixedSingle;
            t.Width = 284;
            t.Location = new Point(28, y);
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
