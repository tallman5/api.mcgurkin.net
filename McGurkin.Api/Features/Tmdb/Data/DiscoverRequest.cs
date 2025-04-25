namespace McGurkin.Api.Features.Tmdb.Data;

public class DiscoverRequest
{
    public int GenreId { get; set; }
    public int Start { get; set; }

    public string GetQueryString()
    {
        string returnValue = "";

        if (Start > 0)
            returnValue += "&page=" + Start.ToString();

        string sortString = "";
        switch (GenreId)
        {
            case 0:
                break;
            case 1:
                sortString = "popularity.desc";
                break;
            case 4:
                sortString = "release_date.desc";
                break;
            case 5:
                sortString = "primary_release_date.desc";
                break;
            case 6:
                sortString = "vote_average.desc";
                break;
            case 7:
                sortString = "vote_count.desc";
                break;
        }

        if (!string.IsNullOrWhiteSpace(sortString))
            returnValue += "&sort_by=" + sortString;

        return returnValue;
    }
}
