using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NeoHal.Core.Entities;
using NeoHal.Services.Interfaces;

namespace NeoHal.Desktop.ViewModels;

/// <summary>
/// Kullanıcı Yönetimi ViewModel
/// </summary>
public partial class KullaniciViewModel : ViewModelBase
{
    private readonly IKullaniciService _kullaniciService;
    
    [ObservableProperty]
    private ObservableCollection<Kullanici> _kullanicilar = new();
    
    [ObservableProperty]
    private Kullanici? _selectedKullanici;
    
    [ObservableProperty]
    private string _statusMessage = "Kullanıcı yönetimi";
    
    [ObservableProperty]
    private bool _isLoading;
    
    // Yeni/Düzenleme Dialog
    [ObservableProperty]
    private bool _dialogAcik;
    
    [ObservableProperty]
    private bool _yeniKayit;
    
    [ObservableProperty]
    private string _dialogKullaniciAdi = string.Empty;
    
    [ObservableProperty]
    private string _dialogSifre = string.Empty;
    
    [ObservableProperty]
    private string _dialogAdSoyad = string.Empty;
    
    [ObservableProperty]
    private string _dialogRol = KullaniciRolleri.Operator;
    
    [ObservableProperty]
    private ObservableCollection<string> _dialogYetkiler = new();
    
    // Şifre Değiştirme Dialog
    [ObservableProperty]
    private bool _sifreDialogAcik;
    
    [ObservableProperty]
    private string _eskiSifre = string.Empty;
    
    [ObservableProperty]
    private string _yeniSifre = string.Empty;
    
    [ObservableProperty]
    private string _yeniSifreTekrar = string.Empty;
    
    // Roller ve Yetkiler
    public ObservableCollection<string> Roller { get; } = new(KullaniciRolleri.Tumu);
    public ObservableCollection<YetkiItem> TumYetkiler { get; } = new();
    
    public KullaniciViewModel(IKullaniciService kullaniciService)
    {
        _kullaniciService = kullaniciService;
        
        // Tüm yetkileri yükle
        foreach (var yetki in Yetkiler.TumYetkiler())
        {
            TumYetkiler.Add(new YetkiItem { Kod = yetki, Secili = false });
        }
        
        _ = LoadDataAsync();
    }
    
    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Kullanıcılar yükleniyor...";
            
            var kullanicilar = await _kullaniciService.GetAllAsync();
            Kullanicilar = new ObservableCollection<Kullanici>(kullanicilar);
            
            StatusMessage = $"✅ {Kullanicilar.Count} kullanıcı";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Hata: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    private async Task YenileAsync()
    {
        await LoadDataAsync();
    }
    
    [RelayCommand]
    private void YeniKullanici()
    {
        YeniKayit = true;
        DialogKullaniciAdi = string.Empty;
        DialogSifre = string.Empty;
        DialogAdSoyad = string.Empty;
        DialogRol = KullaniciRolleri.Operator;
        
        // Varsayılan rol yetkilerini seç
        var rolYetkileri = Yetkiler.RolYetkileri(DialogRol);
        foreach (var yetki in TumYetkiler)
        {
            yetki.Secili = rolYetkileri.Contains(yetki.Kod);
        }
        
        DialogAcik = true;
    }
    
    [RelayCommand]
    private async Task DuzenleAsync()
    {
        if (SelectedKullanici == null)
        {
            StatusMessage = "⚠️ Önce bir kullanıcı seçin!";
            return;
        }
        
        YeniKayit = false;
        DialogKullaniciAdi = SelectedKullanici.KullaniciAdi;
        DialogSifre = string.Empty;
        DialogAdSoyad = SelectedKullanici.AdSoyad;
        DialogRol = SelectedKullanici.Rol;
        
        // Mevcut yetkileri seç
        var yetkiler = await _kullaniciService.GetYetkilerAsync(SelectedKullanici.Id);
        foreach (var yetki in TumYetkiler)
        {
            yetki.Secili = yetkiler.Contains(yetki.Kod);
        }
        
        DialogAcik = true;
    }
    
