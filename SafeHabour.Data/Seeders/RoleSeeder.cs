using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SafeHabour.Models.Enums;

namespace SafeHabour.Data.Seeders;

public static class RoleSeeder
{
    public static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager, ILogger logger)
    {
        logger.LogInformation("Starting role seeding process...");

        var roles = new[]
        {
            UserType.SuperAdmin,
            UserType.ClientUser,
            UserType.ServiceWorker
        };

        foreach (var roleName in roles)
        {
            try
            {
                var roleExists = await roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    var role = new IdentityRole<Guid>
                    {
                        Id = Guid.NewGuid(),
                        Name = roleName,
                        NormalizedName = roleName.ToUpper(),
                        ConcurrencyStamp = Guid.NewGuid().ToString()
                    };

                    var result = await roleManager.CreateAsync(role);
                    if (result.Succeeded)
                    {
                        logger.LogInformation("Successfully created role: {RoleName}", roleName);
                    }
                    else
                    {
                        logger.LogError("Failed to create role {RoleName}. Errors: {Errors}", 
                            roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    logger.LogInformation("Role {RoleName} already exists, skipping...", roleName);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception occurred while creating role: {RoleName}", roleName);
            }
        }

        logger.LogInformation("Role seeding process completed.");
    }
}
