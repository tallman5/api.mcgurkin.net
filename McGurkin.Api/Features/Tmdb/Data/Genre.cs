using McGurkin.Api.Features.Kv.Data;
using System.Text.Json.Serialization;

namespace McGurkin.Api.Features.Tmdb.Data;

public class GetGenresRs
{
    public required Genre[] genres { get; set; }
}

public class Genre : IntId
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("pageName")]
    public string? PageName { get; set; }
}
