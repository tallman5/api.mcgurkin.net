using McGurkin.Api.Features.Iam;
using McGurkin.Api.Features.Iam.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace McGurkin.Api.Test;

[TestClass]
public class DataSeeding
{
    protected readonly IamDbContext _context;
    protected readonly IConfiguration _configuration;

    public DataSeeding()
    {
        _configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddUserSecrets<IIamService>()
            .Build();

        var options = new DbContextOptionsBuilder<IamDbContext>()
            .UseSqlServer(_configuration["IamServiceConfig:DbConnectionString"])
            .Options;

        _context = new IamDbContext(options);
    }

    [TestMethod]
    public void SeedDatabase()
    {
        if (_context.Users.Any())
            return;

        var roles = _configuration.GetSection("SeedData:Roles").Get<List<IdentityRole>>();
        if (null != roles && roles.Count > 0)
            _context.Roles.AddRange(roles);

        var users = _configuration.GetSection("SeedData:Users").Get<List<IamUser>>();
        if (null != users && users.Count > 0)
            _context.Users.AddRange(users);

        _context.SaveChanges();

        var user = _context.Users.Where(u => u.ScreenName == "tallman").FirstOrDefault();
        var role = _context.Roles.FirstOrDefault();
        if (user != null && role != null)
        {
            var userRole = new IdentityUserRole<string>
            {
                UserId = user.Id,
                RoleId = role.Id
            };
            _context.UserRoles.Add(userRole);
        }

        _context.SaveChanges();

        var userList = _context.Users.ToList();
        Assert.IsNotNull(userList);
        Assert.IsTrue(userList.Count > 0);
    }
}
