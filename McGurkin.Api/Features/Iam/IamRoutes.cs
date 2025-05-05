using McGurkin.Api.Features.Iam.Data;
using McGurkin.Api.Features.Iam.Data.Requests;
using McGurkin.ServiceProviders;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using RegisterRequest = McGurkin.Api.Features.Iam.Data.Requests.RegisterRequest;

namespace McGurkin.Api.Features.Iam;

public static class IamRoutes
{
    public static void MapIamRoutes(this WebApplication app)
    {
        var group = app.MapGroup("iam")
            .WithTags("Identity & Access Management")
            .WithOpenApi();

        MapDeleteAccount(group);
        MapGetConfirmEmail(group);
        MapGetGuestToken(group);
        MapGetMyData(group);
        MapPostChangePassword(group);
        MapPostForgotPassword(group);
        MapPostRegister(group);
        MapPostResendConfirmation(group);
        MapPostResetPassword(group);
        MapPostSignIn(group);
    }

    private static void MapDeleteAccount(RouteGroupBuilder group)
    {
        group.MapDelete("delete-account", async (
            ClaimsPrincipal user,
            [FromServices] IIamService svc,
            [FromBody] SignInRequest request) =>
        {
            try
            {
                var response = new Response<string>
                {
                    Data = await svc.DeleteAccountAsync(request, user)
                };
                return Results.Ok(response);
            }
            catch (UnauthorizedAccessException uaex)
            {
                return Results.Problem(uaex.Message, statusCode: StatusCodes.Status401Unauthorized);
            }
            catch
            {
                return Results.Problem("An unexpected error occurred while deleting the account.", statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .RequireAuthorization()
        .Produces<Response<string>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status500InternalServerError);
    }

    private static void MapGetConfirmEmail(RouteGroupBuilder group)
    {
        group.MapGet("confirm-email", async (
            [FromServices] IIamService svc,
            [FromQuery] string userId,
            [FromQuery] string code) =>
        {
            try
            {
                var request = new ConfirmEmailRequest
                {
                    UserId = userId,
                    Code = code
                };
                var response = new Response<string>
                {
                    Data = await svc.ConfirmEmailAsync(request)
                };
                return Results.Ok(response);
            }
            catch
            {
                return Results.Problem("An unexpected error occurred while confirming the email.", statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .Produces<Response<string>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);
    }

    private static void MapGetGuestToken(RouteGroupBuilder group)
    {
        group.MapGet("guest-token", async (
            [FromServices] IIamService svc,
            [FromQuery] bool expired = false
            ) =>
        {
            try
            {
                var response = new Response<Token>
                {
                    Data = await svc.GetGuestTokenAsync(expired)
                };
                return Results.Ok(response);
            }
            catch
            {
                return Results.Problem("An unexpected error occurred while retrieving the guest token.", statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .Produces<Response<Token>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status500InternalServerError);
    }

    private static void MapGetMyData(RouteGroupBuilder group)
    {
        group.MapGet("my-data", async (
            ClaimsPrincipal user,
            [FromServices] IIamService svc) =>
        {
            try
            {
                var response = new Response<Dictionary<string, string>>
                {
                    Data = await svc.DownloadMyDataAsync(user)
                };
                return Results.Ok(response);
            }
            catch (UnauthorizedAccessException uaex)
            {
                return Results.Problem(uaex.Message, statusCode: StatusCodes.Status401Unauthorized);
            }
            catch
            {
                return Results.Problem("An unexpected error occurred while retrieving user data.", statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .RequireAuthorization()
        .Produces<Response<Dictionary<string, string>>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status500InternalServerError);
    }

    private static void MapPostChangePassword(RouteGroupBuilder group)
    {
        group.MapPost("change-password", async (
            ClaimsPrincipal user,
            [FromServices] IIamService svc,
            [FromBody] ChangePasswordRequest request) =>
        {
            try
            {
                var response = new Response<string>
                {
                    Data = await svc.ChangePasswordAsync(request, user)
                };
                return Results.Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
            catch
            {
                return Results.Problem("An unexpected error occurred while changing password.");
            }
        })
        .RequireAuthorization()
        .Produces<Response<string>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status500InternalServerError);
    }

    private static void MapPostForgotPassword(RouteGroupBuilder group)
    {
        group.MapPost("forgot-password", async (
            [FromServices] IIamService svc,
            [FromBody] ForgotPasswordRequest request,
            [FromQuery] string origin) =>
        {
            try
            {
                var response = new Response<string>
                {
                    Data = await svc.ForgotPasswordAsync(request, origin)
                };
                return Results.Ok(response);
            }
            catch
            {
                return Results.Problem("An unexpected error occurred while processing the forgot password request.", statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .Produces<Response<string>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);
    }

    private static void MapPostRegister(RouteGroupBuilder group)
    {
        group.MapPost("register", async (
            [FromServices] IIamService svc,
            [FromBody] RegisterRequest request) =>
        {
            try
            {
                var response = new Response<string>
                {
                    Data = await svc.RegisterAsync(request)
                };
                return Results.Ok(response);
            }
            catch
            {
                return Results.Problem("An unexpected error occurred while registering the user.", statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .Produces<Response<string>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);
    }

    private static void MapPostResendConfirmation(RouteGroupBuilder group)
    {
        group.MapPost("resend-confirmation", async (
            [FromServices] IIamService svc,
            [FromBody] ResendConfirmationRequest request) =>
        {
            try
            {
                var response = new Response<string>
                {
                    Data = await svc.ResendConfirmationAsync(request)
                };
                return Results.Ok(response);
            }
            catch
            {
                return Results.Problem("An unexpected error occurred while resending the confirmation email.", statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .Produces<Response<string>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);
    }

    private static void MapPostResetPassword(RouteGroupBuilder group)
    {
        group.MapPost("reset-password", async (
            [FromServices] IIamService svc,
            [FromBody] Data.Requests.ResetPasswordRequest request) =>
        {
            try
            {
                var response = new Response<string>
                {
                    Data = await svc.ResetPasswordAsync(request)
                };
                return Results.Ok(response);
            }
            catch
            {
                return Results.Problem("An unexpected error occurred while resetting the password.", statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .Produces<Response<string>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);
    }

    private static void MapPostSignIn(RouteGroupBuilder group)
    {
        group.MapPost("signin", async (
            [FromServices] IIamService svc,
            [FromBody] SignInRequest request) =>
        {
            try
            {
                var response = new Response<Token>
                {
                    Data = await svc.SignInAsync(request)
                };
                return Results.Ok(response);
            }
            catch (UnauthorizedAccessException uaex)
            {
                return Results.Problem(uaex.Message, statusCode: StatusCodes.Status401Unauthorized);
            }
            catch
            {
                return Results.Problem("An unexpected error occurred during sign in.", statusCode: StatusCodes.Status500InternalServerError);
            }

        })
        .Produces<Response<Token>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status500InternalServerError);
    }
}