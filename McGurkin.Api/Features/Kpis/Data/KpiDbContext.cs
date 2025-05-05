using Microsoft.EntityFrameworkCore;

namespace McGurkin.Api.Features.Kpis.Data;

public class KpiDbContext(DbContextOptions<KpiDbContext> options) : DbContext(options)
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("KpiServiceConfig:DbConnectionString",
                opt => opt.MigrationsHistoryTable(KpiServiceConfig.HISTORY_TABLE, KpiServiceConfig.SCHEMA)
            );
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema(KpiServiceConfig.SCHEMA);
    }

    public DbSet<Kpi> Kpis { get; set; } = null!;
}
