using System;
using CommunityToolkit.Mvvm.ComponentModel;
using NeoHal.Core.Entities;

namespace NeoHal.Desktop.ViewModels;

/// <summary>
/// Giriş İrsaliyesi için kalem satırı - DataGrid'de düzenleme için
/// </summary>
public partial class GirisKalem : ObservableObject
{
    [ObservableProperty]
    private Guid _id = Guid.NewGuid();
    
    /// <summary>
    /// Hal alışında: Bu satırdaki malı aldığımız komisyoncu
    /// </summary>
    [ObservableProperty]
    private CariHesap? _komisyoncu;
    
    [ObservableProperty]
    private Urun? _urun;
    
    [ObservableProperty]
    private KapTipi? _kapTipi;
    
    [ObservableProperty]
    private int _kapAdet = 1;
    
    [ObservableProperty]
    private decimal _daraliKg;
    
    [ObservableProperty]
    private decimal _birimFiyat;
    
    // Readonly - Display
    public string KomisyoncuAdi => Komisyoncu?.Unvan ?? "";
    public string UrunAdi => Urun?.Ad ?? "";
    public string KapTipiAdi => KapTipi?.Ad ?? "";
    
    // Hesaplanan alanlar
    public decimal DaraKg => KapAdet * (KapTipi?.DaraAgirlik ?? 0);
    public decimal NetKg => DaraliKg - DaraKg;
    public decimal Tutar => NetKg * BirimFiyat;
    
    // Kalan (Hal Kayıt için)
    public decimal KalanKg => NetKg;
    public int KalanKapAdet => KapAdet;
    
    partial void OnKomisyoncuChanged(CariHesap? value)
    {
        OnPropertyChanged(nameof(KomisyoncuAdi));
    }
    
    partial void OnUrunChanged(Urun? value)
    {
        OnPropertyChanged(nameof(UrunAdi));
    }
    
    partial void OnKapTipiChanged(KapTipi? value)
    {
        OnPropertyChanged(nameof(KapTipiAdi));
        OnPropertyChanged(nameof(DaraKg));
        OnPropertyChanged(nameof(NetKg));
        OnPropertyChanged(nameof(Tutar));
    }
    
    partial void OnKapAdetChanged(int value)
    {
        OnPropertyChanged(nameof(DaraKg));
        OnPropertyChanged(nameof(NetKg));
        OnPropertyChanged(nameof(Tutar));
        OnPropertyChanged(nameof(KalanKapAdet));
    }
    
    partial void OnDaraliKgChanged(decimal value)
    {
        OnPropertyChanged(nameof(NetKg));
        OnPropertyChanged(nameof(Tutar));
        OnPropertyChanged(nameof(KalanKg));
    }
    
    partial void OnBirimFiyatChanged(decimal value)
    {
        OnPropertyChanged(nameof(Tutar));
    }
    
    /// <summary>
    /// Entity'ye dönüştür
    /// </summary>
    public GirisIrsaliyesiKalem ToEntity()
    {
        return new GirisIrsaliyesiKalem
        {
            Id = Id,
            KomisyoncuId = Komisyoncu?.Id,
            UrunId = Urun?.Id ?? Guid.Empty,
            Urun = Urun!,
            KapTipiId = KapTipi?.Id ?? Guid.Empty,
            KapTipi = KapTipi!,
            KapAdet = KapAdet,
            BrutKg = DaraliKg, // Daralı = Brüt (kasayla)
            DaraKg = DaraKg,
            NetKg = NetKg,
            BirimFiyat = BirimFiyat,
            KalanKapAdet = KapAdet,
            KalanKg = NetKg
        };
    }
    
    /// <summary>
    /// Entity'den oluştur
    /// </summary>
    public static GirisKalem FromEntity(GirisIrsaliyesiKalem entity)
    {
        return new GirisKalem
        {
            Id = entity.Id,
            Komisyoncu = entity.Komisyoncu,
            Urun = entity.Urun,
            KapTipi = entity.KapTipi,
            KapAdet = entity.KapAdet,
            DaraliKg = entity.BrutKg, // Brüt = Daralı (kasayla)
            BirimFiyat = entity.BirimFiyat ?? 0
        };
    }
    
    /// <summary>
    /// Kopyasını oluştur
    /// </summary>
    public GirisKalem Clone()
    {
        return new GirisKalem
        {
            Id = Guid.NewGuid(),
            Komisyoncu = Komisyoncu,
            Urun = Urun,
            KapTipi = KapTipi,
            KapAdet = KapAdet,
            DaraliKg = DaraliKg,
            BirimFiyat = BirimFiyat
        };
    }
}
