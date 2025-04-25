using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace McGurkin.Api.Features.Iam.Data;

public partial class IamDbContext(DbContextOptions<IamDbContext> options) : IdentityDbContext<IamUser>(options)
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("IamServiceConfig:DbConnectionString",
                opt => opt.MigrationsHistoryTable(IamServiceConfig.HISTORY_TABLE, IamServiceConfig.SCHEMA)
            );
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema(IamServiceConfig.SCHEMA);

        builder.Entity<IamUser>()
            .HasIndex(p => new { p.ScreenName })
            .IsUnique(true);
    }
}
