using System.ComponentModel.DataAnnotations;

namespace McGurkin.Api.Features.Kv.Data;

public class UserProfile
{
    [Key]
    public Guid UserProfileId { get; set; }

    public required string UserEmail { get; set; }

    public List<UserProvider>? UserProviders { get; set; }

    public List<UserRating>? UserRatings { get; set; }

    public bool ShowAllChannels { get; set; }

    public bool ShowHidden { get; set; }

    public bool ShowRated { get; set; }

    public bool ShowWatchList { get; set; }
}

public class UserProvider
{
    [Key]
    public Guid Id { get; set; }
    public Guid UserProfileId { get; set; }
    public int ProviderId { get; set; }
}

public class UserRating
{
    [Key]
    public Guid Id { get; set; }

    public bool InWatchlist { get; set; }

    public bool IsHidden { get; set; }

    public int MovieId { get; set; }

    public int Stars { get; set; }

    public int TvId { get; set; }
}
