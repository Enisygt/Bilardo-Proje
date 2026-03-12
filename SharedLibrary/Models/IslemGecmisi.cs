using System;

namespace SharedLibrary.Models;

public class IslemGecmisi
{
    public int Id { get; set; }
    public string MasaNo { get; set; } = string.Empty;
    public DateTime Baslangic { get; set; }
    public DateTime Bitis { get; set; }
    public decimal UcretSüre { get; set; }
    public decimal UcretSiparis { get; set; }
    public decimal Indirim { get; set; }
    public decimal ToplamTutar { get; set; }
    public bool IsManual { get; set; }
}
