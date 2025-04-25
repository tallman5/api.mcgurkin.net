using McGurkin.Api.Features.Tmdb.Data;
using Microsoft.EntityFrameworkCore;

namespace McGurkin.Api.Features.Kv.Data;

public class KvDbContext(DbContextOptions<KvDbContext> options) : DbContext(options)
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("KvServiceConfig:DbConnectionString",
                opt => opt.MigrationsHistoryTable(KvServiceConfig.HISTORY_TABLE, KvServiceConfig.SCHEMA)
            );
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema(KvServiceConfig.SCHEMA);
    }

    public DbSet<UserProfile> UserProfiles { get; set; } = null!;
    public DbSet<UserProvider> UserProviders { get; set; } = null!;
    public DbSet<UserRating> UserRatings { get; set; } = null!;
}
