namespace McGurkin.Api.Features.Iam.Data;

public class Token
{
    public DateTimeOffset Expires { get; set; }
    public required string[] Roles { get; set; }
    public string? ScreenName { get; set; }
    public required string AccessToken { get; set; }
    public required string UserName { get; set; }
}

public static class CustomClaimTypes
{
    public const string ScreenName = "screen_name";
}
