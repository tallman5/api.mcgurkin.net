namespace McGurkin.Api.Features.Kpis;

public class KpiServiceConfig
{
    public required string DbConnectionString { get; set; }

    public static string SCHEMA = "kpi";
    public static string HISTORY_TABLE = "__EFMigrationsHistory";

    public static KpiServiceConfig FromConfiguration(IConfiguration configuration)
    {
        return configuration.GetRequiredSection("KpiServiceConfig").Get<KpiServiceConfig>() ?? throw new Exception("Configuration section KpiService is required.");
    }
}
