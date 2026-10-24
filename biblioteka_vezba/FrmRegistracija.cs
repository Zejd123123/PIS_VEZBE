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
    }
}
