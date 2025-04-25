namespace McGurkin.Api.Features.Tmdb.Data;

public class GetProvidersRs
{
    public int id { get; set; }
    public ProviderDetails[] results { get; set; }
}

public class GetProviderRs
{
    public int id { get; set; }
    public CountryWatchProvider results { get; set; }
}

public class WatchProviderResponse
{
    public int Id { get; set; }
    public Dictionary<string, CountryWatchProvider> Results { get; set; } = [];
}

public class CountryWatchProvider
{
    public string Link { get; set; }

    public ProviderDetails[]? Rent { get; set; }
    public ProviderDetails[]? Buy { get; set; }
    public ProviderDetails[]? Flatrate { get; set; }
    public ProviderDetails[]? Ads { get; set; }
}

public class ProviderDetails
{
    public string logo_path { get; set; }

    public int provider_id { get; set; }

    public string provider_name { get; set; }

    public int display_priority { get; set; }
}
