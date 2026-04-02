namespace SharedLibrary.Models;
using SharedLibrary.Enums;

public class Masa
{
    public int Id { get; set; }
    public string MasaNo { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public MasaDurum Durum { get; set; }
    public decimal SaatlikUcret { get; set; }
    public DateTime? BaslangicZamani { get; set; }
    public string PlayerAName { get; set; } = string.Empty;
    public string PlayerBName { get; set; } = string.Empty;
    public string MasaNotu { get; set; } = string.Empty;
    public decimal ToplamBorc { get; set; }
    public string BekleyenSiparis { get; set; } = string.Empty;
}
