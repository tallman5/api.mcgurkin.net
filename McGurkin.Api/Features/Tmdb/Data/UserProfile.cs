using System.ComponentModel.DataAnnotations;

namespace McGurkin.Api.Features.Tmdb.Data;

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
    public Guid UserProfileId { get; set; }
    public int ProviderId { get; set; }
}
