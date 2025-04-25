namespace McGurkin.Api.Features.Tmdb.Data;

public class Images
{
    public Image[] profiles { get; set; }
    public Image[] backdrops { get; set; }
    public Image[] posters { get; set; }
}

public class Image
{
    public float aspect_ratio { get; set; }
    public string file_path { get; set; }
    public int height { get; set; }
    public object iso_639_1 { get; set; }
    public float vote_average { get; set; }
    public int vote_count { get; set; }
    public int width { get; set; }
}

public class TaggedImages
{
    public int page { get; set; }
    public TaggedImage[] results { get; set; }
    public int total_pages { get; set; }
    public int total_results { get; set; }
}

public class TaggedImage
{
    public float aspect_ratio { get; set; }
    public string file_path { get; set; }
    public int height { get; set; }
    public string id { get; set; }
    public string iso_639_1 { get; set; }
    public float vote_average { get; set; }
    public int vote_count { get; set; }
    public int width { get; set; }
    public string image_type { get; set; }
    public Media media { get; set; }
    public string media_type { get; set; }
}

public class Media
{
    public string original_title { get; set; }
    public string poster_path { get; set; }
    public bool video { get; set; }
    public float vote_average { get; set; }
    public string overview { get; set; }
    public string release_date { get; set; }
    public int id { get; set; }
    public bool adult { get; set; }
    public string backdrop_path { get; set; }
    public int vote_count { get; set; }
    public int[] genre_ids { get; set; }
    public string title { get; set; }
    public string original_language { get; set; }
    public float popularity { get; set; }
}
