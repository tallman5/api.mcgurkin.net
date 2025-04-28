namespace McGurkin.Api.Features.Iam.Data.Requests;

public class ConfirmEmailRequest
{
    public required string UserId { get; set; }
    public required string Code { get; set; }

    //public static bool TryParse(string? s, IFormatProvider? provider, out ConfirmEmailRequest result)
    //{
    //    result = null;
    //    if (string.IsNullOrEmpty(s))
    //    {
    //        return false;
    //    }

    //    var parts = s.Split(',');
    //    if (parts.Length != 2)
    //    {
    //        return false;
    //    }

    //    result = new ConfirmEmailRequest
    //    {
    //        UserId = parts[0],
    //        Code = parts[1]
    //    };
    //    return true;
    //}

    //public static ConfirmEmailRequest Parse(string s, IFormatProvider? provider)
    //{
    //    if (TryParse(s, provider, out var result))
    //    {
    //        return result;
    //    }
    //    throw new FormatException("Invalid format for ConfirmEmailRequest.");
    //}
}
