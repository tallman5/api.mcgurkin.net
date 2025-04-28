using System.ComponentModel.DataAnnotations;

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
