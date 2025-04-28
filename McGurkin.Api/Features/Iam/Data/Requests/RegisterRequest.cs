namespace McGurkin.Api.Features.Iam.Data.Requests;

public class RegisterRequest
{
    public required string Email { get; set; }
    public required string Origin { get; set; }
    public required string Password { get; set; }
    public required string UserName { get; set; }
}
