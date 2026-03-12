using Microsoft.EntityFrameworkCore;
using SharedLibrary.Models;

namespace ServerApplication.Data;

public class AppDbContext : DbContext
{
    public DbSet<Masa> Masalar { get; set; }
    public DbSet<Siparis> Siparisler { get; set; }
    public DbSet<IslemGecmisi> IslemGecmisleri { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=TicariOtomasyonDb;Trusted_Connection=True;");
        }
    }
}
