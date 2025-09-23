using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SafeHabour.Data.Entities;
using SafeHabour.Data.Data;
using SafeHabour.Models.Enums;

namespace SafeHabour.Data.Seeders;

public static class ClientUserSeeder
{
    public static async Task SeedClientUsersAsync(
        UserManager<User> userManager, 
        ApiDbContext context,
        ILogger logger)
    {
        logger.LogInformation("Starting client user seeding process...");

        var clientUsers = new[]
        {
            new
            {
                Email = "Superkingsely@gmail.com",
                FirstName = "Super King",
                LastName = "Sely",
                PhoneNumber = "+1234567890",
                Bio = "Professional client looking for reliable service workers.",
                City = "Lagos",
                Country = "Nigeria",
                StreetAddress = "123 Victoria Island",
                PostalCode = "100001",
                Latitude = 6.4281,
                Longitude = 3.4219,
                ClientType = 1
            },
            new
            {
                Email = "Ayodejishoga1@gmail.com",
                FirstName = "Ayodeji",
                LastName = "Shoga",
                PhoneNumber = "+1234567891",
                Bio = "Family-oriented client seeking trusted household services.",
                City = "Abuja",
                Country = "Nigeria",
                StreetAddress = "456 Garki District",
                PostalCode = "900001",
                Latitude = 9.0765,
                Longitude = 7.3986,
                ClientType = 1
            }
        };

        foreach (var clientData in clientUsers)
        {
            try
            {
                // Check if user already exists
                var existingUser = await userManager.FindByEmailAsync(clientData.Email);
                if (existingUser != null)
                {
                    logger.LogInformation("Client user with email {Email} already exists, skipping...", clientData.Email);
                    continue;
                }

                // Create the user
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    UserName = clientData.Email,
                    Email = clientData.Email,
                    NormalizedUserName = clientData.Email.ToUpper(),
                    NormalizedEmail = clientData.Email.ToUpper(),
                    EmailConfirmed = true,
                    PhoneNumber = clientData.PhoneNumber,
                    PhoneNumberConfirmed = true,
                    FirstName = clientData.FirstName,
                    LastName = clientData.LastName,
                    Bio = clientData.Bio,
                    StreetAddress = clientData.StreetAddress,
                    City = clientData.City,
                    Country = clientData.Country,
                    PostalCode = clientData.PostalCode,
                    Latitude = clientData.Latitude,
                    Longitude = clientData.Longitude,
                    IsActive = true,
                    IsVerified = true,
                    IsProfileComplete = true,
                    IsProfileApproved = true,
                    CreatedAt = DateTime.UtcNow,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };

                // Create user with default password
                var result = await userManager.CreateAsync(user, "ClientUser123!");
                if (result.Succeeded)
                {
                    // Assign ClientUser role
                    var roleResult = await userManager.AddToRoleAsync(user, UserType.ClientUser);
                    if (roleResult.Succeeded)
                    {
                        logger.LogInformation("Successfully assigned ClientUser role to: {Email}", clientData.Email);
                    }
                    else
                    {
                        logger.LogError("Failed to assign ClientUser role to {Email}. Errors: {Errors}",
                            clientData.Email, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                    }

                    // Create ClientUser profile
                    var clientProfile = new ClientUser
                    {
                        Id = 0, // Auto-generated
                        UserId = user.Id,
                        ClientType = clientData.ClientType,
                        CreatedAt = DateTime.UtcNow
                    };

                    context.ClientUsers.Add(clientProfile);
                    await context.SaveChangesAsync();

                    logger.LogInformation("Successfully created client user and profile for: {Email}", clientData.Email);
                }
                else
                {
                    logger.LogError("Failed to create client user {Email}. Errors: {Errors}",
                        clientData.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception occurred while creating client user: {Email}", clientData.Email);
            }
        }

        logger.LogInformation("Client user seeding process completed.");
    }
}
