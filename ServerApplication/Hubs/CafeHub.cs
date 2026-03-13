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
        // For simplicity in the new single app architecture, we'll verify all incoming terminals
        // The terminal will just tell us which table they are (or they will be assigned one)
        // For now, let's just bypass the strict IP validation and tell them they are verified.
        await Clients.Caller.SendAsync("TerminalVerified", "Yeni Masa");

        await base.OnConnectedAsync();
    }

    public async Task MasaAc(int masaId)
    {
        var masa = await _context.Masalar.FindAsync(masaId);
        if (masa != null)
        {
            masa.Durum = MasaDurum.Aktif;
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
            await _context.SaveChangesAsync();
            await Clients.All.SendAsync("MasaDurumuDegisti", masa.Id, (int)masa.Durum);
            await Clients.All.SendAsync("ResetMatch", masa.Id);
        }
    }
}
