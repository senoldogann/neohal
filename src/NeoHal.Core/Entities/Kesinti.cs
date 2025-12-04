using NeoHal.Core.Common;
using NeoHal.Core.Enums;

namespace NeoHal.Core.Entities;

/// <summary>
/// Kesinti Tanımları - Stopaj, Rüsum, Komisyon vb.
/// </summary>
public class KesintTanimi : BaseEntity
{
    public string Kod { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;
    public KesintTipi Tip { get; set; }
    
    // Hesaplama
    public HesaplamaTipi HesaplamaTipi { get; set; }
    public decimal Oran { get; set; } = 0;
    public decimal SabitTutar { get; set; } = 0;
    
    // Uygulama Kuralları
    public decimal MinTutar { get; set; } = 0;
    public decimal MaxTutar { get; set; } = decimal.MaxValue;
    public bool MustahsildenKesilir { get; set; } = true;
    public bool AlicidanKesilir { get; set; } = false;
    
    // Muhasebe
    public string? MuhasebeHesapKodu { get; set; }
    
    public new bool Aktif { get; set; } = true;
}

/// <summary>
/// Kesinti Hesaplama Sonucu
/// </summary>
public class KesintHesaplama : BaseEntity
{
    public Guid ReferansBelgeId { get; set; }
    public string ReferansBelgeTipi { get; set; } = string.Empty;
    
    public Guid KesintTanimiId { get; set; }
    
    public decimal Matrah { get; set; }
    public decimal Oran { get; set; }
    public decimal Tutar { get; set; }
    
    // Navigation
    public virtual KesintTanimi KesintTanimi { get; set; } = null!;
}
