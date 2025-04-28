using System.ComponentModel.DataAnnotations;

namespace McGurkin.Api.Features.Iam.Data.Requests;

public class SignInRequest
{
    public required string UserName { get; set; }

    [DataType(DataType.Password)]
    public required string Password { get; set; }

    public bool RememberMe { get; set; }
}
