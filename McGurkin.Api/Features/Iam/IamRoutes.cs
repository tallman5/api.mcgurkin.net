using McGurkin.Api.Features.Iam.Data;
using McGurkin.Api.Features.Iam.Data.Requests;
using McGurkin.ServiceProviders;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using RegisterRequest = McGurkin.Api.Features.Iam.Data.Requests.RegisterRequest;
using ResetPasswordRequest = McGurkin.Api.Features.Iam.Data.Requests.ResetPasswordRequest;

namespace McGurkin.Api.Features.Iam;

public static class IamRoutes
{
    public static void MapIamRoutes(this WebApplication app)
    {
        var group = app.MapGroup("iam")
            .WithTags("Identity & Access Management")
            .WithOpenApi();

        // Authentication endpoints
        group.MapPost("signin", SignInAsync)
            .Produces<Response<Token>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError);
        group.MapPost("register", RegisterAsync)
            .Produces<Response<string>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);
        group.MapGet("guest-token", GetGuestTokenAsync)
            .Produces<Task<Token>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);

        // Account management endpoints
        group.MapPost("change-password", ChangePasswordAsync).RequireAuthorization()
            .Produces<Response<string>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);
        group.MapPost("delete-account", DeleteAccountAsync).RequireAuthorization()
            .Produces<Response<string>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);
        group.MapGet("download-data", DownloadMyDataAsync).RequireAuthorization()
            .Produces<Response<Dictionary<string, string>>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);

        // Email confirmation endpoints
        group.MapGet("confirm-email", ConfirmEmailAsync)
            .Produces<Response<string>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);
        group.MapPost("resend-confirmation", ResendConfirmationAsync)
            .Produces<Response<string>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);

        // Password recovery endpoints
        group.MapPost("forgot-password", ForgotPasswordAsync)
            .Produces<Response<string>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);
        group.MapPost("reset-password", ResetPasswordAsync)
            .Produces<Response<string>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> SignInAsync(
        [FromServices] IIamService svc,
        [FromBody] SignInRequest request,
        [FromHeader(Name = Constants.X_CORRELATION_ID)] Guid? correlationId)
    {
        try
        {
            var response = await svc.SignInAsync(request);
            return Results.Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Unauthorized();
        }
        catch (Exception ex)
        {
            return Results.Problem("An unexpected error occurred during sign in.");
        }
    }

    private static async Task<IResult> RegisterAsync(
        [FromServices] IIamService svc,
        [FromBody] RegisterRequest request,
        [FromHeader(Name = Constants.X_CORRELATION_ID)] Guid? correlationId)
    {
        try
        {
            var response = await svc.RegisterAsync(request);
            return response.ResponseType == ResponseTypes.Error
                ? Results.BadRequest(response)
                : Results.Ok(response);
        }
        catch (Exception ex)
        {
            return Results.Problem("An unexpected error occurred during registration.");
        }
    }

    private static async Task<IResult> GetGuestTokenAsync(
        [FromServices] IIamService svc,
        [FromHeader(Name = Constants.X_CORRELATION_ID)] Guid? correlationId,
        [FromQuery] bool expired = false)
    {
        try
        {
            var token = await svc.GetGuestTokenAsync(expired);
            return Results.Ok(token);
        }
        catch (Exception ex)
        {
            return Results.Problem("An unexpected error occurred while generating guest token.");
        }
    }

    private static async Task<IResult> ChangePasswordAsync(
        [FromServices] IIamService svc,
        [FromBody] ChangePasswordRequest request,
        ClaimsPrincipal user,
        [FromHeader(Name = Constants.X_CORRELATION_ID)] Guid? correlationId)
    {
        try
        {
            var response = await svc.ChangePasswordAsync(request, user);
            return response.ResponseType == ResponseTypes.Error
                ? Results.BadRequest(response)
                : Results.Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Unauthorized();
        }
        catch (Exception ex)
        {
            return Results.Problem("An unexpected error occurred while changing password.");
        }
    }

    private static async Task<IResult> DeleteAccountAsync(
        [FromServices] IIamService svc,
        [FromBody] SignInRequest request,
        ClaimsPrincipal user,
        [FromHeader(Name = Constants.X_CORRELATION_ID)] Guid? correlationId)
    {
        try
        {
            var response = await svc.DeleteAccountAsync(request, user);
            return Results.Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Unauthorized();
        }
        catch (Exception ex)
        {
            return Results.Problem("An unexpected error occurred while deleting account.");
        }
    }

    private static async Task<IResult> DownloadMyDataAsync(
        [FromServices] IIamService svc,
        ClaimsPrincipal user,
        [FromHeader(Name = Constants.X_CORRELATION_ID)] Guid? correlationId)
    {
        try
        {
            var response = await svc.DownloadMyDataAsync(user);
            return Results.Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Unauthorized();
        }
        catch (Exception ex)
        {
            return Results.Problem("An unexpected error occurred while downloading user data.");
        }
    }

    private static async Task<IResult> ConfirmEmailAsync(
        [FromServices] IIamService svc,
        [FromQuery] string userId,
        [FromQuery] string code,
        [FromHeader(Name = Constants.X_CORRELATION_ID)] Guid? correlationId)
    {
        try
        {
            var request = new ConfirmEmailRequest { UserId = userId, Code = code };
            var response = await svc.ConfirmEmailAsync(request);
            return response.ResponseType == ResponseTypes.Error
                ? Results.BadRequest(response)
                : Results.Ok(response);
        }
        catch (Exception ex)
        {
            return Results.Problem("An unexpected error occurred during email confirmation.");
        }
    }

    private static async Task<IResult> ResendConfirmationAsync(
        [FromServices] IIamService svc,
        [FromBody] ResendConfirmationRequest request,
        [FromHeader(Name = Constants.X_CORRELATION_ID)] Guid? correlationId)
    {
        try
        {
            var response = await svc.ResendConfirmationAsync(request);
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            return Results.Problem("An unexpected error occurred while resending confirmation.");
        }
    }

    private static async Task<IResult> ForgotPasswordAsync(
        [FromServices] IIamService svc,
        [FromBody] ForgotPasswordRequest request,
        [FromQuery] string origin,
        [FromHeader(Name = Constants.X_CORRELATION_ID)] Guid? correlationId)
    {
        try
        {
            var response = await svc.ForgotPasswordAsync(request, origin);
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            return Results.Problem("An unexpected error occurred while processing password reset.");
        }
    }

    private static async Task<IResult> ResetPasswordAsync(
        [FromServices] IIamService svc,
        [FromBody] ResetPasswordRequest request,
        [FromHeader(Name = Constants.X_CORRELATION_ID)] Guid? correlationId)
    {
        try
        {
            var response = await svc.ResetPasswordAsync(request);
            return response.ResponseType == ResponseTypes.Error
                ? Results.BadRequest(response)
                : Results.Ok(response);
        }
        catch (Exception ex)
        {
            return Results.Problem("An unexpected error occurred while resetting password.");
        }
    }
}