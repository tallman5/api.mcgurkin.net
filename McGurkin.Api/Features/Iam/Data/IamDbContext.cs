using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace McGurkin.Api.Features.Iam.Data;

public partial class IamDbContext(DbContextOptions<IamDbContext> options) : IdentityDbContext<IamUser>(options)
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
        builder.Entity<IamUser>()
            .HasIndex(p => new { p.ScreenName })
            .IsUnique(true);
    }

    public static async Task SeedAsync(IamDbContext context, UserManager<IamUser> userManager, IConfiguration configuration)
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
                    var newUser = new IamUser
                    {
                        ScreenName = seedUser.ScreenName,
                        UserName = seedUser.Email,
                        Email = seedUser.Email,
                        EmailConfirmed = true // Important: Set email as confirmed
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
