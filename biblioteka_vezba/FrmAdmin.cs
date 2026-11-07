using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace biblioteka_vezba
{
    public partial class FrmAdmin : Form
    {
        // boje
        private static readonly Color Navy = Color.FromArgb(23, 43, 77);
        private static readonly Color Krem = Color.FromArgb(242, 239, 233);
        private static readonly Color Zlatna = Color.FromArgb(229, 185, 107);
        private static readonly Color Tamna = Color.FromArgb(28, 39, 51);
        private static readonly Color Siva = Color.FromArgb(91, 104, 119);
        private static readonly Color Crvena = Color.FromArgb(214, 40, 40);
        private static readonly Color KarticaBoja = Color.FromArgb(247, 245, 240);
        private static readonly Color Okvir = Color.FromArgb(214, 210, 200);

        // zaglavlje i navigacija
        private Label _lblPozdrav, _lblDatum, _lblVreme;
        private Timer _sat;
        private readonly Panel[] _stranice = new Panel[4];
        private readonly NavButton[] _nav = new NavButton[4];
        private NavButton _navOdjava;

        // pocetna
        private Label _valKnjiga, _valKorisnika, _valAktivne, _valKasnjenja;
        private DonutPanel _donut;
        private Label _lblDostupne, _lblPozajmljene, _lblKasnjenjaLeg, _lblUkupno;
        private DataGridView _gridPoslednje, _gridKorisniciMali;
        private BarListPanel _barovi;

        // ostale stranice
        private DataGridView _gridKorisnici, _gridKnjige, _gridPozajmice;
        private TextBox _txtNaslov, _txtAutor;
        private NumericUpDown _numGodina, _numKolicina;
        private ComboBox _cmbKategorija;
        private CheckBox _chkAktivne;

        private bool _gotovo = false;
        private bool _spremno = false;

        public FrmAdmin()
        {
            InitializeComponent();

            // login otvara ovu formu samo za admina, ovo je dodatna zastita
            if (Sesija.Uloga != "admin")
            {
                Load += (s, e) => Close();
                return;
            }

            Inicijalizuj();
        }

        // prazne metode, samo da Designer.cs ne baca gresku ako ih jos pominje
        private void FrmAdmin_Load(object sender, EventArgs e) { }
        private void label1_Click(object sender, EventArgs e) { }

        private void Inicijalizuj()
        {
            if (_gotovo) return;
            _gotovo = true;

            IzgradiIzgled();
            UcitajKategorije();
            _spremno = true;
            Prikazi(0);
            OsveziSve();
        }

        // ------------------------------------------------------------------
        // IZGLED
        // ------------------------------------------------------------------

        private void IzgradiIzgled()
        {
            Text = "Biblioteka - Administracija";
            ClientSize = new Size(1200, 740);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10f);
            BackColor = Krem;

            var pnlSadrzaj = new Panel { Dock = DockStyle.Fill, BackColor = Krem };
            var pnlSide = new Panel { Dock = DockStyle.Left, Width = 200, BackColor = Navy };
            Controls.Add(pnlSadrzaj);
            Controls.Add(pnlSide);

            IzgradiSidebar(pnlSide);
            IzgradiZaglavlje(pnlSadrzaj);

            var pnlStranice = new Panel { Location = new Point(0, 110), Size = new Size(1000, 630), BackColor = Krem };
            pnlSadrzaj.Controls.Add(pnlStranice);

            for (int i = 0; i < _stranice.Length; i++)
            {
                _stranice[i] = new Panel { Dock = DockStyle.Fill, BackColor = Krem, Visible = false };
                pnlStranice.Controls.Add(_stranice[i]);
            }

            IzgradiPocetnu(_stranice[0]);
            IzgradiKorisnike(_stranice[1]);
            IzgradiKnjige(_stranice[2]);
            IzgradiPozajmice(_stranice[3]);
        }

        private void IzgradiSidebar(Panel pnlSide)
        {
            pnlSide.Controls.Add(new Label
            {
                Text = "BIBLIOTEKA",
                ForeColor = Zlatna,
                Font = new Font("Georgia", 15f, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 36),
                Size = new Size(200, 40)
            });
            pnlSide.Controls.Add(new Panel { BackColor = Zlatna, Location = new Point(36, 82), Size = new Size(128, 2) });

            string[] imena = { "Početna", "Korisnici", "Knjige", "Pozajmice" };
            string[] glifovi = { "\uE80F", "\uE716", "\uE8F1", "\uE8AB" };

            for (int i = 0; i < 4; i++)
            {
                int idx = i;
                _nav[i] = new NavButton
                {
                    Glyph = glifovi[i],
                    Text = imena[i],
                    Location = new Point(12, 120 + i * 54),
                    Size = new Size(176, 44)
                };
                _nav[i].Click += (s, e) => { Prikazi(idx); OsveziSve(); };
                pnlSide.Controls.Add(_nav[i]);
            }

            _navOdjava = new NavButton
            {
                Glyph = "\uE7E8",
                Text = "Odjava",
                Location = new Point(12, 676),
                Size = new Size(176, 44),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            _navOdjava.Click += Odjava;
            pnlSide.Controls.Add(_navOdjava);
        }

        private void IzgradiZaglavlje(Panel pnlSadrzaj)
        {
            var pnlHeader = new RoundedPanel
            {
                BackColor = Krem,
                FillColor = Navy,
                Radius = 14,
                Location = new Point(20, 18),
                Size = new Size(960, 80)
            };

            pnlHeader.Controls.Add(new Label
            {
                Text = "\uE77B",
                Font = new Font("Segoe MDL2 Assets", 30f),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(22, 16)
            });
            pnlHeader.Controls.Add(new Panel { BackColor = Color.FromArgb(70, 92, 128), Location = new Point(84, 16), Size = new Size(1, 48) });

            _lblPozdrav = new Label
            {
                Text = "Dobrodošao, " + Sesija.Ime + "!",
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(98, 8)
            };
            pnlHeader.Controls.Add(_lblPozdrav);

            pnlHeader.Controls.Add(new Label
            {
                Text = "Upravljajte bibliotekom na jednostavan način.",
                Font = new Font("Segoe UI", 11f),
                ForeColor = Color.FromArgb(200, 210, 225),
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(100, 46)
            });

            pnlHeader.Controls.Add(new Label
            {
                Text = "\uE787",
                Font = new Font("Segoe MDL2 Assets", 14f),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(790, 14)
            });
            _lblDatum = new Label
            {
                Font = new Font("Segoe UI", 12f),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(822, 12)
            };
            pnlHeader.Controls.Add(_lblDatum);

            pnlHeader.Controls.Add(new Label
            {
                Text = "\uE121",
                Font = new Font("Segoe MDL2 Assets", 14f),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(790, 44)
            });
            _lblVreme = new Label
            {
                Font = new Font("Segoe UI", 12f),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(822, 42)
            };
            pnlHeader.Controls.Add(_lblVreme);

            pnlSadrzaj.Controls.Add(pnlHeader);

            _sat = new Timer { Interval = 1000 };
            _sat.Tick += (s, e) => AzurirajSat();
            AzurirajSat();
            _sat.Start();
            FormClosed += (s, e) => { _sat.Stop(); _sat.Dispose(); };
        }

        private void AzurirajSat()
        {
            DateTime sada = DateTime.Now;
            _lblDatum.Text = sada.ToString("dd.MM.yyyy.");
            _lblVreme.Text = sada.ToString("HH:mm");
        }

        private void IzgradiPocetnu(Panel p)
        {
            _valKnjiga = NapraviStatKarticu(p, 20, "\uE8F1", "Ukupno knjiga", Navy);
            _valKorisnika = NapraviStatKarticu(p, 264, "\uE716", "Ukupno korisnika", Navy);
            _valAktivne = NapraviStatKarticu(p, 508, "\uE8AB", "Aktivne pozajmice", Navy);
            _valKasnjenja = NapraviStatKarticu(p, 752, "\uE783", "Kašnjenja", Crvena);

            // Stanje knjiga
            var kStanje = NapraviKarticu(p, 20, 100, 440, 250, "\uE8F1", "Stanje knjiga", null);
            _donut = new DonutPanel { Location = new Point(20, 52), Size = new Size(180, 180), BackColor = KarticaBoja };
            kStanje.Controls.Add(_donut);

            _lblDostupne = NapraviLegendu(kStanje, 76, "Dostupne", Navy);
            _lblPozajmljene = NapraviLegendu(kStanje, 110, "Pozajmljene", Zlatna);
            _lblKasnjenjaLeg = NapraviLegendu(kStanje, 144, "Kašnjenja", Crvena);

            kStanje.Controls.Add(new Panel { BackColor = Okvir, Location = new Point(232, 184), Size = new Size(188, 1) });
            kStanje.Controls.Add(new Label
            {
                Text = "Ukupno:",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Tamna,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(250, 196)
            });
            _lblUkupno = new Label
            {
                Text = "0",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Tamna,
                BackColor = Color.Transparent,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleRight,
                Size = new Size(70, 22),
                Location = new Point(350, 195)
            };
            kStanje.Controls.Add(_lblUkupno);

            // Poslednje pozajmice
            var kPoz = NapraviKarticu(p, 476, 100, 504, 250, "\uE121", "Poslednje pozajmice",
                () => { _chkAktivne.Checked = false; Prikazi(3); });
            _gridPoslednje = NapraviGrid(12, 48, 480, 190, true);
            kPoz.Controls.Add(_gridPoslednje);

            // Korisnici biblioteke
            var kKor = NapraviKarticu(p, 20, 366, 470, 250, "\uE716", "Korisnici biblioteke", () => Prikazi(1));
            _gridKorisniciMali = NapraviGrid(12, 48, 446, 190, true);
            kKor.Controls.Add(_gridKorisniciMali);

            // Najtrazenije knjige
            var kTop = NapraviKarticu(p, 506, 366, 474, 250, "\uE734", "Najtraženije knjige", () => Prikazi(2));
            _barovi = new BarListPanel { Location = new Point(12, 48), Size = new Size(450, 190), BackColor = KarticaBoja };
            kTop.Controls.Add(_barovi);
        }

        private void IzgradiKorisnike(Panel p)
        {
            _gridKorisnici = NapraviGrid(20, 0, 960, 616, false);
            p.Controls.Add(_gridKorisnici);
        }

        private void IzgradiKnjige(Panel p)
        {
            var forma = new RoundedPanel
            {
                BackColor = Krem,
                FillColor = KarticaBoja,
                BorderColor = Okvir,
                Radius = 12,
                Location = new Point(20, 0),
                Size = new Size(960, 104)
            };

            forma.Controls.Add(new Label
            {
                Text = "Dodaj novu knjigu",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Tamna,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(16, 8)
            });

            Natpis(forma, "Naslov", 16, 36);
            Natpis(forma, "Autor", 258, 36);
            Natpis(forma, "Godina", 440, 36);
            Natpis(forma, "Količina", 532, 36);
            Natpis(forma, "Kategorija", 614, 36);

            _txtNaslov = new TextBox { Font = new Font("Segoe UI", 11f), Location = new Point(16, 58), Width = 230 };
            _txtAutor = new TextBox { Font = new Font("Segoe UI", 11f), Location = new Point(258, 58), Width = 170 };
            _numGodina = new NumericUpDown
            {
                Font = new Font("Segoe UI", 11f),
                Location = new Point(440, 58),
                Width = 80,
                Minimum = 1000,
                Maximum = 2100,
                Value = 2020
            };
            _numKolicina = new NumericUpDown
            {
                Font = new Font("Segoe UI", 11f),
                Location = new Point(532, 58),
                Width = 70,
                Minimum = 1,
                Maximum = 1000,
                Value = 1
            };
            _cmbKategorija = new ComboBox
            {
                Font = new Font("Segoe UI", 11f),
                Location = new Point(614, 58),
                Width = 160,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            var btnDodaj = new RoundedButton
            {
                Text = "Dodaj knjigu",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.White,
                FillColor = Navy,
                HoverColor = Color.FromArgb(35, 60, 105),
                Location = new Point(786, 54),
                Size = new Size(158, 38),
                Radius = 10
            };
            btnDodaj.Click += DodajKnjigu;

            forma.Controls.Add(_txtNaslov);
            forma.Controls.Add(_txtAutor);
            forma.Controls.Add(_numGodina);
            forma.Controls.Add(_numKolicina);
            forma.Controls.Add(_cmbKategorija);
            forma.Controls.Add(btnDodaj);
            p.Controls.Add(forma);

            _gridKnjige = NapraviGrid(20, 116, 960, 500, false);
            p.Controls.Add(_gridKnjige);
        }

        private void IzgradiPozajmice(Panel p)
        {
            _chkAktivne = new CheckBox
            {
                Text = "Samo aktivne pozajmice",
                Checked = true,
                AutoSize = true,
                Font = new Font("Segoe UI", 11f),
                ForeColor = Tamna,
                Location = new Point(20, 10)
            };
            _chkAktivne.CheckedChanged += (s, e) =>
            {
                if (!_spremno) return;
                try { UcitajPozajmice(); }
                catch (MySqlException ex) { MessageBox.Show("Greška sa bazom: " + ex.Message); }
            };
            p.Controls.Add(_chkAktivne);

            var btnVrati = new RoundedButton
            {
                Text = "Označi kao vraćeno",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Navy,
                FillColor = Zlatna,
                HoverColor = Color.FromArgb(214, 168, 86),
                Location = new Point(740, 0),
                Size = new Size(240, 40),
                Radius = 10
            };
            btnVrati.Click += VratiKnjigu;
            p.Controls.Add(btnVrati);

            _gridPozajmice = NapraviGrid(20, 52, 960, 564, false);
            p.Controls.Add(_gridPozajmice);
        }

        // ------------------------------------------------------------------
        // POMOCNE METODE ZA IZGLED
        // ------------------------------------------------------------------

        private Label NapraviStatKarticu(Panel roditelj, int x, string glyph, string naslov, Color bojaIkone)
        {
            var k = new RoundedPanel
            {
                BackColor = Krem,
                FillColor = KarticaBoja,
                BorderColor = Okvir,
                Radius = 12,
                Location = new Point(x, 0),
                Size = new Size(228, 84)
            };
            k.Controls.Add(new Label
            {
                Text = glyph,
                Font = new Font("Segoe MDL2 Assets", 28f),
                ForeColor = bojaIkone,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(14, 20)
            });
            k.Controls.Add(new Label
            {
                Text = naslov,
                Font = new Font("Segoe UI", 10.5f),
                ForeColor = Siva,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(72, 12)
            });
            var vrednost = new Label
            {
                Text = "0",
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                ForeColor = Tamna,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(70, 34)
            };
            k.Controls.Add(vrednost);
            roditelj.Controls.Add(k);
            return vrednost;
        }

        private RoundedPanel NapraviKarticu(Panel roditelj, int x, int y, int w, int h, string glyph, string naslov, Action naPrikaziSve)
        {
            var k = new RoundedPanel
            {
                BackColor = Krem,
                FillColor = KarticaBoja,
                BorderColor = Okvir,
                Radius = 12,
                Location = new Point(x, y),
                Size = new Size(w, h)
            };
            k.Controls.Add(new Label
            {
                Text = glyph,
                Font = new Font("Segoe MDL2 Assets", 14f),
                ForeColor = Navy,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(14, 12)
            });
            k.Controls.Add(new Label
            {
                Text = naslov,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Tamna,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(44, 10)
            });

            if (naPrikaziSve != null)
            {
                var link = new LinkLabel
                {
                    Text = "Prikaži sve →",
                    AutoSize = false,
                    Size = new Size(110, 24),
                    TextAlign = ContentAlignment.MiddleRight,
                    Location = new Point(w - 124, 10),
                    LinkColor = Color.FromArgb(11, 80, 160),
                    ActiveLinkColor = Navy,
                    VisitedLinkColor = Color.FromArgb(11, 80, 160),
                    LinkBehavior = LinkBehavior.HoverUnderline,
                    BackColor = Color.Transparent,
                    Font = new Font("Segoe UI", 10f)
                };
                link.LinkClicked += (s, e) => naPrikaziSve();
                k.Controls.Add(link);
            }

            roditelj.Controls.Add(k);
            return k;
        }

        private Label NapraviLegendu(RoundedPanel kartica, int y, string naziv, Color boja)
        {
            kartica.Controls.Add(new Label
            {
                Text = "●",
                ForeColor = boja,
                Font = new Font("Segoe UI", 12f),
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(226, y - 3)
            });
            kartica.Controls.Add(new Label
            {
                Text = naziv,
                Font = new Font("Segoe UI", 11f),
                ForeColor = Tamna,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(250, y)
            });
            var v = new Label
            {
                Text = "0",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Tamna,
                BackColor = Color.Transparent,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleRight,
                Size = new Size(70, 22),
                Location = new Point(350, y - 1)
            };
            kartica.Controls.Add(v);
            return v;
        }

        private void Natpis(Control roditelj, string tekst, int x, int y)
        {
            roditelj.Controls.Add(new Label
            {
                Text = tekst,
                Font = new Font("Segoe UI", 9f),
                ForeColor = Siva,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(x, y)
            });
        }

        // mala = true: tabela za male kartice na pocetnoj (bez skrolbara i bez oznacenog reda)
        private DataGridView NapraviGrid(int x, int y, int w, int h, bool mala)
        {
            var g = new DataGridView
            {
                Location = new Point(x, y),
                Size = new Size(w, h),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
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
                ColumnHeadersHeight = mala ? 30 : 36
            };

            Color zaglavlje = Color.FromArgb(221, 227, 238);
            g.ColumnHeadersDefaultCellStyle.BackColor = zaglavlje;
            g.ColumnHeadersDefaultCellStyle.ForeColor = Tamna;
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = zaglavlje;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", mala ? 10f : 11f, FontStyle.Bold);
            g.DefaultCellStyle.Font = new Font("Segoe UI", mala ? 10f : 11f);
            g.DefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
            g.RowTemplate.Height = mala ? 30 : 34;

            if (mala)
            {
                g.ScrollBars = ScrollBars.None;
                g.TabStop = false;
                g.DefaultCellStyle.SelectionBackColor = Color.White;
                g.DefaultCellStyle.SelectionForeColor = Tamna;
            }
            else
            {
                g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(11, 107, 211);
                g.DefaultCellStyle.SelectionForeColor = Color.White;
            }

            return g;
        }

        private static void Datumi(DataGridView g, params string[] kolone)
        {
            foreach (string k in kolone)
                g.Columns[k].DefaultCellStyle.Format = "dd.MM.yyyy.";
        }

        // parovi: ime kolone, zeljeni naslov
        private static void Naslovi(DataGridView g, params string[] parovi)
        {
            for (int i = 0; i + 1 < parovi.Length; i += 2)
                g.Columns[parovi[i]].HeaderText = parovi[i + 1];
        }

        // parovi: ime kolone, tezina (sirina) kolone
        private static void Tezine(DataGridView g, params object[] parovi)
        {
            for (int i = 0; i + 1 < parovi.Length; i += 2)
                g.Columns[(string)parovi[i]].FillWeight = Convert.ToSingle(parovi[i + 1]);
        }

        private static void SakrijId(DataGridView g)
        {
            if (g.Columns.Contains("id")) g.Columns["id"].Visible = false;
        }

        private void Prikazi(int idx)
        {
            for (int i = 0; i < _stranice.Length; i++)
            {
                _stranice[i].Visible = (i == idx);
                _nav[i].Aktivan = (i == idx);
            }
        }

        // ------------------------------------------------------------------
        // BAZA
        // ------------------------------------------------------------------

        private DataTable Upit(string sql, params MySqlParameter[] parametri)
        {
            var dt = new DataTable();
            using (var conn = new MySqlConnection(Db.ConnStr))
            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddRange(parametri);
                new MySqlDataAdapter(cmd).Fill(dt);
            }
            return dt;
        }

        private void OsveziSve()
        {
            if (!_spremno) return;

            try
            {
                UcitajPocetnu();
                UcitajKorisnike();
                UcitajKnjige();
                UcitajPozajmice();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Greška sa bazom: " + ex.Message);
            }
        }

        private void UcitajKategorije()
        {
            try
            {
                _cmbKategorija.DisplayMember = "naziv";
                _cmbKategorija.ValueMember = "id";
                _cmbKategorija.DataSource = Upit("SELECT id, naziv FROM kategorije ORDER BY naziv");
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Greška sa bazom: " + ex.Message);
            }
        }

        private void UcitajPocetnu()
        {
            DataRow s = Upit(@"
                SELECT
                  (SELECT COALESCE(SUM(kolicina), 0) FROM knjige) AS knjige,
                  (SELECT COUNT(*) FROM korisnici WHERE uloga = 'korisnik') AS korisnici,
                  (SELECT COUNT(*) FROM pozajmice WHERE datum_vracanja IS NULL) AS aktivne,
                  (SELECT COUNT(*) FROM pozajmice
                     WHERE datum_vracanja IS NULL AND rok_vracanja < CURDATE()) AS kasnjenja").Rows[0];

            int ukupnoKnjiga = Convert.ToInt32(s["knjige"]);
            int korisnika = Convert.ToInt32(s["korisnici"]);
            int aktivne = Convert.ToInt32(s["aktivne"]);
            int kasnjenja = Convert.ToInt32(s["kasnjenja"]);

            int dostupne = Math.Max(0, ukupnoKnjiga - aktivne);
            int uRoku = Math.Max(0, aktivne - kasnjenja);

            _valKnjiga.Text = ukupnoKnjiga.ToString("N0");
            _valKorisnika.Text = korisnika.ToString("N0");
            _valAktivne.Text = aktivne.ToString("N0");
            _valKasnjenja.Text = kasnjenja.ToString("N0");

            _lblDostupne.Text = dostupne.ToString("N0");
            _lblPozajmljene.Text = uRoku.ToString("N0");
            _lblKasnjenjaLeg.Text = kasnjenja.ToString("N0");
            _lblUkupno.Text = ukupnoKnjiga.ToString("N0");
            _donut.Postavi(dostupne, uRoku, kasnjenja);

            // poslednje pozajmice
            _gridPoslednje.DataSource = Upit(@"
                SELECT CONCAT(u.ime, ' ', u.prezime) AS Korisnik, k.naslov AS Knjiga,
                       p.datum_pozajmice AS Datum, p.rok_vracanja AS Rok
                FROM pozajmice p
                JOIN korisnici u ON u.id = p.korisnik_id
                JOIN knjige k ON k.id = p.knjiga_id
                ORDER BY p.datum_pozajmice DESC, p.id DESC
                LIMIT 5");
            Naslovi(_gridPoslednje, "Datum", "Datum pozajmice", "Rok", "Rok vraćanja");
            Datumi(_gridPoslednje, "Datum", "Rok");
            Tezine(_gridPoslednje, "Korisnik", 28, "Knjiga", 28, "Datum", 22, "Rok", 22);
            _gridPoslednje.ClearSelection();

            // najnoviji korisnici
            _gridKorisniciMali.DataSource = Upit(@"
                SELECT CONCAT(u.ime, ' ', u.prezime) AS Ime, u.email AS Email, COUNT(p.id) AS Pozajmljeno
                FROM korisnici u
                LEFT JOIN pozajmice p ON p.korisnik_id = u.id AND p.datum_vracanja IS NULL
                WHERE u.uloga = 'korisnik'
                GROUP BY u.id, u.ime, u.prezime, u.email, u.datum_registracije
                ORDER BY u.datum_registracije DESC
                LIMIT 5");
            Naslovi(_gridKorisniciMali, "Ime", "Ime i prezime");
            Tezine(_gridKorisniciMali, "Ime", 38, "Email", 42, "Pozajmljeno", 20);
            _gridKorisniciMali.ClearSelection();

            // najtrazenije knjige
            DataTable top = Upit(@"
                SELECT k.naslov AS naslov, COUNT(p.id) AS broj
                FROM knjige k
                JOIN pozajmice p ON p.knjiga_id = k.id
                GROUP BY k.id, k.naslov
                ORDER BY broj DESC, k.naslov
                LIMIT 5");
            var nazivi = new List<string>();
            var vrednosti = new List<int>();
            foreach (DataRow red in top.Rows)
            {
                nazivi.Add(red["naslov"].ToString());
                vrednosti.Add(Convert.ToInt32(red["broj"]));
            }
            _barovi.Postavi(nazivi, vrednosti);
        }

        private void UcitajKorisnike()
        {
            _gridKorisnici.DataSource = Upit(@"
                SELECT u.id, u.ime AS Ime, u.prezime AS Prezime, u.email AS Email, u.uloga AS Uloga,
                       u.datum_registracije AS Registrovan, COUNT(p.id) AS Aktivne
                FROM korisnici u
                LEFT JOIN pozajmice p ON p.korisnik_id = u.id AND p.datum_vracanja IS NULL
                GROUP BY u.id, u.ime, u.prezime, u.email, u.uloga, u.datum_registracije
                ORDER BY u.prezime, u.ime");
            SakrijId(_gridKorisnici);
            Naslovi(_gridKorisnici, "Aktivne", "Aktivne pozajmice");
            Datumi(_gridKorisnici, "Registrovan");
            Tezine(_gridKorisnici, "Ime", 16, "Prezime", 18, "Email", 28, "Uloga", 12, "Registrovan", 14, "Aktivne", 14);
        }

        private void UcitajKnjige()
        {
            _gridKnjige.DataSource = Upit(@"
                SELECT k.id, k.naslov AS Naslov, k.autor AS Autor, k.godina_izdanja AS Godina,
                       c.naziv AS Kategorija, k.kolicina AS NaStanju, COUNT(p.id) AS Pozajmljeno,
                       k.kolicina - COUNT(p.id) AS Dostupno
                FROM knjige k
                JOIN kategorije c ON c.id = k.kategorija_id
                LEFT JOIN pozajmice p ON p.knjiga_id = k.id AND p.datum_vracanja IS NULL
                GROUP BY k.id, k.naslov, k.autor, k.godina_izdanja, c.naziv, k.kolicina
                ORDER BY k.naslov");
            SakrijId(_gridKnjige);
            Naslovi(_gridKnjige, "NaStanju", "Na stanju");
            Tezine(_gridKnjige, "Naslov", 26, "Autor", 22, "Godina", 8, "Kategorija", 14, "NaStanju", 10, "Pozajmljeno", 10, "Dostupno", 10);

            // knjige kojih nema ni jedan slobodan primerak idu u crveno
            foreach (DataGridViewRow red in _gridKnjige.Rows)
            {
                if (Convert.ToInt32(red.Cells["Dostupno"].Value) == 0)
                {
                    red.DefaultCellStyle.BackColor = Color.MistyRose;
                    red.DefaultCellStyle.ForeColor = Color.Firebrick;
                }
            }
        }

        private void UcitajPozajmice()
        {
            _gridPozajmice.DataSource = Upit(@"
                SELECT p.id, CONCAT(u.ime, ' ', u.prezime) AS Korisnik, k.naslov AS Knjiga,
                       p.datum_pozajmice AS Pozajmljeno, p.rok_vracanja AS Rok, p.datum_vracanja AS Vraceno,
                       CASE WHEN p.datum_vracanja IS NOT NULL THEN 'Vraćeno'
                            WHEN p.rok_vracanja < CURDATE() THEN 'Kasni'
                            ELSE 'U roku' END AS Status
                FROM pozajmice p
                JOIN korisnici u ON u.id = p.korisnik_id
                JOIN knjige k ON k.id = p.knjiga_id
                WHERE (@samoAktivne = 0 OR p.datum_vracanja IS NULL)
                ORDER BY (p.datum_vracanja IS NULL) DESC, p.rok_vracanja, p.id",
                new MySqlParameter("@samoAktivne", _chkAktivne.Checked ? 1 : 0));

            SakrijId(_gridPozajmice);
            Naslovi(_gridPozajmice, "Rok", "Rok vraćanja", "Vraceno", "Vraćeno");
            Datumi(_gridPozajmice, "Pozajmljeno", "Rok", "Vraceno");
            Tezine(_gridPozajmice, "Korisnik", 22, "Knjiga", 28, "Pozajmljeno", 13, "Rok", 13, "Vraceno", 13, "Status", 11);

            foreach (DataGridViewRow red in _gridPozajmice.Rows)
            {
                string status = Convert.ToString(red.Cells["Status"].Value);
                if (status == "Kasni")
                {
                    red.DefaultCellStyle.BackColor = Color.MistyRose;
                    red.DefaultCellStyle.ForeColor = Color.Firebrick;
                }
                else if (red.Cells["Vraceno"].Value != DBNull.Value)
                {
                    red.DefaultCellStyle.ForeColor = Siva;
                }
            }
        }

        // ------------------------------------------------------------------
        // AKCIJE
        // ------------------------------------------------------------------

        private void DodajKnjigu(object sender, EventArgs e)
        {
            string naslov = _txtNaslov.Text.Trim();
            string autor = _txtAutor.Text.Trim();

            if (naslov == "" || autor == "" || _cmbKategorija.SelectedValue == null)
            {
                MessageBox.Show("Unesi naslov, autora i izaberi kategoriju.");
                return;
            }

            string sql = @"INSERT INTO knjige (naslov, autor, godina_izdanja, kolicina, kategorija_id)
                           VALUES (@naslov, @autor, @godina, @kolicina, @kat)";

            try
            {
                using (var conn = new MySqlConnection(Db.ConnStr))
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@naslov", naslov);
                    cmd.Parameters.AddWithValue("@autor", autor);
                    cmd.Parameters.AddWithValue("@godina", (int)_numGodina.Value);
                    cmd.Parameters.AddWithValue("@kolicina", (int)_numKolicina.Value);
                    cmd.Parameters.AddWithValue("@kat", Convert.ToInt32(_cmbKategorija.SelectedValue));
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                _txtNaslov.Clear();
                _txtAutor.Clear();
                _numKolicina.Value = 1;
                _txtNaslov.Focus();
                OsveziSve();
                MessageBox.Show("Knjiga je dodata.");
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Greška sa bazom: " + ex.Message);
            }
        }

        private void VratiKnjigu(object sender, EventArgs e)
        {
            if (_gridPozajmice.CurrentRow == null)
            {
                MessageBox.Show("Izaberi pozajmicu iz tabele.");
                return;
            }

            if (_gridPozajmice.CurrentRow.Cells["Vraceno"].Value != DBNull.Value)
            {
                MessageBox.Show("Ova knjiga je već vraćena.");
                return;
            }

            int id = Convert.ToInt32(_gridPozajmice.CurrentRow.Cells["id"].Value);
            string knjiga = Convert.ToString(_gridPozajmice.CurrentRow.Cells["Knjiga"].Value);
            string korisnik = Convert.ToString(_gridPozajmice.CurrentRow.Cells["Korisnik"].Value);

            var odgovor = MessageBox.Show(
                "Označiti knjigu „" + knjiga + "“ (" + korisnik + ") kao vraćenu?",
                "Vraćanje knjige", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (odgovor != DialogResult.Yes) return;

            try
            {
                using (var conn = new MySqlConnection(Db.ConnStr))
                using (var cmd = new MySqlCommand(
                    "UPDATE pozajmice SET datum_vracanja = CURDATE() WHERE id = @id AND datum_vracanja IS NULL", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                OsveziSve();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Greška sa bazom: " + ex.Message);
            }
        }

        private void Odjava(object sender, EventArgs e)
        {
            Sesija.Id = 0;
            Sesija.Ime = null;
            Sesija.Uloga = null;
            this.Close();
        }
    }

    // ----------------------------------------------------------------------
    // POMOCNE KONTROLE (ostaju u ovom fajlu, ispod forme)
    // ----------------------------------------------------------------------

    public class NavButton : Button
    {
        public string Glyph { get; set; } = "";

        private bool _aktivan;
        public bool Aktivan
        {
            get { return _aktivan; }
            set { _aktivan = value; Invalidate(); }
        }

        private bool _hover;

        public NavButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Cursor = Cursors.Hand;
            TabStop = false;
            DoubleBuffered = true;
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent != null ? Parent.BackColor : SystemColors.Control);

            var r = new Rectangle(0, 0, Width - 1, Height - 1);
            if (_aktivan || _hover)
            {
                using (var path = Zaobljeno.Putanja(r, 8))
                using (var br = new SolidBrush(_aktivan ? Color.FromArgb(229, 185, 107) : Color.FromArgb(35, 60, 105)))
                    g.FillPath(br, path);
            }

            Color boja = _aktivan ? Color.FromArgb(23, 43, 77) : Color.White;

            using (var fi = new Font("Segoe MDL2 Assets", 14f))
                TextRenderer.DrawText(g, Glyph, fi, new Rectangle(12, 0, 32, Height), boja,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);

            using (var ft = new Font("Segoe UI", 11.5f, _aktivan ? FontStyle.Bold : FontStyle.Regular))
                TextRenderer.DrawText(g, Text, ft, new Rectangle(52, 0, Width - 56, Height), boja,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPadding);
        }
    }

    public class DonutPanel : Panel
    {
        private int _dostupne, _pozajmljene, _kasnjenja;

        private static readonly Color BojaDostupne = Color.FromArgb(23, 43, 77);
        private static readonly Color BojaPozajmljene = Color.FromArgb(229, 185, 107);
        private static readonly Color BojaKasnjenja = Color.FromArgb(214, 40, 40);
        private static readonly Color Tamna = Color.FromArgb(28, 39, 51);

        public DonutPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
        }

        public void Postavi(int dostupne, int pozajmljene, int kasnjenja)
        {
            _dostupne = dostupne;
            _pozajmljene = pozajmljene;
            _kasnjenja = kasnjenja;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int debljina = 30;
            int velicina = Math.Min(Width, Height) - debljina - 4;
            var r = new Rectangle((Width - velicina) / 2, (Height - velicina) / 2, velicina, velicina);
            int ukupno = _dostupne + _pozajmljene + _kasnjenja;

            using (var pozadina = new Pen(Color.FromArgb(225, 222, 214), debljina))
                g.DrawEllipse(pozadina, r);

            if (ukupno > 0)
            {
                float start = -90f;
                Segment(g, r, debljina, BojaDostupne, ref start, _dostupne, ukupno);
                Segment(g, r, debljina, BojaPozajmljene, ref start, _pozajmljene, ukupno);
                Segment(g, r, debljina, BojaKasnjenja, ref start, _kasnjenja, ukupno);
            }

            int procenat = ukupno > 0 ? (int)Math.Round(100.0 * _dostupne / ukupno) : 0;
            var flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding;
            using (var f1 = new Font("Segoe UI", 20f, FontStyle.Bold))
                TextRenderer.DrawText(g, procenat + "%", f1, new Rectangle(0, Height / 2 - 30, Width, 32), Tamna, flags);
            using (var f2 = new Font("Segoe UI", 11f))
                TextRenderer.DrawText(g, "Dostupno", f2, new Rectangle(0, Height / 2 + 2, Width, 22), Tamna, flags);
        }

        private static void Segment(Graphics g, Rectangle r, int debljina, Color boja, ref float start, int vrednost, int ukupno)
        {
            if (vrednost <= 0) return;
            float ugao = 360f * vrednost / ukupno;
            using (var pen = new Pen(boja, debljina))
                g.DrawArc(pen, r, start, ugao);
            start += ugao;
        }
    }

    public class BarListPanel : Panel
    {
        private readonly List<string> _nazivi = new List<string>();
        private readonly List<int> _vrednosti = new List<int>();

        public BarListPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
        }

        public void Postavi(List<string> nazivi, List<int> vrednosti)
        {
            _nazivi.Clear();
            _vrednosti.Clear();
            _nazivi.AddRange(nazivi);
            _vrednosti.AddRange(vrednosti);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            Color tamna = Color.FromArgb(28, 39, 51);
            Color traka = Color.FromArgb(225, 222, 214);
            Color navy = Color.FromArgb(23, 43, 77);

            using (var br = new SolidBrush(Color.FromArgb(221, 227, 238)))
                g.FillRectangle(br, 0, 0, Width, 28);

            var fl = TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding;
            using (var fb = new Font("Segoe UI", 10f, FontStyle.Bold))
            {
                TextRenderer.DrawText(g, "Naslov", fb, new Rectangle(8, 0, 150, 28), tamna, fl | TextFormatFlags.Left);
                TextRenderer.DrawText(g, "Broj pozajmica", fb, new Rectangle(Width - 128, 0, 120, 28), tamna, fl | TextFormatFlags.Right);
            }

            int max = 1;
            foreach (int v in _vrednosti) if (v > max) max = v;

            int naslovSirina = (int)(Width * 0.34);
            int barX = naslovSirina + 14;
            int barSirina = Math.Max(10, Width - barX - 140);

            using (var f = new Font("Segoe UI", 10f))
            using (var linija = new Pen(traka))
            {
                for (int i = 0; i < _nazivi.Count; i++)
                {
                    int y = 28 + i * 32;

                    TextRenderer.DrawText(g, _nazivi[i], f, new Rectangle(8, y, naslovSirina, 32), tamna,
                        fl | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);

                    using (var b = new SolidBrush(traka))
                        g.FillRectangle(b, barX, y + 11, barSirina, 10);
                    int popunjeno = (int)(barSirina * (double)_vrednosti[i] / max);
                    using (var b = new SolidBrush(navy))
                        g.FillRectangle(b, barX, y + 11, popunjeno, 10);

                    TextRenderer.DrawText(g, _vrednosti[i].ToString(), f, new Rectangle(Width - 128, y, 120, 32), tamna,
                        fl | TextFormatFlags.Right);

                    g.DrawLine(linija, 0, y + 31, Width, y + 31);
                }
            }
        }
    }
}
