using System;
using CommunityToolkit.Mvvm.ComponentModel;
using NeoHal.Core.Entities;

namespace NeoHal.Desktop.ViewModels;

/// <summary>
/// SatisFaturasiKalem için Observable Wrapper
/// Tutar hesaplaması otomatik yapılır
/// </summary>
public partial class SatisFaturasiKalemVM : ObservableObject
{
    private readonly SatisFaturasiKalem _kalem;
    private readonly Action? _onChanged;

    public SatisFaturasiKalemVM(SatisFaturasiKalem kalem, Action? onChanged = null)
    {
        _kalem = kalem;
        _onChanged = onChanged;
    }

    public Guid Id => _kalem.Id;
    public Guid? GirisKalemId => _kalem.GirisKalemId;

    [ObservableProperty]
    private Urun? _urun;
    
    partial void OnUrunChanged(Urun? value)
    {
        if (value != null)
        {
            _kalem.UrunId = value.Id;
            _kalem.Urun = value;
            _kalem.RusumOrani = value.RusumOrani;
            _kalem.StopajOrani = value.StopajOrani;
        }
        RecalculateTotals();
    }

    [ObservableProperty]
    private KapTipi? _kapTipi;
    
    partial void OnKapTipiChanged(KapTipi? value)
    {
        if (value != null)
        {
            _kalem.KapTipiId = value.Id;
            _kalem.KapTipi = value;
            // Dara kg güncelle
            _kalem.DaraKg = KapAdet * value.DaraAgirlik;
            OnPropertyChanged(nameof(DaraKg));
            RecalculateNetKg();
        }
    }

    public int KapAdet
    {
        get => _kalem.KapAdet;
        set
        {
            if (_kalem.KapAdet != value)
            {
                _kalem.KapAdet = value;
                OnPropertyChanged();
                // Dara güncelle
                if (KapTipi != null)
                {
                    _kalem.DaraKg = value * KapTipi.DaraAgirlik;
                    OnPropertyChanged(nameof(DaraKg));
                }
                RecalculateNetKg();
            }
        }
    }

    public decimal BrutKg
    {
        get => _kalem.BrutKg;
        set
        {
            if (_kalem.BrutKg != value)
            {
                _kalem.BrutKg = value;
                OnPropertyChanged();
                RecalculateNetKg();
            }
        }
    }

    public decimal DaraKg => _kalem.DaraKg;

    public decimal NetKg
    {
        get => _kalem.NetKg;
        set
        {
            if (_kalem.NetKg != value)
            {
                _kalem.NetKg = value;
                OnPropertyChanged();
                RecalculateTotals();
            }
        }
    }

    public decimal BirimFiyat
    {
        get => _kalem.BirimFiyat;
        set
        {
            if (_kalem.BirimFiyat != value)
            {
                _kalem.BirimFiyat = value;
                OnPropertyChanged();
                RecalculateTotals();
            }
        }
    }

    public decimal Tutar
    {
        get => _kalem.Tutar;
        private set
        {
            if (_kalem.Tutar != value)
            {
                _kalem.Tutar = value;
                OnPropertyChanged();
            }
        }
    }

    public decimal RusumOrani
    {
        get => _kalem.RusumOrani;
        set
        {
            if (_kalem.RusumOrani != value)
            {
                _kalem.RusumOrani = value;
                OnPropertyChanged();
                RecalculateTotals();
            }
        }
    }

    public decimal RusumTutari
    {
        get => _kalem.RusumTutari;
        private set
        {
            if (_kalem.RusumTutari != value)
            {
                _kalem.RusumTutari = value;
                OnPropertyChanged();
            }
        }
    }

    public decimal KomisyonOrani
    {
        get => _kalem.KomisyonOrani;
        set
        {
            if (_kalem.KomisyonOrani != value)
            {
                _kalem.KomisyonOrani = value;
                OnPropertyChanged();
                RecalculateTotals();
            }
        }
    }

    public decimal KomisyonTutari
    {
        get => _kalem.KomisyonTutari;
        private set
        {
            if (_kalem.KomisyonTutari != value)
            {
                _kalem.KomisyonTutari = value;
                OnPropertyChanged();
            }
        }
    }

    public decimal StopajOrani
    {
        get => _kalem.StopajOrani;
        set
        {
            if (_kalem.StopajOrani != value)
            {
                _kalem.StopajOrani = value;
                OnPropertyChanged();
                RecalculateTotals();
            }
        }
    }

    public decimal StopajTutari
    {
        get => _kalem.StopajTutari;
        private set
        {
            if (_kalem.StopajTutari != value)
            {
                _kalem.StopajTutari = value;
                OnPropertyChanged();
            }
        }
    }

    private void RecalculateNetKg()
    {
        _kalem.NetKg = _kalem.BrutKg - _kalem.DaraKg;
        if (_kalem.NetKg < 0) _kalem.NetKg = 0;
        OnPropertyChanged(nameof(NetKg));
        RecalculateTotals();
    }

    private void RecalculateTotals()
    {
        Tutar = NetKg * BirimFiyat;
        RusumTutari = Tutar * RusumOrani / 100;
        KomisyonTutari = Tutar * KomisyonOrani / 100;
        StopajTutari = Tutar * StopajOrani / 100;
        _onChanged?.Invoke();
    }

    /// <summary>
    /// Underlying entity'yi döndürür
    /// </summary>
    public SatisFaturasiKalem ToEntity() => _kalem;

    /// <summary>
    /// Entity'den wrapper oluşturur
    /// </summary>
    public static SatisFaturasiKalemVM FromEntity(SatisFaturasiKalem kalem, Action? onChanged = null)
    {
        var vm = new SatisFaturasiKalemVM(kalem, onChanged)
        {
            Urun = kalem.Urun,
            KapTipi = kalem.KapTipi
        };
        return vm;
    }
}
