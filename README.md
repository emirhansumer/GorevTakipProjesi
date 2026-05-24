# Görev Takip Sistemi

Kullanıcıların günlük görevlerini düzenleyebileceği, **kullanıcıya özel veri yönetimi** sağlayan modern bir ASP.NET Core MVC uygulaması.

> Üniversite ödevi olarak başlandı, sonrasında özellikler eklenerek geliştirilmektedir.

## ✨ Özellikler

### Temel (Hocanın zorunlu 10 ekranı)
- 🔐 Kayıt / Giriş / Çıkış (Session-based auth, BCrypt şifre hash'leme)
- 📊 Dashboard (toplam / bekleyen / tamamlanan / iptal sayaçları + tamamlanma yüzdesi)
- 📝 Görev CRUD (Ekle / Listele / Güncelle / Sil / Detay)
- 🔍 Filtreli listeler (Bekleyen / Tamamlanan / Kategoriye göre)
- 👤 Profil ekranı (kullanıcı bilgileri + session bilgisi)

### Ekstra geliştirmeler
- 🚩 **Görev önceliği** — Düşük / Orta / Yüksek / Acil (renkli + ikonlu)
- 🏷️ **Kategoriler** — Kullanıcı kendi renkli/ikonlu kategorilerini oluşturup yönetebilir
  - 26 hazır ikon + 1 özel SVG (futbol topu) — Türkçe tooltip'li
  - HTML5 color picker + 10 hazır renk paleti
  - **Drag-drop sıralama** (SortableJS)
  - Kategori başına istatistik (X / Y tamamlandı + ilerleme çubuğu)
- 🎯 **Inline kategori değiştirme** — Görev listesinde chip'e tıklayıp sayfa yenilenmeden kategori değiştir
- 🏷️ **Form'dan hızlı kategori ekleme** — Görev oluştururken "+ Yeni Kategori" butonuyla modal
- 🍬 **SweetAlert2** — Modern modal ve toast bildirimleri
- 🎨 **Otomatik renk kontrastı** — Açık/koyu arkaplan için text otomatik koyu/beyaz (luminance hesabı)
- 📅 Bitiş tarihi takibi, oluşturma tarihi
- 🛡️ Güvenlik: AntiForgery, SameSite=Strict cookies, DataProtection kalıcı keyring, yetkisiz erişim engeli
- 🎨 Canlı Productivity teması (Bootstrap 5 + Bootstrap Icons + özel CSS)
- 🚫 Şık 404 / hata sayfaları

## 🛠️ Teknoloji

| Katman | Teknoloji |
|--------|-----------|
| Backend | ASP.NET Core 10 MVC |
| ORM | Entity Framework Core 10 (Code First) |
| Veritabanı | SQLite |
| Auth | Session-based + BCrypt.Net-Next |
| Frontend | Bootstrap 5 + Bootstrap Icons + SweetAlert2 + SortableJS |
| Dil | Türkçe (UI) + C# 12 |

## 📁 Proje yapısı

```
GorevTakipProjesi/
├── Controllers/              # MVC controller'lar (Home, Account, Gorev, Kategori)
├── Data/AppDbContext.cs      # EF Core context, ilişkiler
├── Filters/AuthCheckAttribute.cs  # [AuthCheck] action filter
├── Helpers/                  # Session, Renk, Durum, Oncelik, SvgIkonlar helper'ları
├── Migrations/               # EF Core migration'ları
├── Models/                   # Kullanici, Gorev, Kategori, GorevDurum, Oncelik
├── ViewModels/               # Form ve dashboard view modelleri
├── Views/
│   ├── Account/              # Giriş, Kayıt, Profil
│   ├── Gorev/                # Liste, Detay, Ekle, Güncelle
│   ├── Kategori/             # Kategori CRUD
│   ├── Home/                 # Dashboard
│   └── Shared/_Layout.cshtml # Ana layout
├── wwwroot/
│   ├── css/site.css          # Tüm özel temalar (Canlı Productivity)
│   └── js/site.js            # Toast, modal, AJAX helper'ları
├── Program.cs                # Servis kayıtları + middleware
├── appsettings.json          # ConnectionString
└── baslat.bat                # Çift tıklayarak çalıştırma
```

## 🚀 Kurulum (yeni bir bilgisayarda)

### Gereksinimler

1. **[.NET 10 SDK](https://dotnet.microsoft.com/download)** kurulu olmalı
2. **[Git](https://git-scm.com/download/win)** kurulu olmalı
3. (Opsiyonel) **[VS Code](https://code.visualstudio.com)** + C# Dev Kit eklentisi

### Adımlar

```powershell
# 1. Projeyi klonla
git clone https://github.com/emirhansumer/GorevTakipProjesi.git
cd GorevTakipProjesi

# 2. EF Core CLI aracı (ilk kez kuruyorsan)
dotnet tool install --global dotnet-ef

# 3. Veritabanını oluştur
dotnet ef database update

# 4. Uygulamayı çalıştır
dotnet run --launch-profile http
```

Tarayıcıda **http://localhost:5009** adresini aç.

### Daha kolayı

`baslat.bat` dosyasına **çift tıkla** → otomatik olarak app başlar ve tarayıcı açılır.

## 🧪 İlk kullanım

1. Site açılınca **"Kayıt Ol"** tıkla
2. Yeni hesap aç
3. Giriş yap
4. Önce **Kategorilerim** sayfasına gidip birkaç kategori oluştur (örn: Okul, İş, Spor)
5. Sonra **+ Yeni Görev** butonuyla görev eklemeye başla

## 📋 Hocanın değerlendirme kriterleri

| Kriter | Puan | Durum |
|--------|------|-------|
| Kod Kalitesi | 20 | ✅ Katmanlı mimari (Controllers / Models / ViewModels / Helpers / Filters / Data) |
| CRUD İşlemleri | 20 | ✅ Tam (Görev + Kategori) |
| Veri Tabanı Tasarımı | 15 | ✅ Code First + 3 tablo + FK ilişkiler + unique index |
| Authentication & Session | 15 | ✅ BCrypt + Session + AntiForgery + SameSite=Strict |
| Arayüz (UI/UX) | 10 | ✅ Responsive Bootstrap 5 + custom tema |
| Proje Fonksiyonelliği | 10 | ✅ 10 ekran + ekstra özellikler |
| Sunum | 10 | Demo'da gösterilecek |

## 📝 Notlar

- `gorevtakip.db` ve `DataProtection-Keys/` `.gitignore`'a alındı, repo'da değil
- Her ortam kendi keylerini ve veritabanını üretir (`dotnet ef database update`)
- HTTPS dev ortamında zorlanmaz (Plesk shared hosting'lerde çakışmaması için)

## 👤 Geliştirici

**Emirhan Sümer** — [GitHub](https://github.com/emirhansumer)

---

📚 [Detaylı PR geçmişi için Pull Requests sekmesine bakın](https://github.com/emirhansumer/GorevTakipProjesi/pulls?q=is%3Apr)
