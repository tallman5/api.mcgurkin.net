using McGurkin.Api.Features.Kpis.Data;
using McGurkin.ServiceProviders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace McGurkin.Api.Features.Kpis;

public static class KpiRoutes
{
    public static void MapKpiRoutes(this WebApplication app)
    {
        var group = app.MapGroup("kpis")
            .WithTags("KPIs");

        MapPostAddEpochs(group);
        MapDeleteKpi(group);
        MapGetKpis(group);
        MapGetKpisDevice(group);
        MapGetKpisDeviceKey(group);
        MapUpsertKpi(group);
    }

    private static void MapDeleteKpi(RouteGroupBuilder group)
    {
        group.MapDelete("{kpiId}", [Authorize] async (
            [FromServices] IKpiService svc,
            [FromRoute] Guid kpiId
        ) =>
        {
            await svc.DeleteKpiAsync(kpiId);
            return Results.Ok();
        })
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status500InternalServerError);
    }

    private static void MapGetKpis(RouteGroupBuilder group)
    {
        group.MapGet("", async (
            [FromServices] IKpiService svc
        ) =>
        {
            var returnValue = new Response<Kpi[]>
            {
                Data = await svc.GetKpisAsync()
            };
            return Results.Ok(returnValue);
        })
        .Produces<Response<Kpi[]>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status500InternalServerError);
    }

    private static void MapGetKpisDevice(RouteGroupBuilder group)
    {
        group.MapGet("{deviceName}", async (
            [FromServices] IKpiService svc,
            [FromRoute] string deviceName
        ) =>
        {
            var returnValue = new Response<Kpi[]>
            {
                Data = await svc.GetKpisByDeviceAsync(deviceName)
            };
            return Results.Ok(returnValue);
        })
        .Produces<Response<Kpi[]>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status500InternalServerError);
    }

    private static void MapGetKpisDeviceKey(RouteGroupBuilder group)
    {
        group.MapGet("{deviceName}/{keyName}", async (
            [FromServices] IKpiService svc,
            [FromRoute] string deviceName,
            [FromRoute] string keyName
        ) =>
        {
            var returnValue = new Response<Kpi[]>
            {
                Data = await svc.GetKpisByDeviceKeyAsync(deviceName, keyName)
            };
            return Results.Ok(returnValue);
        })
        .Produces<Response<Kpi[]>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status500InternalServerError);
    }

    private static void MapPostAddEpochs(RouteGroupBuilder group)
    {
        group.MapPost("epochs", [Authorize] async (
            [FromServices] IKpiService svc,
            [FromBody] EpochsRq epochsRq
        ) =>
        {
            await svc.AddEpochsAsync(epochsRq);
            return Results.Ok();
        })
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status500InternalServerError);
    }

    private static void MapUpsertKpi(RouteGroupBuilder group)
    {
        group.MapPost("", [Authorize] async (
            [FromServices] IKpiService svc,
            [FromBody] KpiEpoch kpiEpoch
        ) =>
        {
            await svc.UpsertKpiAsync(kpiEpoch.ToKpi());
            return Results.Ok();
        })
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status500InternalServerError);
    }
}
