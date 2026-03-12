namespace SharedLibrary.Models;
using SharedLibrary.Enums;

public class Masa
{
    public int Id { get; set; }
    public string MasaNo { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public MasaDurum Durum { get; set; }
    public decimal SaatlikUcret { get; set; }
}
