using McGurkin.Runtime.Caching;
using McGurkin.Runtime.Serialization;
using System.Globalization;

namespace McGurkin.Api.Features.Utilities;

public class HttpClientUtils
{
    public static string ExtractLocaleFromLanguageTag(string lang, string fallback = "US")
    {
        if (string.IsNullOrWhiteSpace(lang))
            return fallback;

        // Try splitting the language tag (e.g., "en-US" → ["en", "US"])
        var parts = lang.Split('-', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2 && parts[1].Length == 2)
            return parts[1].ToUpperInvariant();

        // Try CultureInfo + RegionInfo if region not explicitly present
        try
        {
            var culture = CultureInfo.GetCultureInfo(lang);
            var region = new RegionInfo(culture.Name);
            return region.TwoLetterISORegionName;
        }
        catch
        {
            return fallback;
        }
    }

    public static string ExtractPrimaryLanguage(string? lang, string? acceptLanguage, string fallback = "en-US")
    {
        var raw = lang ?? acceptLanguage ?? fallback;

        // Accept-Language: en-US,en;q=0.9,de;q=0.8
        var firstPart = raw.Split(',').FirstOrDefault()?.Split(';').FirstOrDefault()?.Trim();

        try
        {
            return CultureInfo.GetCultureInfo(firstPart ?? fallback).Name;
        }
        catch (CultureNotFoundException)
        {
            return fallback;
        }
    }

    public static async Task<T> GetAsync<T>(HttpClient httpClient, Guid correlationId, string url, int expireDays = 7)
    {
        var cacheKey = CacheHelper.GetSafeCacheKeyFromUrl(url);
        var returnValue = CacheHelper.Get<T>(cacheKey);

        if (null == returnValue)
        {
            httpClient.DefaultRequestHeaders.Remove(Constants.X_CORRELATION_ID);
            httpClient.DefaultRequestHeaders.Add(Constants.X_CORRELATION_ID, correlationId.ToString());
            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            returnValue = Serializer.FromString<T>(json);
            if (null == returnValue) throw new Exception($"Could not deserialize response from {url} to {typeof(T).Name}");
            CacheHelper.Set(cacheKey, returnValue, DateTimeOffset.Now.AddDays(expireDays));
        }

        return returnValue;
    }
}
