using System.Text.Json.Serialization;

namespace McGurkin.Api.Features.Kv.Data;

public class IntId
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
}
