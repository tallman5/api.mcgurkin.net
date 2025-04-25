using McGurkin.Runtime.Serialization;
using McGurkin.ServiceProviders;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace McGurkin.Api.Features.Utilities;

public static class UtilitiesRoutes
{
    public static void MapApiHealthRoutes(this WebApplication app)
    {
        app.MapHealthChecks("/alive", new HealthCheckOptions
        {
            Predicate = (HealthCheckRegistration r) => r.Tags.Contains("live")
        });

        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = async (HttpContext context, HealthReport report) =>
            {
                context.Response.ContentType = "application/json";
                var config = context.RequestServices.GetRequiredService<IConfiguration>();
                string text = Serializer.ToString(new
                {
                    env = config["EnvName"],
                    status = report.Status.ToString(),
                    checks = report.Entries.Select(entry => new
                    {
                        name = entry.Key,
                        status = entry.Value.Status.ToString(),
                        description = entry.Value.Description ?? string.Empty,
                        duration = entry.Value.Duration.TotalMilliseconds + "ms"
                    })
                });
                await context.Response.WriteAsync(text);
            }
        });
    }

    public static void MapUtilitiesRoutes(this WebApplication app)
    {
        var thisGroup = app.MapGroup("utils")
            .WithTags("Utilities");
        MapGetGuids(thisGroup);
        MapGetWait(thisGroup);
    }

    private static void MapGetGuids(RouteGroupBuilder thisGroup)
    {
        thisGroup.MapGet("guids", (
            [FromHeader(Name = Constants.X_CORRELATION_ID)] Guid? correleationId
        ) =>
        {
            var returnValue = new Response<Guid[]>
            {
                Data = Enumerable.Range(0, 20).Select(_ => Guid.NewGuid()).ToArray()
            };
            return Results.Ok(returnValue);
        })
        .Produces<Response<Guid[]>>(StatusCodes.Status200OK);
    }

    private static void MapGetWait(RouteGroupBuilder thisGroup)
    {
        thisGroup.MapGet("wait-for/{milliseconds}", (
            int milliseconds,
            [FromHeader(Name = Constants.X_CORRELATION_ID)] Guid? correleationId
        ) =>
        {
            Thread.Sleep(milliseconds);
            return Results.Ok(new Response());
        })
        .Produces<Response>(StatusCodes.Status200OK);
    }
}
