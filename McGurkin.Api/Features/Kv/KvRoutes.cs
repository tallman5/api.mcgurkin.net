using McGurkin.Api.Features.Tmdb.Data;
using McGurkin.ServiceProviders;
using Microsoft.AspNetCore.Mvc;

namespace McGurkin.Api.Features.Kv;

public static class KvRoutes
{
    public static void MapKvRoutes(this WebApplication app)
    {
        var thisGroup = app.MapGroup("kv")
            .WithTags("Profile");

        MapGetMyProfile(thisGroup);
    }

    private static void MapGetMyProfile(RouteGroupBuilder thisGroup)
    {
        thisGroup.MapGet("my-profile", async (
            [FromServices] IKvService svc,
            [FromHeader(Name = Constants.X_CORRELATION_ID)] Guid? correlationId
        ) =>
        {
            var returnValue = new Response<UserProfile>
            {
                Data = await svc.GetMyProfileAsync("tester@kixvu.com")
            };
            return Results.Ok(returnValue);
        })
        .Produces<Response<UserProfile>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status500InternalServerError);
    }
}
