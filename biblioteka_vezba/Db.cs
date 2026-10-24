public static class Db
{
    // XAMPP podrazumevano: korisnik root, bez lozinke
    public const string ConnStr =
        "Server=localhost;Port=3306;Database=biblioteka;Uid=root;Pwd=;CharSet=utf8mb4;";
}

// Pamti ko je ulogovan dok se prelazi između formi
public static class Sesija
{
    public static int Id;
    public static string Ime;
    public static string Uloga;
}
