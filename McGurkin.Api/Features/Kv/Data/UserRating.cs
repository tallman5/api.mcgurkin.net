using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace McGurkin.Api.Features.Kv.Data;

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

    [JsonIgnore]
    public UserProfile UserProfile { get; set; } = null!;
}
