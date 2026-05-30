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

        base.OnModelCreating(modelBuilder);
    }
}
