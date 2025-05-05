using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace McGurkin.Api.Features.Kpis.Data;

public class KpiDbContextFactory : IDesignTimeDbContextFactory<KpiDbContext>
{
    public KpiDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        var connectionString = configuration["KpiServiceConfig:DbConnectionString"];
        Console.WriteLine($"Kpi Connection string: {connectionString}");

        var optionsBuilder = new DbContextOptionsBuilder<KpiDbContext>();
        optionsBuilder.UseSqlServer(connectionString, sql =>
        {
            sql.MigrationsHistoryTable(KpiServiceConfig.HISTORY_TABLE, KpiServiceConfig.SCHEMA);
        });

        return new KpiDbContext(optionsBuilder.Options);
    }
}
