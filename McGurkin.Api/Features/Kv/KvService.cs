using McGurkin.Api.Features.Kv.Data;
using Microsoft.EntityFrameworkCore;

namespace McGurkin.Api.Features.Kv;

public interface IKvService
{
    Task<UserProfile> GetOrCreateMyProfileAsync(string email);
    Task ToggleProviderAsync(string email, int providerId);
    Task<UserRating> UpsertRatingAsync(string email, UserRating userRating);
}

public class KvService(KvDbContext kvDbContext) : IKvService
{
    private readonly KvDbContext _kvDbContext = kvDbContext;

    public async Task<UserProfile> GetOrCreateMyProfileAsync(string email)
    {
        var returnValue = await _kvDbContext.UserProfiles
            .Where(x => x.UserEmail == email)
            .AsNoTracking()
            .Include(x => x.UserProviders)
            .Include(x => x.UserRatings)
            .FirstOrDefaultAsync();

        if (returnValue == null)
        {
            returnValue = new UserProfile
            {
                UserEmail = email
            };
            _kvDbContext.UserProfiles.Add(returnValue);
            await _kvDbContext.SaveChangesAsync();
        }

        return returnValue;
    }

    public async Task ToggleProviderAsync(string email, int providerId)
    {
        var profile = await GetOrCreateMyProfileAsync(email);
        var provider = profile.UserProviders?.FirstOrDefault(x => x.ProviderId == providerId);

        if (provider != null)
        {
            _kvDbContext.UserProviders.Remove(provider);
        }
        else
        {
            provider = new UserProvider
            {
                UserProfileId = profile.UserProfileId,
                ProviderId = providerId
            };
            await _kvDbContext.UserProviders.AddAsync(provider);
        }

        await _kvDbContext.SaveChangesAsync();
    }

    public async Task<UserRating> UpsertRatingAsync(string email, UserRating userRating)
    {
        var profile = await GetOrCreateMyProfileAsync(email);

        var updatedRating = profile.UserRatings?.FirstOrDefault(x => x.Id == userRating.Id);
        if (updatedRating != null)
        {
            if (null == profile.UserRatings)
                profile.UserRatings = [];
            profile.UserRatings.Add(userRating);
        }
        else
        {
            updatedRating = new UserRating
            {
                InWatchlist = userRating.InWatchlist,
                IsHidden = userRating.IsHidden,
                MovieId = userRating.MovieId,
                Stars = userRating.Stars,
                TvId = userRating.TvId
            };
        }
        await _kvDbContext.SaveChangesAsync();
        return updatedRating;
    }
}
