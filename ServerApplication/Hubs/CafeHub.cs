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
        var httpContext = Context.GetHttpContext();
        var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString();

        if (!string.IsNullOrEmpty(ipAddress))
        {
            var masa = await _context.Masalar.FirstOrDefaultAsync(m => m.IpAddress == ipAddress);
            if (masa != null)
            {
                // We'll broadcast logic to clients based on connection
                await Clients.Caller.SendAsync("TerminalVerified", masa.MasaNo);
            }
        }

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
        }
    }
}
