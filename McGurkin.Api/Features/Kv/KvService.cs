using McGurkin.Api.Features.Kv.Data;
using McGurkin.Api.Features.Tmdb.Data;
using Microsoft.EntityFrameworkCore;

namespace McGurkin.Api.Features.Kv;

public interface IKvService
{
    Task<UserProfile> GetMyProfileAsync(string email);
}

public class KvService(KvDbContext kvDbContext, ILogger<KvService> logger) : IKvService
{
    private readonly KvDbContext _kvDbContext = kvDbContext;
    private readonly ILogger<KvService> _logger = logger;

    public async Task<UserProfile> GetMyProfileAsync(string email)
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
}
