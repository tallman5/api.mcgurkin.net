namespace McGurkin.Api.Features.Kv;

public class KvServiceConfig
{
    public required string DbConnectionString { get; set; }

    public static string SCHEMA = "kv";
    public static string HISTORY_TABLE = "__EFMigrationsHistory";

    public static KvServiceConfig FromConfiguration(IConfiguration configuration)
    {
        return configuration.GetRequiredSection("KvServiceConfig").Get<KvServiceConfig>() ?? throw new Exception("Configuration section KvService is required.");
    }
}
