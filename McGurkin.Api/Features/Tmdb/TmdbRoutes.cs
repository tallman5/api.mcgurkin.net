using McGurkin.Api.Features.Tmdb.Data;
using McGurkin.Api.Features.Utilities;
using McGurkin.ServiceProviders;
using Microsoft.AspNetCore.Mvc;

namespace McGurkin.Api.Features.Tmdb;

public static class TmdbRoutes
{
    public static void MapTmdbRoutes(this WebApplication app)
    {
        var thisGroup = app.MapGroup("tmdb")
            .WithTags("The Movie Database");

        MapGetGenres(thisGroup);
        MapGetPerson(thisGroup);
        MapGetProviders(thisGroup);
        MapGetMovie(thisGroup);
        MapGetMovieProviders(thisGroup);
        MapGetRegions(thisGroup);

        MapPostDiscoverMovies(thisGroup);
        MapPostSearch(thisGroup);
    }

    private static void MapGetGenres(RouteGroupBuilder thisGroup)
    {
        thisGroup.MapGet("genres", async (
            [FromServices] ITmdbService svc,
            [FromHeader(Name = "Accept-Language")] string? acceptLanguage,
            [FromQuery] string? lang,
            [FromHeader(Name = Constants.X_CORRELATION_ID)] Guid? correlationId
        ) =>
        {
            var language = HttpClientUtils.ExtractPrimaryLanguage(lang, acceptLanguage);
            var returnValue = new Response<Genre[]>
            {
                Data = await svc.GetGenresAsync(correlationId ?? Guid.NewGuid(), language)
            };
            return Results.Ok(returnValue);
        })
        .Produces<Response<Genre[]>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status500InternalServerError);
    }

    private static void MapGetMovie(RouteGroupBuilder thisGroup)
    {
        thisGroup.MapGet("movies/{movieId}", async (
            [FromServices] ITmdbService svc,
            [FromRoute] int movieId,
            [FromHeader(Name = "Accept-Language")] string? acceptLanguage,
            [FromQuery] string? lang,
            [FromHeader(Name = Constants.X_CORRELATION_ID)] Guid? correlationId
        ) =>
        {
            try
            {
                var language = HttpClientUtils.ExtractPrimaryLanguage(lang, acceptLanguage);
                var returnValue = new Response<Movie>
                {
                    Data = await svc.GetMovieAsync(movieId, correlationId ?? Guid.NewGuid(), language, true)
                };
                return Results.Ok(returnValue);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return Results.NotFound();
            }
            catch (Exception)
            {
                return Results.Problem("An error occurred while processing your request.", statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .Produces<Response<Movie>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError);
    }

    private static void MapGetMovieProviders(RouteGroupBuilder thisGroup)
    {
        thisGroup.MapGet("movies/{movieId}/providers", async (
            [FromServices] ITmdbService svc,
            [FromRoute] int movieId,
            [FromHeader(Name = "Accept-Language")] string? acceptLanguage,
            [FromQuery] string? lang,
            [FromHeader(Name = Constants.X_CORRELATION_ID)] Guid? correlationId
        ) =>
        {
            try
            {
                var language = HttpClientUtils.ExtractPrimaryLanguage(lang, acceptLanguage);
                var returnValue = new Response<Dictionary<string, CountryWatchProvider>>
                {
                    Data = await svc.GetMovieProvidersAsync(movieId, correlationId ?? Guid.NewGuid(), language, HttpClientUtils.ExtractLocaleFromLanguageTag(language))
                };
                return Results.Ok(returnValue);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return Results.NotFound();
            }
            catch (Exception)
            {
                return Results.Problem("An error occurred while processing your request.", statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .Produces<Response<Movie>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError);
    }

    private static void MapGetPerson(RouteGroupBuilder thisGroup)
    {
        thisGroup.MapGet("people/{personId}", async (
            [FromServices] ITmdbService svc,
            [FromRoute] int personId,
            [FromHeader(Name = "Accept-Language")] string? acceptLanguage,
            [FromQuery] string? lang,
            [FromHeader(Name = Constants.X_CORRELATION_ID)] Guid? correlationId
        ) =>
        {
            try
            {
                var language = HttpClientUtils.ExtractPrimaryLanguage(lang, acceptLanguage);
                var returnValue = new Response<Person>
                {
                    Data = await svc.GetPersonAsync(personId, correlationId ?? Guid.NewGuid(), language, true)
                };
                return Results.Ok(returnValue);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return Results.NotFound();
            }
            catch (Exception)
            {
                return Results.Problem("An error occurred while processing your request.", statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .Produces<Response<Person>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError);
    }

    private static void MapGetProviders(RouteGroupBuilder thisGroup)
    {
        thisGroup.MapGet("providers", async (
            [FromServices] ITmdbService svc,
            [FromHeader(Name = "Accept-Language")] string? acceptLanguage,
            [FromQuery] string? lang,
            [FromHeader(Name = Constants.X_CORRELATION_ID)] Guid? correlationId
        ) =>
        {
            var language = HttpClientUtils.ExtractPrimaryLanguage(lang, acceptLanguage);
            var returnValue = new Response<ProviderDetails[]>
            {
                Data = await svc.GetProvidersAsync(correlationId ?? Guid.NewGuid(), language)
            };
            return Results.Ok(returnValue);
        })
        .Produces<Response<ProviderDetails[]>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status500InternalServerError);
    }

    private static void MapGetRegions(RouteGroupBuilder thisGroup)
    {
        thisGroup.MapGet("regions", async (
            [FromServices] ITmdbService svc,
            [FromHeader(Name = "Accept-Language")] string? acceptLanguage,
            [FromQuery] string? lang,
            [FromHeader(Name = Constants.X_CORRELATION_ID)] Guid? correlationId
        ) =>
        {
            var language = HttpClientUtils.ExtractPrimaryLanguage(lang, acceptLanguage);
            var returnValue = new Response<Region[]>
            {
                Data = await svc.GetRegionsAsync(correlationId ?? Guid.NewGuid(), language)
            };
            return Results.Ok(returnValue);
        })
        .Produces<Response<Region[]>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status500InternalServerError);
    }

    private static void MapPostDiscoverMovies(RouteGroupBuilder thisGroup)
    {
        thisGroup.MapPost("movies/discover", async (
            [FromServices] ITmdbService svc,
            [FromBody] DiscoverRequest discoverRequest,
            [FromHeader(Name = "Accept-Language")] string? acceptLanguage,
            [FromQuery] string? lang,
            [FromHeader(Name = Constants.X_CORRELATION_ID)] Guid? correlationId
        ) =>
        {
            var language = HttpClientUtils.ExtractPrimaryLanguage(lang, acceptLanguage);
            var returnValue = new Response<Movie[]>
            {
                Data = await svc.DiscoverMoviesAsync(discoverRequest, correlationId ?? Guid.NewGuid(), language)
            };
            return Results.Ok(returnValue);
        })
        .Produces<Response<Genre[]>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status500InternalServerError);
    }

    private static void MapPostSearch(RouteGroupBuilder thisGroup)
    {
        thisGroup.MapPost("search", async (
            [FromServices] ITmdbService svc,
            [FromBody] string query,
            [FromHeader(Name = "Accept-Language")] string? acceptLanguage,
            [FromQuery] string? lang,
            [FromHeader(Name = Constants.X_CORRELATION_ID)] Guid? correlationId
        ) =>
        {
            var language = HttpClientUtils.ExtractPrimaryLanguage(lang, acceptLanguage);
            var returnValue = new Response<SearchMultiResult>
            {
                Data = await svc.SearchMultiAsync(query, correlationId ?? Guid.NewGuid(), language)
            };
            return Results.Ok(returnValue);
        })
        .Produces<Response<SearchMultiResult>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status500InternalServerError);
    }
}