    [RelayCommand]
    private async Task KaydetAsync()
    {
        try
        {
            IsLoading = true;
            
            var seciliYetkiler = TumYetkiler.Where(y => y.Secili).Select(y => y.Kod).ToList();
            
            if (YeniKayit)
            {
                if (string.IsNullOrWhiteSpace(DialogKullaniciAdi) || string.IsNullOrWhiteSpace(DialogSifre))
                {
                    StatusMessage = "⚠️ Kullanıcı adı ve şifre zorunludur!";
                    return;
                }
                
                var dto = new KullaniciOlusturDto
                {
                    KullaniciAdi = DialogKullaniciAdi,
                    Sifre = DialogSifre,
                    AdSoyad = DialogAdSoyad,
                    Rol = DialogRol,
                    Yetkiler = seciliYetkiler
                };
                
                var yeni = await _kullaniciService.CreateAsync(dto);
                Kullanicilar.Add(yeni);
                StatusMessage = $"✅ '{yeni.KullaniciAdi}' oluşturuldu";
            }
            else
            {
                if (SelectedKullanici == null) return;
                
                var dto = new KullaniciGuncelleDto
                {
                    Id = SelectedKullanici.Id,
                    AdSoyad = DialogAdSoyad,
                    Rol = DialogRol,
                    Yetkiler = seciliYetkiler,
                    YeniSifre = string.IsNullOrWhiteSpace(DialogSifre) ? null : DialogSifre
                };
                
                var guncellenen = await _kullaniciService.UpdateAsync(dto);
                
                // Listeyi güncelle
                var index = Kullanicilar.IndexOf(SelectedKullanici);
                if (index >= 0)
                {
                    Kullanicilar[index] = guncellenen;
                    SelectedKullanici = guncellenen;
                }
                
                StatusMessage = $"✅ '{guncellenen.KullaniciAdi}' güncellendi";
            }
            
            DialogAcik = false;
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Hata: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    private async Task SilAsync()
    {
        if (SelectedKullanici == null)
        {
            StatusMessage = "⚠️ Önce bir kullanıcı seçin!";
            return;
        }
        
        try
        {
            IsLoading = true;
            
            var kullaniciAdi = SelectedKullanici.KullaniciAdi;
            var sonuc = await _kullaniciService.DeleteAsync(SelectedKullanici.Id);
            
            if (sonuc)
            {
                Kullanicilar.Remove(SelectedKullanici);
                SelectedKullanici = null;
                StatusMessage = $"✅ '{kullaniciAdi}' silindi";
            }
            else
            {
                StatusMessage = "❌ Silme işlemi başarısız!";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Hata: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    private async Task AktiflikDegistirAsync()
    {
        if (SelectedKullanici == null)
        {
            StatusMessage = "⚠️ Önce bir kullanıcı seçin!";
            return;
        }
        
        try
        {
            IsLoading = true;
            
            var yeniDurum = !SelectedKullanici.Aktif;
            var sonuc = await _kullaniciService.AktiflikDegistirAsync(SelectedKullanici.Id, yeniDurum);
            
            if (sonuc)
            {
                SelectedKullanici.Aktif = yeniDurum;
                StatusMessage = $"✅ '{SelectedKullanici.KullaniciAdi}' {(yeniDurum ? "aktif" : "pasif")} yapıldı";
                await LoadDataAsync(); // Listeyi yenile
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Hata: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    private void SifreDegistirDialogAc()
    {
        if (SelectedKullanici == null)
        {
            StatusMessage = "⚠️ Önce bir kullanıcı seçin!";
            return;
        }
        
        EskiSifre = string.Empty;
        YeniSifre = string.Empty;
        YeniSifreTekrar = string.Empty;
        SifreDialogAcik = true;
    }
    
    [RelayCommand]
    private async Task SifreDegistirAsync()
    {
        if (SelectedKullanici == null) return;
        
        if (string.IsNullOrWhiteSpace(YeniSifre))
        {
            StatusMessage = "⚠️ Yeni şifre boş olamaz!";
            return;
        }
        
        if (YeniSifre != YeniSifreTekrar)
        {
            StatusMessage = "⚠️ Şifreler eşleşmiyor!";
            return;
        }
        
        try
        {
            IsLoading = true;
            
            var dto = new KullaniciGuncelleDto
            {
                Id = SelectedKullanici.Id,
                YeniSifre = YeniSifre
            };
            
            await _kullaniciService.UpdateAsync(dto);
            
            SifreDialogAcik = false;
            StatusMessage = $"✅ '{SelectedKullanici.KullaniciAdi}' şifresi değiştirildi";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Hata: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    private void DialogKapat()
    {
        DialogAcik = false;
        SifreDialogAcik = false;
    }
    
    partial void OnDialogRolChanged(string value)
    {
        // Rol değiştiğinde varsayılan yetkileri seç
        if (YeniKayit)
        {
            var rolYetkileri = Yetkiler.RolYetkileri(value);
            foreach (var yetki in TumYetkiler)
            {
                yetki.Secili = rolYetkileri.Contains(yetki.Kod);
            }
        }
    }
}

/// <summary>
/// Yetki listesi için item
/// </summary>
public partial class YetkiItem : ObservableObject
{
    [ObservableProperty]
    private string _kod = string.Empty;
    
    [ObservableProperty]
    private bool _secili;
    
    public string Aciklama => Kod.Replace("_", " ");
}
