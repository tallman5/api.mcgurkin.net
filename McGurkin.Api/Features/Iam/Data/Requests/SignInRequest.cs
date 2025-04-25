using System.ComponentModel.DataAnnotations;

namespace McGurkin.Api.Features.Iam.Data.Requests;

public class SignInRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public required string Password { get; set; }

    [Required]
    public bool RememberMe { get; set; }
}
