using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace McGurkin.Api.Features.Iam.Data;

public partial class IamDbContext(DbContextOptions<IamDbContext> options) : IdentityDbContext<IdentityUser>(options)
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("IamServiceConfig:DbConnectionString",
                opt => opt.MigrationsHistoryTable(IamServiceConfig.HISTORY_TABLE, IamServiceConfig.SCHEMA)
            );
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema(IamServiceConfig.SCHEMA);
    }

    public static async Task SeedAsync(IamDbContext context, UserManager<IdentityUser> userManager, IConfiguration configuration)
    {
        if (!context.Users.Any())
        {
            var seedUsers = configuration.GetSection("SeedData:SeedUsers").Get<List<SeedUser>>();
            if (seedUsers == null || seedUsers.Count == 0)
                return;
            foreach (var seedUser in seedUsers)
            {
                var existingUser = await userManager.FindByEmailAsync(seedUser.Email);
                if (existingUser == null)
                {
                    var newUser = new IdentityUser
                    {
                        UserName = seedUser.UserName,
                        Email = seedUser.Email,
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(newUser, seedUser.Password);

                    if (!result.Succeeded)
                    {
                        var errors = string.Join(", ", result.Errors);
                        throw new Exception($"Failed to create user {seedUser.Email}: {errors}");
                    }
                }
            }
            await context.SaveChangesAsync();
        }
    }
}
