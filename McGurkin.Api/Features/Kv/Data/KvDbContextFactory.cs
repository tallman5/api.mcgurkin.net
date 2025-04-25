using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace McGurkin.Api.Features.Kv.Data;

public class KvDbContextFactory : IDesignTimeDbContextFactory<KvDbContext>
{
    public KvDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        var connectionString = configuration["KvServiceConfig:DbConnectionString"];
        Console.WriteLine($"Connection string: {connectionString}");
        var optionsBuilder = new DbContextOptionsBuilder<KvDbContext>();
        optionsBuilder.UseSqlServer(connectionString, sql =>
        {
            sql.MigrationsHistoryTable(KvServiceConfig.HISTORY_TABLE, KvServiceConfig.SCHEMA);
        });

        return new KvDbContext(optionsBuilder.Options);
    }
}
