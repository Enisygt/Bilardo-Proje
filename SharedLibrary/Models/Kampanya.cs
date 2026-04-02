namespace SharedLibrary.Models;

public class Kampanya
{
    public int Id { get; set; }
    public string Baslik { get; set; } = string.Empty;
    public string Aciklama { get; set; } = string.Empty;
    public decimal Fiyat { get; set; }
    public bool IsAktif { get; set; } = true;
}
