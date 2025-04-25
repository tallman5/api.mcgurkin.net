using McGurkin.Api.Features.Tmdb.Data;
using McGurkin.Api.Features.Utilities;

namespace McGurkin.Api.Features.Tmdb;

public interface ITmdbService
{
    Task<Movie[]> DiscoverMoviesAsync(DiscoverRequest discoverRequest, Guid correlationId, string language);
    Task<Genre[]> GetGenresAsync(Guid correlationId, string language);
    Task<Movie> GetMovieAsync(int movieId, Guid correlationId, string language, bool includeDetails);
    Task<Dictionary<string, CountryWatchProvider>> GetMovieProvidersAsync(int id, Guid correlationId, string language, string region);
    Task<Person> GetPersonAsync(int personId, Guid correlationId, string language, bool includeDetails);
    Task<ProviderDetails[]> GetProvidersAsync(Guid correlationId, string language);
    Task<Person> GetRandomPersonAsync(Guid correlationId, string language);
    Task<Region[]> GetRegionsAsync(Guid correlationId, string language);

    Task<Movie[]> SearchMoviesAsync(string query, Guid correlationId, string language);
    Task<Person[]> SearchPeopleAsync(string query, Guid correlationId, string language);
    Task<SearchMultiResult> SearchMultiAsync(string query, Guid correlationId, string language);
}

public class TmdbService(IConfiguration configuration, IHttpClientFactory httpClientFactory) : ITmdbService
{
    private readonly TmdbServiceConfig _config = TmdbServiceConfig.FromConfiguration(configuration);
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();
    protected readonly static Random _random = new();

    public async Task<Movie[]> DiscoverMoviesAsync(DiscoverRequest discoverRequest, Guid correlationId, string language)
    {
        // Build the initial discover URL
        var extendedQuery = discoverRequest.GetQueryString() ?? "";
        var url = $"{_config.ApiUrl}/3/discover/movie?api_key={_config.ApiKey}&language={language}{extendedQuery}";

        // Fetch movie IDs (first API call)
        var rs = await HttpClientUtils.GetAsync<GetMoviesRs>(
            _httpClient,
            correlationId,
            url
        ).ConfigureAwait(false);

        // Early exit if no movies found
        if (rs.results == null || rs.results.Length == 0)
            return [];

        // Run all provider fetch tasks concurrently
        var region = HttpClientUtils.ExtractLocaleFromLanguageTag(language);
        var providerTasks = rs.results.Select(async movie =>
        {
            movie.providers = await GetMovieProvidersAsync(movie.id, correlationId, language, region).ConfigureAwait(false);
        }).ToList();

        await Task.WhenAll(providerTasks).ConfigureAwait(false);

        return rs.results;
    }

    public async Task<Genre[]> GetGenresAsync(Guid correlationId, string language)
    {
        var url = $"{_config.ApiUrl}/3/genre/movie/list?api_key={_config.ApiKey}&language={language}";
        var rs = await HttpClientUtils.GetAsync<GetGenresRs>(_httpClient, correlationId, url, 365);
        var returnValue = rs.genres;
        return returnValue;
    }

    public async Task<Movie> GetMovieAsync(int movieId, Guid correlationId, string language, bool includeDetails)
    {
        var region = HttpClientUtils.ExtractLocaleFromLanguageTag(language);
        var detailUrl = $"{_config.ApiUrl}/3/movie/{movieId}?api_key={_config.ApiKey}&language={language}";
        if (includeDetails == true) detailUrl += "&append_to_response=credits";
        var providerUrl = $"{_config.ApiUrl}/3/movie/{movieId}/watch/providers?api_key={_config.ApiKey}&language={language}&watch_region={region}";

        // Start both requests concurrently
        var movieTask = HttpClientUtils.GetAsync<Movie>(_httpClient, correlationId, detailUrl);
        var providersTask = HttpClientUtils.GetAsync<WatchProviderResponse>(_httpClient, correlationId, providerUrl);

        // Await both tasks (no deadlock risk since no .Result is used before this)
        await Task.WhenAll(movieTask, providersTask).ConfigureAwait(false);

        // Safely access results after awaiting
        var movie = await movieTask;
        var providersResponse = await providersTask;

        // Filter providers by region (case-insensitive)
        var filteredProviders = providersResponse.Results
            .Where(p => p.Key.Equals(region, StringComparison.OrdinalIgnoreCase))
            .ToDictionary(p => p.Key, p => p.Value);

        movie.providers = filteredProviders;
        return movie;
    }

