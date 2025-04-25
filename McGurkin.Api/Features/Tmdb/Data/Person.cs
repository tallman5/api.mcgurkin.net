namespace McGurkin.Api.Features.Tmdb.Data;

public class Person
{
    public bool adult { get; set; }
    //public string[] also_known_as { get; set; }
    public string biography { get; set; }
    //public string birthday { get; set; }
    public Credits credits { get; set; }
    //public object deathday { get; set; }
    //public int gender { get; set; }
    public object homepage { get; set; }
    public int id { get; set; }
    public Images images { get; set; }
    //public string imdb_id { get; set; }
    //public string known_for_department { get; set; }
    public string name { get; set; }
    //public string place_of_birth { get; set; }
    //public float popularity { get; set; }
    public string profile_path { get; set; }
    public TaggedImages tagged_images { get; set; }
}

public class GetPeopleRs
{
    public int page { get; set; }
    public Person[] results { get; set; }
    public int total_pages { get; set; }
    public int total_results { get; set; }
}