using Microsoft.EntityFrameworkCore;
using NeoHal.Core.Common;
using NeoHal.Core.Interfaces;
using NeoHal.Data.Context;

namespace NeoHal.Data.Repositories;

/// <summary>
/// Generic Repository Implementation
/// </summary>
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly NeoHalDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(NeoHalDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.Where(e => e.Aktif).ToListAsync();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        entity.OlusturmaTarihi = DateTime.Now;
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        entity.GuncellemeTarihi = DateTime.Now;
        _dbSet.Update(entity);
        await Task.CompletedTask;
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            entity.Aktif = false;
            entity.GuncellemeTarihi = DateTime.Now;
            _dbSet.Update(entity);
        }
    }

    public virtual async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbSet.AnyAsync(e => e.Id == id);
    }

    public virtual IQueryable<T> Query()
    {
        return _dbSet.AsQueryable();
    }
}
