namespace SharedLibrary.Models;

public class Urun
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public decimal Fiyat { get; set; }
    public string Kategori { get; set; } = "İçecek";
    public bool IsAktif { get; set; } = true;
}
