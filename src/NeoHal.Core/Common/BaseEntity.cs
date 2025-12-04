using NeoHal.Core.Enums;

namespace NeoHal.Core.Common;

/// <summary>
/// Tüm entity'ler için temel sınıf
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
    public DateTime? GuncellemeTarihi { get; set; }
    public SyncDurumu SyncDurumu { get; set; } = SyncDurumu.Beklemede;
    public bool Aktif { get; set; } = true;
}

/// <summary>
/// Soft delete özellikli entity'ler için
/// </summary>
public abstract class SoftDeleteEntity : BaseEntity
{
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
}
