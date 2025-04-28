using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace McGurkin.Api.Features.Kv.Data;

public class UserProfile
{
    [Key]
    public Guid UserProfileId { get; set; }

    public required string UserEmail { get; set; }

    public List<UserProvider> UserProviders { get; set; } = [];

    public List<UserRating> UserRatings { get; set; } = [];

    public bool ShowAllChannels { get; set; } = false;

    public bool ShowHidden { get; set; } = false;

    public bool ShowRated { get; set; } = false;

    public bool ShowWatchList { get; set; } = true;
}

public class UserProvider
{
    [Key]
    public Guid UserProviderId { get; set; }

    [ForeignKey(nameof(UserProfile))]
    public Guid UserProfileId { get; set; }

    public int ProviderId { get; set; }
}

public class UserRating
{
    [Key]
    public Guid UserRatingId { get; set; }

    [ForeignKey(nameof(UserProfile))]
    public Guid UserProfileId { get; set; }

    public bool InWatchlist { get; set; }

    public bool IsHidden { get; set; }

    public int MovieId { get; set; }

    public int Stars { get; set; }

    public int TvId { get; set; }
}
