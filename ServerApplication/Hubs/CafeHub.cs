using Microsoft.AspNetCore.SignalR;
using ServerApplication.Data;
using SharedLibrary.Enums;
using Microsoft.EntityFrameworkCore;

namespace ServerApplication.Hubs;

public class CafeHub : Hub
{
    private readonly AppDbContext _context;

    public CafeHub(AppDbContext context)
    {
        _context = context;
    }

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("TerminalVerified", "Yeni Masa");
        await base.OnConnectedAsync();
    }

    public async Task MasaAc(int masaId)
    {
        var masa = await _context.Masalar.FindAsync(masaId);
        if (masa != null)
        {
            masa.Durum = MasaDurum.Aktif;
            masa.BaslangicZamani = DateTime.Now;
            await _context.SaveChangesAsync();
            await Clients.All.SendAsync("MasaDurumuDegisti", masa.Id, (int)masa.Durum);
        }
    }

    public async Task MasaKapat(int masaId)
    {
        var masa = await _context.Masalar.FindAsync(masaId);
        if (masa != null)
        {
            masa.Durum = MasaDurum.Bos;
            masa.BaslangicZamani = null;
            masa.ToplamBorc = 0;
            masa.BekleyenSiparis = string.Empty;
            masa.PlayerAName = string.Empty;
            masa.PlayerBName = string.Empty;
            await _context.SaveChangesAsync();
            await Clients.All.SendAsync("MasaDurumuDegisti", masa.Id, (int)masa.Durum);
            await Clients.All.SendAsync("ResetMatch", masa.Id);
        }
    }

    public async Task SiparisGonder(int masaId, string siparisOzeti)
    {
        var masa = await _context.Masalar.FindAsync(masaId);
        if (masa != null)
        {
            masa.Durum = MasaDurum.SiparisBekliyor;
            masa.BekleyenSiparis = siparisOzeti;
            await _context.SaveChangesAsync();
        }
        // Kasa bildir
        await Clients.All.SendAsync("SiparisGeldi", masaId, siparisOzeti);
    }

    public async Task SiparisOnayla(int masaId)
    {
        var masa = await _context.Masalar.FindAsync(masaId);
        if (masa != null)
        {
            masa.Durum = MasaDurum.Aktif;
            masa.BekleyenSiparis = string.Empty;
            await _context.SaveChangesAsync();
        }
        // Client'e sipariş onayı gönder
        await Clients.All.SendAsync("SiparisOnaylandi", masaId);
        await Clients.All.SendAsync("MasaDurumuDegisti", masaId, (int)MasaDurum.Aktif);
    }

    public async Task MasaBilgisiGuncelle(int masaId, decimal tutar, string sure)
    {
        var masa = await _context.Masalar.FindAsync(masaId);
        if (masa != null)
        {
            masa.ToplamBorc = tutar;
            await _context.SaveChangesAsync();
        }
        await Clients.All.SendAsync("MasaTutarGuncellendi", masaId, tutar, sure);
    }
}
