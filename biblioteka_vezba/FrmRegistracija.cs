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
    public partial class FrmRegistracija : Form
    {
        public FrmRegistracija()
        {
            InitializeComponent();
            UrediIzgled();
        }
        private void UrediIzgled()
        {
            Color navy = Color.FromArgb(23, 43, 77);
            Color krem = Color.FromArgb(242, 239, 233);
            Color tamna = Color.FromArgb(28, 39, 51);

            // postojeće kontrole pokupi prije nego što se pomjere
            var labele = Controls.OfType<Label>().OrderBy(l => l.Top).ToList();
            var polja = Controls.OfType<TextBox>().OrderBy(t => t.Top).ToList();
            var dugme = Controls.OfType<Button>().FirstOrDefault();

            // forma
            Text = "Registracija - Biblioteka";
            ClientSize = new Size(420, 540);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = krem;
            Font = new Font("Segoe UI", 10f);

            // bijela kartica u koju prelaze sve kontrole
            var kartica = new RoundedPanel
            {
                BackColor = krem,
                FillColor = Color.White,
                BorderColor = Color.FromArgb(214, 210, 200),
                Radius = 14,
                Location = new Point(40, 30),
                Size = new Size(340, 480)
            };
            Controls.Add(kartica);

            kartica.Controls.Add(new Label
            {
                Text = "Registracija",
                Font = new Font("Georgia", 20f, FontStyle.Bold),
                ForeColor = tamna,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(26, 22)
            });
            kartica.Controls.Add(new Label
            {
                Text = "Napravi nalog da pozajmiš knjigu.",
                Font = new Font("Segoe UI", 10f),
                ForeColor = Color.FromArgb(91, 104, 119),
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(28, 66)
            });

            // svako polje dobije svoj natpis iznad sebe
            int y = 112;
            for (int i = 0; i < polja.Count; i++)
            {
                if (i < labele.Count)
                {
                    var l = labele[i];
                    kartica.Controls.Add(l);
                    l.AutoSize = true;
                    l.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
                    l.ForeColor = tamna;
                    l.BackColor = Color.Transparent;
                    l.Location = new Point(28, y);
                }

                var t = polja[i];
                kartica.Controls.Add(t);
                t.Font = new Font("Segoe UI", 11f);
                t.BorderStyle = BorderStyle.FixedSingle;
                t.Width = 284;
                t.Location = new Point(28, y + 22);
                y += 66;
            }

            // zadnje polje je lozinka
            if (polja.Count > 0) polja.Last().UseSystemPasswordChar = true;

            // dugme
            if (dugme != null)
            {
                kartica.Controls.Add(dugme);
                dugme.Text = "Registruj se";
                dugme.Size = new Size(284, 46);
                dugme.Location = new Point(28, y + 14);
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
                AcceptButton = dugme;   // Enter pokreće registraciju
            }

            // povratak na prijavu
            var link = new LinkLabel
            {
                Text = "Već imaš nalog? Prijavi se",
                AutoSize = false,
                Size = new Size(284, 24),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(28, y + 76),
                LinkColor = Color.FromArgb(11, 80, 160),
                ActiveLinkColor = navy,
                LinkBehavior = LinkBehavior.HoverUnderline,
                BackColor = Color.Transparent
            };
            link.LinkClicked += (s, e) => Close();
            kartica.Controls.Add(link);
        }

        private void btnRegistracija_Click(object sender, EventArgs e)
        {
            string ime = txtIme.Text.Trim();
            string prezime = txtPrezime.Text.Trim();
            string email = txtEmail.Text.Trim();
            string lozinka = txtLozinka.Text;

            if (ime == "" || prezime == "" || email == "" || lozinka.Length < 6)
            {
                MessageBox.Show("Popuni sva polja, lozinka mora imati bar 6 karaktera.");
                return;
            }

            string hash = BCrypt.Net.BCrypt.HashPassword(lozinka);

            try
            {
                using (var conn = new MySqlConnection(Db.ConnStr))
                {
                    conn.Open();
                    string sql = "INSERT INTO korisnici (ime, prezime, email, lozinka) " +
                                 "VALUES (@ime, @prezime, @email, @lozinka)";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@ime", ime);
                        cmd.Parameters.AddWithValue("@prezime", prezime);
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@lozinka", hash);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Nalog je napravljen. Sada se možeš prijaviti.");
                this.Close();
            }
            catch (MySqlException ex)
            {
                // 1062 = duplikat, email je UNIQUE u tabeli
                if (ex.Number == 1062)
                    MessageBox.Show("Taj email je već registrovan.");
                else
                    MessageBox.Show("Greška sa bazom: " + ex.Message);
            }
        }

        private void FrmRegistracija_Load(object sender, EventArgs e)
        {

        }
    }
}
