using McGurkin.Api.Features.Kv.Data;
using McGurkin.ServiceProviders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace McGurkin.Api.Features.Kv;

public static class KvRoutes
{
    public static void MapKvRoutes(this WebApplication app)
    {
        var thisGroup = app.MapGroup("my")
            .WithTags("Profile");

        MapGetMyProfile(thisGroup);
        MapPostToggleProvider(thisGroup);
        MapPostUpsertRating(thisGroup);
    }

    private static string GetUserEmail(ClaimsPrincipal user)
    {
        var email = user.Identity?.Name ?? throw new UnauthorizedAccessException();
        if (email.Equals("guest@kixvu.com", StringComparison.CurrentCultureIgnoreCase))
            throw new UnauthorizedAccessException("Guest account is not allowed to perform this action.");
        return email;
    }

    private static void MapGetMyProfile(RouteGroupBuilder thisGroup)
    {
        thisGroup.MapGet("profile", [Authorize] async (
            ClaimsPrincipal user,
            [FromServices] IKvService svc,
            [FromHeader(Name = Constants.X_CORRELATION_ID)] Guid? correlationId
        ) =>
        {
            var email = GetUserEmail(user);
            var returnValue = new Response<UserProfile>
            {
                Data = await svc.GetOrCreateMyProfileAsync(email)
            };
            return Results.Ok(returnValue);
        })
        .Produces<Response<UserProfile>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status500InternalServerError);
    }

    private static void MapPostToggleProvider(RouteGroupBuilder thisGroup)
    {
        thisGroup.MapPost("toggle-provider/{providerId}", [Authorize] async (
            ClaimsPrincipal user,
            [FromServices] IKvService svc,
            [FromRoute] int providerId,
            [FromHeader(Name = Constants.X_CORRELATION_ID)] Guid? correlationId
        ) =>
        {
            var email = GetUserEmail(user);
            await svc.ToggleProviderAsync(email, providerId);
            return Results.Ok();
        })
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status500InternalServerError);
    }

    private static void MapPostUpsertRating(RouteGroupBuilder thisGroup)
    {
        thisGroup.MapPost("upsert-rating", [Authorize] async (
            ClaimsPrincipal user,
            [FromServices] IKvService svc,
            [FromBody] UserRating userRating,
            [FromHeader(Name = Constants.X_CORRELATION_ID)] Guid? correlationId
        ) =>
        {
            var email = GetUserEmail(user);
            var returnValue = new Response<UserRating>
            {
                Data = await svc.UpsertRatingAsync(email, userRating)
            };
            return Results.Ok(returnValue);
        })
        .Produces<Response<UserRating>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status500InternalServerError);
    }
}
