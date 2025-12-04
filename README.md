# NeoHal - Hal Otomasyon Sistemi

Modern, hibrit bir Sebze/Meyve Hali otomasyon sistemi.

## Proje Yapisi

```
NeoHal/
├── src/
│   ├── NeoHal.Core/           # Entity'ler, Enum'lar, Interface'ler
│   ├── NeoHal.Data/           # Veritabani katmani (EF Core + SQLite)
│   ├── NeoHal.Services/       # Is mantigi katmani
│   └── NeoHal.Desktop/        # Avalonia UI Desktop uygulamasi
└── NeoHal.sln
```

## Veritabani Modulleri

1. **Cari Hesaplar** - Mustahsil, Komisyoncu, Alici, Nakliyeci
2. **Urun/Stok** - Urunler, Kap Tipleri, Dara Agirliklari
3. **Operasyon** - Giris Irsaliyesi, Satis Faturasi
4. **Kasa Takip** - Dolu/Bos Kasa Dongusu, Rehin Fisleri
5. **Finans** - Cari Hareketler, Tahsilat, Odeme

## Kasa Dongusu

```
MUSTAHSIL --[DOLU KASA]--> KOMISYONCU --[DOLU KASA]--> ALICI
    ^                           |                        |
    |                           |                        |
    +------[BOS KASA]<----------+------[BOS KASA]<-------+
```

## Calistirma

```bash
cd NeoHal
dotnet build
cd src/NeoHal.Desktop
dotnet run
```

## Teknolojiler

- .NET 9 + Avalonia UI (Cross-platform Desktop)
- SQLite + Entity Framework Core
- MVVM + CommunityToolkit.Mvvm
- Repository Pattern + Dependency Injection
