using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NeoHal.Data.Context;

/// <summary>
/// EF Core Migrations için design-time factory
/// </summary>
public class NeoHalDbContextFactory : IDesignTimeDbContextFactory<NeoHalDbContext>
{
    public NeoHalDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<NeoHalDbContext>();
        optionsBuilder.UseSqlite("Data Source=neohal.db");
        
        return new NeoHalDbContext(optionsBuilder.Options);
    }
}
