namespace McGurkin.Api.Features.Tmdb.Data;

public class Videos
{
    public Video[] results { get; set; }

    internal void SetEmbedLinks()
    {
        // <iframe width="560" height="315" src="https://www.youtube.com/embed/sfM7_JLk-84" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
        // YouTube https://youtu.be/sfM7_JLk-84

        foreach (var v in results)
        {
            if (v.site == "YouTube")
            {
                v.embed_iframe = string.Format("<iframe title='{0}' src='https://www.youtube.com/embed/{1}' frameborder='0' allow='accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture' allowfullscreen></iframe>", v.name, v.key);
            }
        }
    }
}

public class Video
{
    public string embed_iframe { get; set; }
    public string id { get; set; }
    public string iso_639_1 { get; set; }
    public string iso_3166_1 { get; set; }
    public string key { get; set; }
    public string name { get; set; }
    public string site { get; set; }
    public int size { get; set; }
    public string type { get; set; }
}
