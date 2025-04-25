using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace McGurkin.Api.Features.Iam.Data;

public class IamUser : IdentityUser
{
    [Display(Name = "Screen Name")]
    public required string ScreenName { get; set; }
}
