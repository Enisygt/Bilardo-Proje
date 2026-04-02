using Microsoft.EntityFrameworkCore;
using SharedLibrary.Models;
using System.IO;

namespace ServerApplication.Data;

public class AppDbContext : DbContext
{
    public DbSet<Masa> Masalar { get; set; }
    public DbSet<Siparis> Siparisler { get; set; }
    public DbSet<IslemGecmisi> IslemGecmisleri { get; set; }
    public DbSet<Kampanya> Kampanyalar { get; set; }
    public DbSet<Urun> Urunler { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dbFolder = Path.Combine(appData, "BilardoApp");
            Directory.CreateDirectory(dbFolder);
            var dbPath = Path.Combine(dbFolder, "bilardo.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed varsayılan masalar
        for (int i = 1; i <= 9; i++)
        {
            modelBuilder.Entity<Masa>().HasData(new Masa
            {
                Id = i,
                MasaNo = $"Masa {i}",
                IpAddress = $"192.168.1.{100 + i}",
                Durum = SharedLibrary.Enums.MasaDurum.Bos,
                SaatlikUcret = 150.0m
            });
        }

        // Seed varsayılan ürünler
        modelBuilder.Entity<Urun>().HasData(
            new Urun { Id = 1, Ad = "Çay", Fiyat = 15, Kategori = "Sıcak İçecekler", IsAktif = true },
            new Urun { Id = 2, Ad = "Türk Kahvesi", Fiyat = 35, Kategori = "Sıcak İçecekler", IsAktif = true },
            new Urun { Id = 3, Ad = "Nescafe", Fiyat = 40, Kategori = "Sıcak İçecekler", IsAktif = true },
            new Urun { Id = 4, Ad = "Kola", Fiyat = 40, Kategori = "Soğuk İçecekler", IsAktif = true },
            new Urun { Id = 5, Ad = "Su", Fiyat = 10, Kategori = "Soğuk İçecekler", IsAktif = true },
            new Urun { Id = 6, Ad = "Meyve Suyu", Fiyat = 30, Kategori = "Soğuk İçecekler", IsAktif = true },
            new Urun { Id = 7, Ad = "Ayran", Fiyat = 20, Kategori = "Soğuk İçecekler", IsAktif = true },
            new Urun { Id = 8, Ad = "Sandviç", Fiyat = 60, Kategori = "Atıştırmalık", IsAktif = true },
            new Urun { Id = 9, Ad = "Tost", Fiyat = 50, Kategori = "Atıştırmalık", IsAktif = true },
            new Urun { Id = 10, Ad = "Çips", Fiyat = 25, Kategori = "Atıştırmalık", IsAktif = true }
        );

        // Seed varsayılan kampanyalar
        modelBuilder.Entity<Kampanya>().HasData(
            new Kampanya { Id = 1, Baslik = "🎯 2 Saatlik Oyun", Aciklama = "2 saat oyun + 4 çay dahil", Fiyat = 250, IsAktif = true },
            new Kampanya { Id = 2, Baslik = "☕ Cafe Paketi", Aciklama = "3 Çay + 1 Kahve", Fiyat = 70, IsAktif = true },
            new Kampanya { Id = 3, Baslik = "🏆 Hafta Sonu", Aciklama = "3 saat oyun + sınırsız su", Fiyat = 350, IsAktif = true }
        );
    }
}
