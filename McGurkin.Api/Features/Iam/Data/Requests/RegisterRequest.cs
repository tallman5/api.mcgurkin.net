using System.ComponentModel.DataAnnotations;

namespace McGurkin.Api.Features.Iam.Data.Requests;

public class RegisterRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    public required string Origin { get; set; }

    [Required]
    public required string ScreenName { get; set; }

    [Required]
    public required string Password { get; set; }
}
