using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NeoHal.Core.Entities;
using NeoHal.Data.Context;
using NeoHal.Services.Interfaces;

namespace NeoHal.Services.Implementations;

/// <summary>
/// Kullanıcı Yönetimi Servisi
/// </summary>
public class KullaniciService : IKullaniciService
{
    private readonly NeoHalDbContext _context;
    private readonly ILogger<KullaniciService> _logger;
    
    public Kullanici? AktifKullanici { get; private set; }
    public bool GirisYapildi => AktifKullanici != null;
    
    public event EventHandler<Kullanici>? KullaniciGirisYapti;
    public event EventHandler? KullaniciCikisYapti;

    public KullaniciService(NeoHalDbContext context, ILogger<KullaniciService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Authentication

    public async Task<KullaniciGirisSonucu> GirisYapAsync(string kullaniciAdi, string sifre)
    {
        var sonuc = new KullaniciGirisSonucu();
        
        try
        {
            var kullanici = await _context.Kullanicilar
                .FirstOrDefaultAsync(k => k.KullaniciAdi == kullaniciAdi && k.Aktif);
            
            if (kullanici == null)
            {
                sonuc.Mesaj = "Kullanıcı bulunamadı veya aktif değil!";
                _logger.LogWarning("Başarısız giriş denemesi: {KullaniciAdi}", kullaniciAdi);
                return sonuc;
            }
            
            var sifreHash = HashPassword(sifre);
            if (kullanici.SifreHash != sifreHash)
            {
                sonuc.Mesaj = "Hatalı şifre!";
                _logger.LogWarning("Hatalı şifre denemesi: {KullaniciAdi}", kullaniciAdi);
                return sonuc;
            }
            
            // Son giriş tarihini güncelle
            kullanici.SonGirisTarihi = DateTime.Now;
            await _context.SaveChangesAsync();
            
            AktifKullanici = kullanici;
            
            sonuc.Basarili = true;
            sonuc.Mesaj = "Giriş başarılı!";
            sonuc.Kullanici = kullanici;
            sonuc.Yetkiler = await GetYetkilerAsync(kullanici.Id);
            
            _logger.LogInformation("Kullanıcı giriş yaptı: {KullaniciAdi}", kullaniciAdi);
            KullaniciGirisYapti?.Invoke(this, kullanici);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Giriş hatası");
            sonuc.Mesaj = $"Giriş hatası: {ex.Message}";
        }
        
        return sonuc;
    }

    public Task CikisYapAsync()
    {
        var kullaniciAdi = AktifKullanici?.KullaniciAdi;
        AktifKullanici = null;
        
        _logger.LogInformation("Kullanıcı çıkış yaptı: {KullaniciAdi}", kullaniciAdi);
        KullaniciCikisYapti?.Invoke(this, EventArgs.Empty);
        
        return Task.CompletedTask;
    }

    public async Task<bool> SifreDegistirAsync(Guid kullaniciId, string eskiSifre, string yeniSifre)
    {
        var kullanici = await _context.Kullanicilar.FindAsync(kullaniciId);
        if (kullanici == null)
            return false;
        
        var eskiHash = HashPassword(eskiSifre);
        if (kullanici.SifreHash != eskiHash)
        {
            _logger.LogWarning("Şifre değiştirme: Eski şifre hatalı - {KullaniciAdi}", kullanici.KullaniciAdi);
            return false;
        }
        
        kullanici.SifreHash = HashPassword(yeniSifre);
        kullanici.GuncellemeTarihi = DateTime.Now;
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Şifre değiştirildi: {KullaniciAdi}", kullanici.KullaniciAdi);
        return true;
    }

    #endregion

    #region CRUD

    public async Task<Kullanici?> GetByIdAsync(Guid id)
    {
        return await _context.Kullanicilar.FindAsync(id);
    }

    public async Task<Kullanici?> GetByKullaniciAdiAsync(string kullaniciAdi)
    {
        return await _context.Kullanicilar
            .FirstOrDefaultAsync(k => k.KullaniciAdi == kullaniciAdi);
    }

    public async Task<IEnumerable<Kullanici>> GetAllAsync()
    {
        return await _context.Kullanicilar
            .OrderBy(k => k.KullaniciAdi)
            .ToListAsync();
    }

    public async Task<Kullanici> CreateAsync(KullaniciOlusturDto dto)
    {
        // Kullanıcı adı benzersiz mi kontrol et
        var mevcut = await GetByKullaniciAdiAsync(dto.KullaniciAdi);
        if (mevcut != null)
            throw new InvalidOperationException($"'{dto.KullaniciAdi}' kullanıcı adı zaten mevcut!");
        
        var kullanici = new Kullanici
        {
            KullaniciAdi = dto.KullaniciAdi,
            SifreHash = HashPassword(dto.Sifre),
            AdSoyad = dto.AdSoyad,
            Rol = dto.Rol,
            Yetkiler = JsonSerializer.Serialize(dto.Yetkiler.Any() 
                ? dto.Yetkiler 
                : Yetkiler.RolYetkileri(dto.Rol))
        };
        
        _context.Kullanicilar.Add(kullanici);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Yeni kullanıcı oluşturuldu: {KullaniciAdi}", kullanici.KullaniciAdi);
        return kullanici;
    }

    public async Task<Kullanici> UpdateAsync(KullaniciGuncelleDto dto)
    {
        var kullanici = await _context.Kullanicilar.FindAsync(dto.Id);
        if (kullanici == null)
            throw new InvalidOperationException("Kullanıcı bulunamadı!");
        
        if (!string.IsNullOrEmpty(dto.AdSoyad))
            kullanici.AdSoyad = dto.AdSoyad;
        
        if (!string.IsNullOrEmpty(dto.Rol))
            kullanici.Rol = dto.Rol;
        
        if (dto.Yetkiler != null)
            kullanici.Yetkiler = JsonSerializer.Serialize(dto.Yetkiler);
        
        if (!string.IsNullOrEmpty(dto.YeniSifre))
            kullanici.SifreHash = HashPassword(dto.YeniSifre);
        
        kullanici.GuncellemeTarihi = DateTime.Now;
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Kullanıcı güncellendi: {KullaniciAdi}", kullanici.KullaniciAdi);
        return kullanici;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var kullanici = await _context.Kullanicilar.FindAsync(id);
        if (kullanici == null)
            return false;
        
        // Admin kullanıcısı silinemez
        if (kullanici.Rol == KullaniciRolleri.Admin && 
            await _context.Kullanicilar.CountAsync(k => k.Rol == KullaniciRolleri.Admin) == 1)
        {
            throw new InvalidOperationException("Son admin kullanıcısı silinemez!");
        }
        
        _context.Kullanicilar.Remove(kullanici);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Kullanıcı silindi: {KullaniciAdi}", kullanici.KullaniciAdi);
        return true;
    }

    public async Task<bool> AktiflikDegistirAsync(Guid id, bool aktif)
    {
        var kullanici = await _context.Kullanicilar.FindAsync(id);
        if (kullanici == null)
            return false;
        
        kullanici.Aktif = aktif;
        kullanici.GuncellemeTarihi = DateTime.Now;
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Kullanıcı aktifliği değiştirildi: {KullaniciAdi} = {Aktif}", 
            kullanici.KullaniciAdi, aktif);
        return true;
    }

