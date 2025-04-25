using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace McGurkin.Api.Features.Iam.Data;

public class IamDbContextFactory : IDesignTimeDbContextFactory<IamDbContext>
{
    public IamDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        var connectionString = configuration["IamServiceConfig:DbConnectionString"];
        Console.WriteLine($"Iam Connection string: {connectionString}");

        var optionsBuilder = new DbContextOptionsBuilder<IamDbContext>();
        optionsBuilder.UseSqlServer(connectionString, sql =>
        {
            sql.MigrationsHistoryTable(IamServiceConfig.HISTORY_TABLE, IamServiceConfig.SCHEMA);
        });

        return new IamDbContext(optionsBuilder.Options);
    }
}