    public async Task<Dictionary<string, CountryWatchProvider>> GetMovieProvidersAsync(int id, Guid correlationId, string language, string region)
    {
        var providerUrl = $"{_config.ApiUrl}/3/movie/{id}/watch/providers?api_key={_config.ApiKey}&language={language}&watch_region={region}";
        var watchProviderResponse = await HttpClientUtils.GetAsync<WatchProviderResponse>(_httpClient, correlationId, providerUrl);
        var returnValue = watchProviderResponse.Results
            .Where(p => p.Key.Equals(region, StringComparison.OrdinalIgnoreCase))
            .ToDictionary(p => p.Key, p => p.Value);
        return returnValue;
    }

    public async Task<Person> GetPersonAsync(int personId, Guid correlationId, string language, bool includeDetails)
    {
        var url = $"{_config.ApiUrl}/3/person/{personId}?api_key={_config.ApiKey}&language={language}";
        if (includeDetails == true)
            url += "&append_to_response=credits,images,tagged_images";
        var returnValue = await HttpClientUtils.GetAsync<Person>(_httpClient, correlationId, url).ConfigureAwait(false);
        return returnValue;
    }

    public async Task<ProviderDetails[]> GetProvidersAsync(Guid correlationId, string language)
    {
        var region = HttpClientUtils.ExtractLocaleFromLanguageTag(language);
        var url = $"{_config.ApiUrl}/3/watch/providers/movie?api_key={_config.ApiKey}&language={language}&watch_region={region}";
        var rs = await HttpClientUtils.GetAsync<GetProvidersRs>(_httpClient, correlationId, url, 365);
        var returnValue = rs.results;
        return returnValue;
    }

    public async Task<Person> GetRandomPersonAsync(Guid correlationId, string language)
    {
        var pageSize = 20;
        var personIndex = _random.Next(1, 500);
        var page = personIndex / pageSize + 1;
        var personPageIndex = personIndex - page * pageSize + pageSize;
        var url = $"{_config.ApiUrl}/3/person/popular?api_key={_config.ApiKey}&language={language}&page={page}";
        var rs = await HttpClientUtils.GetAsync<GetPeopleRs>(_httpClient, correlationId, url).ConfigureAwait(false);
        var returnValue = rs.results[personPageIndex];
        return returnValue;
    }

    public async Task<Region[]> GetRegionsAsync(Guid correlationId, string language)
    {
        var url = $"{_config.ApiUrl}/3/watch/providers/regions?api_key={_config.ApiKey}&language={language}";
        var rs = await HttpClientUtils.GetAsync<GetRegionsRs>(_httpClient, correlationId, url, 365);
        var returnValue = rs.results;
        return returnValue;
    }

    public async Task<Movie[]> SearchMoviesAsync(string query, Guid correlationId, string language)
    {
        var url = $"{_config.ApiUrl}/3/search/movie?api_key={_config.ApiKey}&language={language}&query={query}";
        var rs = await HttpClientUtils.GetAsync<GetMoviesRs>(_httpClient, correlationId, url).ConfigureAwait(false);
        return rs.results;
    }

    public async Task<Person[]> SearchPeopleAsync(string query, Guid correlationId, string language)
    {
        var url = $"{_config.ApiUrl}/3/search/person?api_key={_config.ApiKey}&language={language}&query={query}";
        var rs = await HttpClientUtils.GetAsync<GetPeopleRs>(_httpClient, correlationId, url).ConfigureAwait(false);
        return rs.results;
    }

    public async Task<SearchMultiResult> SearchMultiAsync(string query, Guid correlationId, string language)
    {
        var moviesTask = SearchMoviesAsync(query, correlationId, language);
        var peopleTask = SearchPeopleAsync(query, correlationId, language);

        await Task.WhenAll(moviesTask, peopleTask).ConfigureAwait(false);

        return new SearchMultiResult
        {
            Movies = await moviesTask,
            People = await peopleTask
        };
    }
}
