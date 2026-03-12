namespace SharedLibrary.Models;
using SharedLibrary.Enums;

public class Siparis
{
    public int Id { get; set; }
    public int MasaId { get; set; }
    public int UrunId { get; set; }
    public int Adet { get; set; }
    public DateTime SiparisZamani { get; set; }
    public SiparisDurum Durum { get; set; }
}
