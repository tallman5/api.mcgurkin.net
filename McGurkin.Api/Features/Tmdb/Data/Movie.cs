namespace McGurkin.Api.Features.Tmdb.Data;

public class Movie
{
    public bool adult { get; set; }
    public string backdrop_path { get; set; }
    //public object belongs_to_collection { get; set; }
    //public int budget { get; set; }
    public Credits credits { get; set; }
    public Genre[] genres { get; set; }
    //public string homepage { get; set; }
    public int id { get; set; }
    public Images images { get; set; }
    //public string imdb_id { get; set; }
    //public string original_language { get; set; }
    //public string original_title { get; set; }
    public string overview { get; set; }
    //public float popularity { get; set; }
    public string poster_path { get; set; }
    //public List<ProductionCompany> production_companies { get; set; }
    //public List<Country> production_countries { get; set; }
    //public MovieProviderCountries provider_countries { get; set; }
    public Dictionary<string, CountryWatchProvider> providers { get; set; }
    public string release_date { get; set; }
    //public int? revenue { get; set; }
    public int runtime { get; set; }
    //public GetMoviesResponse similar { get; set; }
    //public List<Language> spoken_languages { get; set; }
    //public string status { get; set; }
    public string tagline { get; set; }
    public string title { get; set; }
    //public bool video { get; set; }
    public Videos videos { get; set; }
    //public float vote_average { get; set; }
    //public int vote_count { get; set; }
}

public class GetMoviesRs
{
    public int page { get; set; }
    public int total_results { get; set; }
    public int total_pages { get; set; }
    public Movie[] results { get; set; }
}

public class GetMovieIdsRs
{
    public int page { get; set; }
    public int total_results { get; set; }
    public int total_pages { get; set; }
    public ItemId[] results { get; set; }
}

public class ItemId
{
    public int id { get; set; }
}
