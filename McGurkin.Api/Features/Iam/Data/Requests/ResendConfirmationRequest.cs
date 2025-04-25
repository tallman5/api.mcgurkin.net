using System.ComponentModel.DataAnnotations;

namespace McGurkin.Api.Features.Iam.Data.Requests;

public class ResendConfirmationRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    public required string Origin { get; set; }
}
