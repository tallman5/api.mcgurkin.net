namespace McGurkin.Api.Features.Tmdb.Data;

public class GetGenresRs
{
    public required Genre[] genres { get; set; }
}

public class Genre
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? PageName { get; set; }
}
