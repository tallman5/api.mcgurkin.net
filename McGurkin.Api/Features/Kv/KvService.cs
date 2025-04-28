using McGurkin.Api.Features.Kv.Data;
using Microsoft.EntityFrameworkCore;

namespace McGurkin.Api.Features.Kv;

public interface IKvService
{
    Task DeleteUserRatingAsync(string email, Guid userRatingId);
    Task<UserProfile> GetOrCreateMyProfileAsync(string email);
    Task ToggleProviderAsync(string email, int providerId);
    Task<UserRating> UpsertUserRatingAsync(string email, UserRating userRating);
}

public class KvService(KvDbContext kvDbContext) : IKvService
{
    private readonly KvDbContext _kvDbContext = kvDbContext;

    public async Task DeleteUserRatingAsync(string email, Guid userRatingId)
    {
        var userProfile = await GetOrCreateMyProfileAsync(email);
        var userRating = userProfile.UserRatings
            .FirstOrDefault(x => x.UserRatingId == userRatingId);

        if (userRating != null)
        {
            _kvDbContext.UserRatings.Remove(userRating);
            await _kvDbContext.SaveChangesAsync().ConfigureAwait(false);
        }
    }

    public async Task<UserProfile> GetOrCreateMyProfileAsync(string email)
    {
        var returnValue = await _kvDbContext.UserProfiles
            .Where(x => x.UserEmail == email)
            .Include(x => x.UserProviders)
            .Include(x => x.UserRatings)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (returnValue == null)
        {
            returnValue = new UserProfile
            {
                UserEmail = email
            };
            _kvDbContext.UserProfiles.Add(returnValue);
            await _kvDbContext.SaveChangesAsync().ConfigureAwait(false);
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
            await _kvDbContext.UserProviders.AddAsync(provider).ConfigureAwait(false);
        }

        await _kvDbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task<UserRating> UpsertUserRatingAsync(string email, UserRating userRating)
    {
        var userProfile = await GetOrCreateMyProfileAsync(email);
        if (userRating.UserRatingId == Guid.Empty)
            userRating.UserRatingId = Guid.NewGuid();
        userRating.UserProfileId = userProfile.UserProfileId;

        var existingRating = _kvDbContext.UserRatings
            .Where(ur => ur.UserRatingId == userRating.UserRatingId)
            .AsNoTracking()
            .FirstOrDefault();

        if (existingRating == null)
        {
            _kvDbContext.UserRatings.Add(userRating);
        }
        else
        {
            if (existingRating.UserProfileId != userProfile.UserProfileId)
                throw new InvalidOperationException("User rating does not belong to the current user.");
            _kvDbContext.UserRatings.Update(userRating);
        }

        await _kvDbContext.SaveChangesAsync();
        return userRating;
    }
}
