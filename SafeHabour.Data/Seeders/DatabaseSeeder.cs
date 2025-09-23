using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SafeHabour.Data.Data;
using SafeHabour.Data.Entities;

namespace SafeHabour.Data.Seeders;

/// <summary>
/// Master seeder class that orchestrates the seeding process for all entities
/// </summary>
public static class DatabaseSeeder
{
    /// <summary>
    /// Seeds all required data for the SafeHabour application
    /// </summary>
    /// <param name="serviceProvider">Service provider for dependency injection</param>
    /// <param name="logger">Logger instance</param>
    public static async Task SeedAllAsync(IServiceProvider serviceProvider, ILogger logger)
    {
        logger.LogInformation("Starting database seeding process...");

        try
        {
            using var scope = serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();

            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Seed in the correct order to respect dependencies

            var roleExists = await context.Roles.AnyAsync();
            if (!roleExists)
            {
                // 1. Seed roles first (required for user creation)
                await RoleSeeder.SeedRolesAsync(roleManager, logger);
            }

            var userExists = await context.Users.AnyAsync();

            if (userExists)
            {
                logger.LogInformation("Users already exist in the database. Skipping user seeding.");
                return;
            }
            
            // 2. Seed client users
            await ClientUserSeeder.SeedClientUsersAsync(userManager, context, logger);
            
            // 3. Seed service worker users
            await ServiceWorkerSeeder.SeedServiceWorkersAsync(userManager, context, logger);
            
            // 4. Seed super admin users
            await SuperAdminSeeder.SeedSuperAdminsAsync(userManager, context, logger);

            logger.LogInformation("Database seeding process completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during the database seeding process.");
            throw;
        }
    }

    /// <summary>
    /// Seeds only roles (useful for production environments)
    /// </summary>
    /// <param name="serviceProvider">Service provider for dependency injection</param>
    /// <param name="logger">Logger instance</param>
    public static async Task SeedRolesOnlyAsync(IServiceProvider serviceProvider, ILogger logger)
    {
        logger.LogInformation("Starting roles-only seeding process...");

        try
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            await RoleSeeder.SeedRolesAsync(roleManager, logger);

            logger.LogInformation("Roles seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during roles seeding.");
            throw;
        }
    }
}
