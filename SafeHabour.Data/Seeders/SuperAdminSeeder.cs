using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SafeHabour.Data.Entities;
using SafeHabour.Data.Data;
using SafeHabour.Models.Enums;

namespace SafeHabour.Data.Seeders;

public static class SuperAdminSeeder
{
    public static async Task SeedSuperAdminsAsync(
        UserManager<User> userManager,
        ApiDbContext context,
        ILogger logger)
    {
        logger.LogInformation("Starting super admin seeding process...");

        var superAdmins = new[]
        {
            new
            {
                Email = "adekniyi@gmail.com",
                FirstName = "Adekunle",
                LastName = "Adeniyi",
                PhoneNumber = "++23409070970079"
            },
            new
            {
                Email = "stanleyinegben@gmail.com",
                FirstName = "Stanley",
                LastName = "Inegben",
                PhoneNumber = "+2348222222222"
            }
        };

        foreach (var superAdminData in superAdmins)
        {
            // Check if user already exists
            var existingUser = await userManager.FindByEmailAsync(superAdminData.Email);
            if (existingUser != null)
            {
                logger.LogInformation("Super admin user {Email} already exists, skipping...", superAdminData.Email);
                continue;
            }

            // Create new user
            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = superAdminData.Email,
                Email = superAdminData.Email,
                FirstName = superAdminData.FirstName,
                LastName = superAdminData.LastName,
                PhoneNumber = superAdminData.PhoneNumber,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Create user with default password
            var result = await userManager.CreateAsync(user, "SuperAdmin123!");

            if (result.Succeeded)
            {
                // Assign SuperAdmin role
                await userManager.AddToRoleAsync(user, UserType.SuperAdmin);

                // Create SuperAdmin profile
                var superAdminProfile = new SuperAdmin
                {
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow
                };

                context.SuperAdmins.Add(superAdminProfile);
                await context.SaveChangesAsync();

                logger.LogInformation("Successfully created super admin user: {Email}", superAdminData.Email);
            }
            else
            {
                logger.LogError("Failed to create super admin user {Email}. Errors: {Errors}",
                    superAdminData.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        logger.LogInformation("Super admin seeding process completed.");
    }
}
