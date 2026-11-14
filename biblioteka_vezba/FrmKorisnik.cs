using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace biblioteka_vezba
{
    public partial class FrmKorisnik : Form
    {
        // koliko kasnjenja je dovoljno da se pozajmljivanje blokira
        public const int MaxKasnjenja = 5;

        private Label lblDobrodosli;
        private TextBox txtPretraga;
        private ComboBox cmbKategorija;
        private DataGridView dgvKnjige;
        private DataGridView dgvMoje;
        private RoundedButton btnPozajmi;
        private RoundedButton btnVrati;
        private RoundedButton btnOdjava;

        private bool _gotovo = false;
        private bool _spremno = false;

        private static readonly Color Navy = Color.FromArgb(23, 43, 77);
        private static readonly Color Krem = Color.FromArgb(242, 239, 233);
        private static readonly Color Zlatna = Color.FromArgb(229, 185, 107);
        private static readonly Color Tamna = Color.FromArgb(28, 39, 51);

        public FrmKorisnik()
        {
            InitializeComponent();
            Inicijalizuj();
        }

        // ako je Load povezan u dizajneru, poziva se i odavde, ali se izvrsi samo jednom
        private void FrmKorisnik_Load(object sender, EventArgs e)
        {
            Inicijalizuj();
        }

        // ostavljeno samo da Designer.cs ne baca gresku ako jos pominje label1
        private void label1_Click(object sender, EventArgs e) { }

        private void Inicijalizuj()
        {
            if (_gotovo) return;
            _gotovo = true;

            IzgradiIzgled();
            lblDobrodosli.Text = "Dobrodošao, " + Sesija.Ime + "!";

            dgvKnjige.ReadOnly = true;
            dgvKnjige.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvKnjige.MultiSelect = false;
            dgvKnjige.AllowUserToAddRows = false;
            dgvKnjige.RowHeadersVisible = false;
            dgvKnjige.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            UcitajKategorije();
            _spremno = true;
            UcitajKnjige();
            UcitajMoje();
        }

        // ------------------------------------------------------------------
        // PROVERE (koriste se i u istoriji)
        // ------------------------------------------------------------------

        // broji pozajmice vracene nakon roka + one koje su nevracene a rok je prosao
        public static int BrojKasnjenja(int korisnikId)
        {
            string sql = @"
                SELECT COUNT(*) FROM pozajmice
                WHERE korisnik_id = @korisnik
                  AND ( (datum_vracanja IS NOT NULL AND datum_vracanja > rok_vracanja)
                     OR (datum_vracanja IS NULL AND rok_vracanja < CURDATE()) )";

            using (var conn = new MySqlConnection(Db.ConnStr))
            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@korisnik", korisnikId);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        // ------------------------------------------------------------------
        // IZGLED
        // ------------------------------------------------------------------

        private void IzgradiIzgled()
        {
            Text = "Biblioteka - Korisnik";
            ClientSize = new Size(1000, 690);
            MinimumSize = new Size(900, 650);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10f);

            // leva tamna traka sa naslovom
            var pnlSide = new Panel { Dock = DockStyle.Left, Width = 188, BackColor = Navy };
            pnlSide.Controls.Add(new Label
            {
                Text = "BIBLIOTEKA",
                ForeColor = Zlatna,
                Font = new Font("Georgia", 15f, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 70),
                Size = new Size(188, 40)
            });
            pnlSide.Controls.Add(new Panel { BackColor = Zlatna, Location = new Point(30, 116), Size = new Size(128, 2) });

            var pnlGlavni = new Panel { Dock = DockStyle.Fill, BackColor = Krem };
            Controls.Add(pnlGlavni);
            Controls.Add(pnlSide);

            // zaglavlje sa pozdravom
            var pnlHeader = new RoundedPanel
            {
                FillColor = Navy,
                Radius = 14,
                Location = new Point(18, 18),
                Size = new Size(776, 62),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            pnlHeader.Controls.Add(new Label
            {
                Text = "\uE77B",
                Font = new Font("Segoe MDL2 Assets", 26f),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(18, 14)
            });
            lblDobrodosli = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Location = new Point(80, 12)
            };
            pnlHeader.Controls.Add(lblDobrodosli);

            // traka sa pretragom i filterom
            var pnlFilter = new RoundedPanel
            {
                FillColor = Color.FromArgb(247, 245, 240),
                BorderColor = Color.FromArgb(214, 210, 200),
                Radius = 12,
                Location = new Point(18, 92),
                Size = new Size(776, 56),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            pnlFilter.Controls.Add(new Label
            {
                Text = "\uE721",
                Font = new Font("Segoe MDL2 Assets", 16f),
                ForeColor = Tamna,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(18, 15)
            });

            txtPretraga = new TextBox
            {
                Font = new Font("Segoe UI", 11f),
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(54, 13),
                Width = 340
            };
            txtPretraga.TextChanged += txtPretraga_TextChanged;
            pnlFilter.Controls.Add(txtPretraga);
            txtPretraga.HandleCreated += (s, a) => txtPretraga.SetPlaceholder("Pretraži knjige...");

            pnlFilter.Controls.Add(new Panel { BackColor = Tamna, Location = new Point(412, 10), Size = new Size(1, 36) });
            pnlFilter.Controls.Add(new Label
            {
                Text = "\uE71C",
                Font = new Font("Segoe MDL2 Assets", 16f),
                ForeColor = Tamna,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(430, 15)
            });

            cmbKategorija = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 11f),
                Location = new Point(466, 13),
                Width = 210
            };
            cmbKategorija.SelectedIndexChanged += cmbKategorija_SelectedIndexChanged;
            pnlFilter.Controls.Add(cmbKategorija);

            // tabela sa dostupnim knjigama (gornja)
            dgvKnjige = new DataGridView
            {
                Location = new Point(18, 162),
                Size = new Size(776, 230),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                GridColor = Color.FromArgb(208, 213, 221),
                CellBorderStyle = DataGridViewCellBorderStyle.Single,
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 38
            };
            dgvKnjige.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(221, 227, 238);
            dgvKnjige.ColumnHeadersDefaultCellStyle.ForeColor = Tamna;
            dgvKnjige.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(221, 227, 238);
            dgvKnjige.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            dgvKnjige.DefaultCellStyle.Font = new Font("Segoe UI", 11f);
            dgvKnjige.DefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
            dgvKnjige.DefaultCellStyle.SelectionBackColor = Color.FromArgb(11, 107, 211);
            dgvKnjige.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvKnjige.RowTemplate.Height = 34;

            // dugmad pri dnu: Pozajmi (levo), Vrati (sredina), Odjava (desno)
            btnPozajmi = new RoundedButton
            {
                Text = "Pozajmi knjigu",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.White,
                FillColor = Navy,
                HoverColor = Color.FromArgb(35, 60, 105),
                Location = new Point(18, 600),
                Size = new Size(240, 56),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            btnPozajmi.Click += btnPozajmi_Click;

            btnVrati = new RoundedButton
            {
                Text = "Vrati knjigu",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Navy,
                FillColor = Zlatna,
                HoverColor = Color.FromArgb(214, 168, 86),
                Location = new Point(274, 600),
                Size = new Size(240, 56),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            btnVrati.Click += btnVrati_Click;

            btnOdjava = new RoundedButton
            {
                Text = "Odjava",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Navy,
                FillColor = Color.FromArgb(253, 252, 250),
                HoverColor = Color.FromArgb(232, 236, 244),
                BorderColor = Navy,
                BorderWidth = 2,
                Location = new Point(644, 600),
                Size = new Size(150, 56),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            btnOdjava.Click += btnOdjava_Click;

            pnlGlavni.Controls.Add(pnlHeader);
            pnlGlavni.Controls.Add(pnlFilter);
            pnlGlavni.Controls.Add(dgvKnjige);
            pnlGlavni.Controls.Add(btnPozajmi);
            pnlGlavni.Controls.Add(btnVrati);
            pnlGlavni.Controls.Add(btnOdjava);

            DodajMojePozajmice(pnlGlavni);
        }

        private void DodajMojePozajmice(Panel pnlGlavni)
        {
            var lbl = new Label
            {
                Text = "Moje pozajmice",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Tamna,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(18, 404),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };

            // link koji otvara celu istoriju
            var lnkIstorija = new LinkLabel
            {
                Text = "Istorija pozajmica →",
                AutoSize = false,
                Size = new Size(190, 24),
                TextAlign = ContentAlignment.MiddleRight,
                Location = new Point(604, 404),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                LinkColor = Color.FromArgb(11, 80, 160),
                ActiveLinkColor = Navy,
                VisitedLinkColor = Color.FromArgb(11, 80, 160),
                LinkBehavior = LinkBehavior.HoverUnderline,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 10.5f)
            };
            lnkIstorija.LinkClicked += (s, e) =>
            {
                using (var f = new FrmIstorija())
                    f.ShowDialog(this);
            };

            dgvMoje = new DataGridView
            {
                Location = new Point(18, 432),
                Size = new Size(776, 150),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToResizeRows = false,
                MultiSelect = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                GridColor = Color.FromArgb(208, 213, 221),
                CellBorderStyle = DataGridViewCellBorderStyle.Single,
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 34
            };
            dgvMoje.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(221, 227, 238);
            dgvMoje.ColumnHeadersDefaultCellStyle.ForeColor = Tamna;
            dgvMoje.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(221, 227, 238);
            dgvMoje.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            dgvMoje.DefaultCellStyle.Font = new Font("Segoe UI", 10.5f);
            dgvMoje.DefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
            dgvMoje.DefaultCellStyle.SelectionBackColor = Color.FromArgb(11, 107, 211);
            dgvMoje.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvMoje.RowTemplate.Height = 30;

            pnlGlavni.Controls.Add(lbl);
            pnlGlavni.Controls.Add(lnkIstorija);
            pnlGlavni.Controls.Add(dgvMoje);
        }

        // ------------------------------------------------------------------
        // PODACI
        // ------------------------------------------------------------------

        private void UcitajKategorije()
        {
            var dt = new DataTable();
            using (var conn = new MySqlConnection(Db.ConnStr))
            {
                new MySqlDataAdapter("SELECT id, naziv FROM kategorije ORDER BY naziv", conn).Fill(dt);
            }

            DataRow sve = dt.NewRow();
            sve["id"] = 0;
            sve["naziv"] = "Sve kategorije";
            dt.Rows.InsertAt(sve, 0);

            cmbKategorija.DisplayMember = "naziv";
            cmbKategorija.ValueMember = "id";
            cmbKategorija.DataSource = dt;
        }

        private void UcitajKnjige()
        {
            if (!_spremno) return;

            string sql = @"
                SELECT k.id, k.naslov AS Naslov, k.autor AS Autor, k.godina_izdanja AS Godina,
                       c.naziv AS Kategorija, k.kolicina - COUNT(p.id) AS Dostupno
                FROM knjige k
                JOIN kategorije c ON c.id = k.kategorija_id
                LEFT JOIN pozajmice p ON p.knjiga_id = k.id AND p.datum_vracanja IS NULL
                WHERE (k.naslov LIKE @q OR k.autor LIKE @q)
                  AND (@kat = 0 OR k.kategorija_id = @kat)
                GROUP BY k.id, k.naslov, k.autor, k.godina_izdanja, c.naziv, k.kolicina
                HAVING Dostupno > 0
                ORDER BY k.naslov";

            try
            {
                var dt = new DataTable();
                using (var conn = new MySqlConnection(Db.ConnStr))
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@q", "%" + txtPretraga.Text.Trim() + "%");
                    cmd.Parameters.AddWithValue("@kat", Convert.ToInt32(cmbKategorija.SelectedValue));
                    new MySqlDataAdapter(cmd).Fill(dt);
                }

                dgvKnjige.DataSource = dt;
                dgvKnjige.Columns["id"].Visible = false;
                PrilagodiKolone();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Greška sa bazom: " + ex.Message);
            }
        }

        private void PrilagodiKolone()
        {
            dgvKnjige.Columns["Naslov"].FillWeight = 28;
            dgvKnjige.Columns["Autor"].FillWeight = 28;
            dgvKnjige.Columns["Godina"].FillWeight = 12;
            dgvKnjige.Columns["Kategorija"].FillWeight = 20;
            dgvKnjige.Columns["Dostupno"].FillWeight = 12;
            dgvKnjige.Columns["Dostupno"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvKnjige.Columns["Dostupno"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        private void UcitajMoje()
        {
            string sql = @"
                SELECT p.id, k.naslov AS Naslov, k.autor AS Autor,
                       p.datum_pozajmice AS Pozajmljeno, p.rok_vracanja AS Rok,
                       DATEDIFF(p.rok_vracanja, CURDATE()) AS Preostalo
                FROM pozajmice p
                JOIN knjige k ON k.id = p.knjiga_id
                WHERE p.korisnik_id = @korisnik AND p.datum_vracanja IS NULL
                ORDER BY p.rok_vracanja";

            try
            {
                var dt = new DataTable();
                using (var conn = new MySqlConnection(Db.ConnStr))
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@korisnik", Sesija.Id);
                    new MySqlDataAdapter(cmd).Fill(dt);
                }

                dgvMoje.DataSource = dt;

                dgvMoje.Columns["id"].Visible = false;
                dgvMoje.Columns["Pozajmljeno"].DefaultCellStyle.Format = "dd.MM.yyyy";
                dgvMoje.Columns["Rok"].DefaultCellStyle.Format = "dd.MM.yyyy";
                dgvMoje.Columns["Rok"].HeaderText = "Rok vraćanja";
                dgvMoje.Columns["Preostalo"].HeaderText = "Preostalo dana";
                dgvMoje.Columns["Preostalo"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgvMoje.Columns["Preostalo"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

                // rok je prosao, red ide u crveno
                foreach (DataGridViewRow red in dgvMoje.Rows)
                {
                    if (Convert.ToInt32(red.Cells["Preostalo"].Value) < 0)
                    {
                        red.DefaultCellStyle.BackColor = Color.MistyRose;
                        red.DefaultCellStyle.ForeColor = Color.Firebrick;
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Greška sa bazom: " + ex.Message);
            }
        }

        private void txtPretraga_TextChanged(object sender, EventArgs e) { UcitajKnjige(); }
        private void cmbKategorija_SelectedIndexChanged(object sender, EventArgs e) { UcitajKnjige(); }

        // ------------------------------------------------------------------
        // AKCIJE
        // ------------------------------------------------------------------

        private void btnPozajmi_Click(object sender, EventArgs e)
        {
            if (dgvKnjige.CurrentRow == null)
            {
                MessageBox.Show("Izaberi knjigu iz liste.");
                return;
            }

            int knjigaId = Convert.ToInt32(dgvKnjige.CurrentRow.Cells["id"].Value);
            string naslov = Convert.ToString(dgvKnjige.CurrentRow.Cells["Naslov"].Value);

            try
            {
                // 1) provera kasnjenja
                int kasnjenja = BrojKasnjenja(Sesija.Id);
                if (kasnjenja >= MaxKasnjenja)
                {
                    MessageBox.Show(
                        "Ne možeš da pozajmiš knjigu jer si kasnio sa vraćanjem " + kasnjenja +
                        " puta, a dozvoljeno je manje od " + MaxKasnjenja + ".\n\nObrati se bibliotekaru.",
                        "Pozajmljivanje blokirano", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 2) provera da isti korisnik vec nema ovu knjigu kod sebe
                int vecImas;
                using (var conn = new MySqlConnection(Db.ConnStr))
                using (var cmd = new MySqlCommand(
                    @"SELECT COUNT(*) FROM pozajmice
                      WHERE korisnik_id = @korisnik AND knjiga_id = @knjiga AND datum_vracanja IS NULL", conn))
                {
                    cmd.Parameters.AddWithValue("@korisnik", Sesija.Id);
                    cmd.Parameters.AddWithValue("@knjiga", knjigaId);
                    conn.Open();
                    vecImas = Convert.ToInt32(cmd.ExecuteScalar());
                }

                if (vecImas > 0)
                {
                    MessageBox.Show(
                        "Knjigu „" + naslov + "“ već imaš pozajmljenu. Prvo je vrati pa je možeš ponovo uzeti.",
                        "Već pozajmljeno", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 3) upis, uslovi su ponovo u samom upitu da dva korisnika ne uzmu poslednji primerak
                string sql = @"
                    INSERT INTO pozajmice (korisnik_id, knjiga_id, datum_pozajmice, rok_vracanja)
                    SELECT @korisnik, @knjiga, CURDATE(), DATE_ADD(CURDATE(), INTERVAL 14 DAY)
                    FROM knjige k
                    WHERE k.id = @knjiga
                      AND k.kolicina > (SELECT COUNT(*) FROM pozajmice
                                        WHERE knjiga_id = @knjiga AND datum_vracanja IS NULL)
                      AND NOT EXISTS (SELECT 1 FROM pozajmice
                                      WHERE knjiga_id = @knjiga AND korisnik_id = @korisnik
                                        AND datum_vracanja IS NULL)";

                int upisano;
                using (var conn = new MySqlConnection(Db.ConnStr))
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@korisnik", Sesija.Id);
                    cmd.Parameters.AddWithValue("@knjiga", knjigaId);
                    conn.Open();
                    upisano = cmd.ExecuteNonQuery();
                }

                if (upisano == 0)
                    MessageBox.Show("Knjiga više nije dostupna.");
                else
                    MessageBox.Show("Pozajmljeno. Rok za vraćanje je 14 dana.");

                UcitajKnjige();
                UcitajMoje();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Greška sa bazom: " + ex.Message);
            }
        }

        private void btnVrati_Click(object sender, EventArgs e)
        {
            if (dgvMoje.CurrentRow == null)
            {
                MessageBox.Show("Izaberi knjigu iz tabele „Moje pozajmice“.");
                return;
            }

            int pozajmicaId = Convert.ToInt32(dgvMoje.CurrentRow.Cells["id"].Value);
            string naslov = dgvMoje.CurrentRow.Cells["Naslov"].Value.ToString();

            var odgovor = MessageBox.Show(
                "Da li želiš da vratiš knjigu „" + naslov + "“?",
                "Vraćanje knjige", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (odgovor != DialogResult.Yes) return;

            // pozajmica ostaje u bazi kao istorija, samo dobija datum vracanja
            string sql = @"UPDATE pozajmice SET datum_vracanja = CURDATE()
                           WHERE id = @id AND korisnik_id = @korisnik AND datum_vracanja IS NULL";

            try
            {
                int izmenjeno;
                using (var conn = new MySqlConnection(Db.ConnStr))
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", pozajmicaId);
                    cmd.Parameters.AddWithValue("@korisnik", Sesija.Id);
                    conn.Open();
                    izmenjeno = cmd.ExecuteNonQuery();
                }

                if (izmenjeno == 0)
                    MessageBox.Show("Ova knjiga je već vraćena.");
                else
                    MessageBox.Show("Knjiga je vraćena.");

                UcitajKnjige();
                UcitajMoje();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Greška sa bazom: " + ex.Message);
            }
        }

        private void btnOdjava_Click(object sender, EventArgs e)
        {
            Sesija.Id = 0;
            Sesija.Ime = null;
            Sesija.Uloga = null;
            this.Close();
        }
    }

    // ----------------------------------------------------------------------
    // PROZOR SA ISTORIJOM POZAJMICA (pravi se iz koda, nema Designer fajl)
    // ----------------------------------------------------------------------

    public class FrmIstorija : Form
    {
        private static readonly Color Navy = Color.FromArgb(23, 43, 77);
        private static readonly Color Krem = Color.FromArgb(242, 239, 233);
        private static readonly Color Tamna = Color.FromArgb(28, 39, 51);
        private static readonly Color Siva = Color.FromArgb(91, 104, 119);

        private readonly DataGridView _grid;
        private readonly Label _sazetak;

        public FrmIstorija()
        {
            Text = "Istorija pozajmica";
            ClientSize = new Size(860, 540);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Krem;
            Font = new Font("Segoe UI", 10f);

            var naslov = new Label
            {
                Text = "Istorija pozajmica",
                Font = new Font("Georgia", 20f, FontStyle.Bold),
                ForeColor = Tamna,
                AutoSize = true,
                Location = new Point(24, 18)
            };

            _sazetak = new Label
            {
                Font = new Font("Segoe UI", 11f),
                ForeColor = Siva,
                AutoSize = true,
                Location = new Point(27, 62)
            };

            _grid = new DataGridView
            {
                Location = new Point(24, 100),
                Size = new Size(812, 370),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToResizeRows = false,
                MultiSelect = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                GridColor = Color.FromArgb(208, 213, 221),
                CellBorderStyle = DataGridViewCellBorderStyle.Single,
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 36
            };
            _grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(221, 227, 238);
            _grid.ColumnHeadersDefaultCellStyle.ForeColor = Tamna;
            _grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(221, 227, 238);
            _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            _grid.DefaultCellStyle.Font = new Font("Segoe UI", 10.5f);
            _grid.DefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
            _grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(11, 107, 211);
            _grid.DefaultCellStyle.SelectionForeColor = Color.White;
            _grid.RowTemplate.Height = 32;

            var btnZatvori = new RoundedButton
            {
                Text = "Zatvori",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.White,
                FillColor = Navy,
                HoverColor = Color.FromArgb(35, 60, 105),
                Location = new Point(686, 484),
                Size = new Size(150, 42),
                Radius = 10
            };
            btnZatvori.Click += (s, e) => Close();
            CancelButton = btnZatvori;   // Esc zatvara prozor

            Controls.Add(naslov);
            Controls.Add(_sazetak);
            Controls.Add(_grid);
            Controls.Add(btnZatvori);

            Ucitaj();
        }

        private void Ucitaj()
        {
            string sql = @"
                SELECT k.naslov AS Naslov, k.autor AS Autor,
                       p.datum_pozajmice AS Pozajmljeno, p.rok_vracanja AS Rok, p.datum_vracanja AS Vraceno,
                       CASE
                         WHEN p.datum_vracanja IS NULL AND p.rok_vracanja < CURDATE() THEN 'Kasni'
                         WHEN p.datum_vracanja IS NULL THEN 'U roku'
                         WHEN p.datum_vracanja > p.rok_vracanja THEN 'Vraćeno kasno'
                         ELSE 'Vraćeno na vreme'
                       END AS Status
                FROM pozajmice p
                JOIN knjige k ON k.id = p.knjiga_id
                WHERE p.korisnik_id = @korisnik
                ORDER BY p.datum_pozajmice DESC, p.id DESC";

            try
            {
                var dt = new DataTable();
                using (var conn = new MySqlConnection(Db.ConnStr))
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@korisnik", Sesija.Id);
                    new MySqlDataAdapter(cmd).Fill(dt);
                }

                _grid.DataSource = dt;
                _grid.Columns["Rok"].HeaderText = "Rok vraćanja";
                _grid.Columns["Vraceno"].HeaderText = "Vraćeno";
                _grid.Columns["Pozajmljeno"].DefaultCellStyle.Format = "dd.MM.yyyy.";
                _grid.Columns["Rok"].DefaultCellStyle.Format = "dd.MM.yyyy.";
                _grid.Columns["Vraceno"].DefaultCellStyle.Format = "dd.MM.yyyy.";
                _grid.Columns["Naslov"].FillWeight = 28;
                _grid.Columns["Autor"].FillWeight = 20;
                _grid.Columns["Pozajmljeno"].FillWeight = 14;
                _grid.Columns["Rok"].FillWeight = 14;
                _grid.Columns["Vraceno"].FillWeight = 14;
                _grid.Columns["Status"].FillWeight = 18;

                foreach (DataGridViewRow red in _grid.Rows)
                {
                    string status = Convert.ToString(red.Cells["Status"].Value);
                    if (status == "Kasni")
                    {
                        red.DefaultCellStyle.BackColor = Color.MistyRose;
                        red.DefaultCellStyle.ForeColor = Color.Firebrick;
                    }
                    else if (status == "Vraćeno kasno")
                    {
                        red.DefaultCellStyle.BackColor = Color.FromArgb(255, 243, 220);
                        red.DefaultCellStyle.ForeColor = Color.FromArgb(160, 90, 0);
                    }
                    else if (status == "Vraćeno na vreme")
                    {
                        red.DefaultCellStyle.ForeColor = Siva;
                    }
                }

                int kasnjenja = FrmKorisnik.BrojKasnjenja(Sesija.Id);
                string tekst = "Ukupno pozajmica: " + dt.Rows.Count +
                               "     |     Kašnjenja: " + kasnjenja + " (blokada pri " + FrmKorisnik.MaxKasnjenja + ")";

                if (kasnjenja >= FrmKorisnik.MaxKasnjenja)
                {
                    tekst += "  –  pozajmljivanje je blokirano";
                    _sazetak.ForeColor = Color.Firebrick;
                }

                _sazetak.Text = tekst;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Greška sa bazom: " + ex.Message);
            }
        }
    }
}