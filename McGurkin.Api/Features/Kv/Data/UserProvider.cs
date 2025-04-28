using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace McGurkin.Api.Features.Kv.Data;

public class UserProvider
{
    [Key]
    public Guid UserProviderId { get; set; }

    [ForeignKey(nameof(UserProfile))]
    public Guid UserProfileId { get; set; }

    public int ProviderId { get; set; }

    [JsonIgnore]
    public UserProfile UserProfile { get; set; } = null!;
}
