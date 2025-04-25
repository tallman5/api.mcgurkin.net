﻿namespace McGurkin.Api.Features.Tmdb.Data;

public class Credits
{
    public Credit[] cast { get; set; }
    public Credit[] crew { get; set; }
}

public class Credit
{
    public bool adult { get; set; }
    public string backdrop_path { get; set; }
    public int cast_id { get; set; }
    public string character { get; set; }
    public string credit_id { get; set; }
    public string department { get; set; }
    //public int gender { get; set; }
    public int[] genre_ids { get; set; }
    public int id { get; set; }
    public string job { get; set; }
    //public string known_for_department { get; set; }
    public string name { get; set; }
    public int order { get; set; }
    //public string original_language { get; set; }
    //public string original_name { get; set; }
    //public string original_title { get; set; }
    //public float popularity { get; set; }
    public string overview { get; set; }
    public string poster_path { get; set; }
    public string profile_path { get; set; }
    public string release_date { get; set; }
    public string title { get; set; }
    //public bool video { get; set; }
    //public float vote_average { get; set; }
    //public int vote_count { get; set; }
}
