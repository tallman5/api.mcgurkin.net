namespace McGurkin.Api.Features.Iam;

public class IamServiceConfig
{
    public static string SCHEMA = "iam";
    public static string HISTORY_TABLE = "__EFMigrationsHistory";

    public required string DbConnectionString { get; set; }
    public required string Issuer { get; set; }
    public required string IssuerKey { get; set; }

    public static IamServiceConfig FromConfiguration(IConfiguration configuration)
    {
        return configuration.GetRequiredSection("IamServiceConfig").Get<IamServiceConfig>() ?? throw new Exception("Configuration section IamService is required.");
    }
}
