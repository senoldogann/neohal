using Avalonia.Data.Converters;
using NeoHal.Desktop.ViewModels;
using System;
using System.Globalization;

namespace NeoHal.Desktop.Converters;

public class ActivePageConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ViewModelBase viewModel && parameter is string pageName)
        {
            // Map ViewModel types to page names
            return pageName switch
            {
                "Dashboard" => viewModel is null, // Dashboard is null in MainWindowViewModel
                "CariHesaplar" => viewModel is CariHesapViewModel,
                "Urunler" => viewModel is UrunViewModel,
                "UrunGruplari" => viewModel is UrunGrubuViewModel,
                "KapTipleri" => viewModel is KapTipiViewModel,
                "GirisIrsaliye" => viewModel is GirisIrsaliyesiListViewModel,
                "SatisFatura" => viewModel is SatisFaturasiListViewModel,
                "HizliSatis" => viewModel is HizliSatisViewModel,
                "KasaHesabi" => viewModel is KasaHesabiViewModel,
                "KasaTakip" => viewModel is KasaTakipViewModel,
                "HalKayit" => viewModel is HalKayitViewModel,
                "SevkiyatGiris" => viewModel is SevkiyatGirisViewModel,
                "SubeBorcRaporu" => viewModel is SubeBorcRaporuViewModel,
                "FaturaListesi" => viewModel is FaturaListesiViewModel,
                "StokDurumu" => viewModel is StokDurumuViewModel,
                "CariExtre" => viewModel is CariExtreViewModel,
                "GunlukRapor" => viewModel is GunlukRaporViewModel,
                "SubeTahsilat" => viewModel is SubeTahsilatViewModel,
                "Kullanicilar" => viewModel is KullaniciViewModel,
                "Yedekleme" => viewModel is BackupViewModel,
                "RaporMerkezi" => viewModel is RaporViewModel,
                _ => false
            };
        }
        
        // Special case for Dashboard when value is null (if that's how it's handled)
        if (value is null && parameter is string p && p == "Dashboard")
        {
            return true;
        }

        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
