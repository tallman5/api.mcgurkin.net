namespace McGurkin.Api.Features.Tmdb;

public class TmdbServiceConfig
{
    public required string ApiKey { get; set; }
    public required string ApiUrl { get; set; }
    public static TmdbServiceConfig FromConfiguration(IConfiguration configuration)
    {
        return configuration.GetRequiredSection("TmdbServiceConfig").Get<TmdbServiceConfig>() ?? throw new Exception("Configuration section TmdbServiceConfig is required.");
    }
}
