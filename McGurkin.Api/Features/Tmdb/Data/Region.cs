namespace McGurkin.Api.Features.Tmdb.Data;

public class GetRegionsRs
{
    public Region[] results { get; set; }
}

public class Region
{
    public string iso_3166_1 { get; set; }
    public string english_name { get; set; }
    public string native_name { get; set; }
}
