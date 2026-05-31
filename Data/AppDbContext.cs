using GorevTakip.Models;
using Microsoft.EntityFrameworkCore;

namespace GorevTakip.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Kullanici> Kullanicilar => Set<Kullanici>();
    public DbSet<Gorev> Gorevler => Set<Gorev>();
    public DbSet<Kategori> Kategoriler => Set<Kategori>();
    public DbSet<AltGorev> AltGorevler => Set<AltGorev>();
    public DbSet<SiteAyar> SiteAyarlari => Set<SiteAyar>();
    public DbSet<Proje> Projeler => Set<Proje>();
    public DbSet<ProjeUye> ProjeUyeleri => Set<ProjeUye>();
    public DbSet<ProjeDavet> ProjeDavetleri => Set<ProjeDavet>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Kullanici>()
            .HasIndex(k => k.Email)
            .IsUnique();

        modelBuilder.Entity<Gorev>()
            .HasOne(g => g.Kullanici)
            .WithMany(k => k.Gorevler)
            .HasForeignKey(g => g.KullaniciId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Kategori>()
            .HasOne(k => k.Kullanici)
            .WithMany(u => u.Kategoriler)
            .HasForeignKey(k => k.KullaniciId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Gorev>()
            .HasOne(g => g.Kategori)
            .WithMany(k => k.Gorevler)
            .HasForeignKey(g => g.KategoriId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<AltGorev>()
            .HasOne(a => a.Gorev)
            .WithMany(g => g.AltGorevler)
            .HasForeignKey(a => a.GorevId)
            .OnDelete(DeleteBehavior.Cascade);

        // --- Proje / Takım ilişkileri ---

        // Proje -> Lider (kullanıcı). Lider silinirse projeleri de silinir.
        modelBuilder.Entity<Proje>()
            .HasOne(p => p.Lider)
            .WithMany(k => k.LiderOlduguProjeler)
            .HasForeignKey(p => p.LiderId)
            .OnDelete(DeleteBehavior.Cascade);

        // ProjeUye (çok-çok bağlantı)
        modelBuilder.Entity<ProjeUye>()
            .HasOne(u => u.Proje)
            .WithMany(p => p.Uyeler)
            .HasForeignKey(u => u.ProjeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProjeUye>()
            .HasOne(u => u.Kullanici)
            .WithMany(k => k.ProjeUyelikleri)
            .HasForeignKey(u => u.KullaniciId)
            .OnDelete(DeleteBehavior.Cascade);

        // Aynı kullanıcı bir projeye iki kez üye olamaz
        modelBuilder.Entity<ProjeUye>()
            .HasIndex(u => new { u.ProjeId, u.KullaniciId })
            .IsUnique();

        // ProjeDavet
        modelBuilder.Entity<ProjeDavet>()
            .HasOne(d => d.Proje)
            .WithMany(p => p.Davetler)
            .HasForeignKey(d => d.ProjeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProjeDavet>()
            .HasOne(d => d.Kullanici)
            .WithMany()
            .HasForeignKey(d => d.KullaniciId)
            .OnDelete(DeleteBehavior.Cascade);

        // Görevi atayan lider silinirse görev kalır (AtayanId null olur)
        modelBuilder.Entity<Gorev>()
            .HasOne(g => g.Atayan)
            .WithMany()
            .HasForeignKey(g => g.AtayanId)
            .OnDelete(DeleteBehavior.SetNull);

        // Proje silinirse görevler kalır (ProjeId null olur)
        modelBuilder.Entity<Gorev>()
            .HasOne(g => g.Proje)
            .WithMany(p => p.Gorevler)
            .HasForeignKey(g => g.ProjeId)
            .OnDelete(DeleteBehavior.SetNull);

        base.OnModelCreating(modelBuilder);
    }
}