    #endregion

    #region Yetkilendirme

    public async Task<bool> YetkiKontrolAsync(Guid kullaniciId, string yetki)
    {
        var yetkiler = await GetYetkilerAsync(kullaniciId);
        return yetkiler.Contains(yetki);
    }

    public async Task<List<string>> GetYetkilerAsync(Guid kullaniciId)
    {
        var kullanici = await _context.Kullanicilar.FindAsync(kullaniciId);
        if (kullanici == null)
            return new List<string>();
        
        // Admin her şeye yetkili
        if (kullanici.Rol == KullaniciRolleri.Admin)
            return Yetkiler.TumYetkiler();
        
        // Kaydedilmiş yetkiler
        if (!string.IsNullOrEmpty(kullanici.Yetkiler))
        {
            try
            {
                return JsonSerializer.Deserialize<List<string>>(kullanici.Yetkiler) ?? new();
            }
            catch
            {
                return Yetkiler.RolYetkileri(kullanici.Rol);
            }
        }
        
        // Varsayılan rol yetkileri
        return Yetkiler.RolYetkileri(kullanici.Rol);
    }

    public async Task<bool> YetkiAtaAsync(Guid kullaniciId, List<string> yetkiler)
    {
        var kullanici = await _context.Kullanicilar.FindAsync(kullaniciId);
        if (kullanici == null)
            return false;
        
        kullanici.Yetkiler = JsonSerializer.Serialize(yetkiler);
        kullanici.GuncellemeTarihi = DateTime.Now;
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Kullanıcı yetkileri güncellendi: {KullaniciAdi}", kullanici.KullaniciAdi);
        return true;
    }

    #endregion

    #region Yardımcı Metodlar

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "NeoHal_Salt_2024"));
        return Convert.ToBase64String(hashedBytes);
    }

    #endregion
}
