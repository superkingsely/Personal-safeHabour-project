using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SafeHabour.Data.Entities;
using SafeHabour.Data.Data;
using SafeHabour.Models.Requests;
using SafeHabour.Models.Enums;
using System.Text.Json;

namespace SafeHabour.Data.Seeders;

public static class ServiceWorkerSeeder
{
    public static async Task SeedServiceWorkersAsync(
        UserManager<User> userManager, 
        ApiDbContext context,
        ILogger logger)
    {
        logger.LogInformation("Starting service worker seeding process...");

        var serviceWorkers = new[]
        {
            new
            {
                Email = "oreoluwa.ibikunle1@gmail.com",
                FirstName = "Oreoluwa",
                LastName = "Ibikunle",
                PhoneNumber = "+2348123456789",
                Bio = "Professional house cleaner with 5+ years of experience. Specializing in deep cleaning and organization.",
                City = "Lagos",
                Country = "Nigeria",
                StreetAddress = "789 Ikeja Road",
                PostalCode = "100001",
                Latitude = 6.5244,
                Longitude = 3.3792,
                HourlyRate = 2500.00m,
                DateOfBirth = new DateTime(1990, 5, 15),
                Services = new List<ServiceItem>
                {
                    new() { Name = "House Cleaning", Description = "Professional residential cleaning", Category = "Cleaning", IsActive = true },
                    new() { Name = "Deep Cleaning", Description = "Comprehensive deep cleaning service", Category = "Cleaning", IsActive = true },
                    new() { Name = "Window Cleaning", Description = "Professional window cleaning", Category = "Cleaning", IsActive = true }
                },
                Languages = new List<LanguageItem>
                {
                    new() { Name = "English", Code = "en", ProficiencyLevel = "Native", IsNative = true },
                    new() { Name = "Yoruba", Code = "yo", ProficiencyLevel = "Native", IsNative = true }
                }
            },
            new
            {
                Email = "adekdebby67@gmail.com",
                FirstName = "Adebayo",
                LastName = "Deborah",
                PhoneNumber = "+2348987654321",
                Bio = "Experienced babysitter and tutor. Great with children and passionate about education.",
                City = "Abuja",
                Country = "Nigeria",
                StreetAddress = "321 Wuse District",
                PostalCode = "900001",
                Latitude = 9.0579,
                Longitude = 7.4951,
                HourlyRate = 3000.00m,
                DateOfBirth = new DateTime(1988, 8, 22),
                Services = new List<ServiceItem>
                {
                    new() { Name = "Babysitting", Description = "Professional child care services", Category = "Childcare", IsActive = true },
                    new() { Name = "Tutoring", Description = "Educational tutoring services", Category = "Education", IsActive = true },
                    new() { Name = "Pet Care", Description = "Professional pet care and walking", Category = "Pet Care", IsActive = true }
                },
                Languages = new List<LanguageItem>
                {
                    new() { Name = "English", Code = "en", ProficiencyLevel = "Native", IsNative = true },
                    new() { Name = "Hausa", Code = "ha", ProficiencyLevel = "Fluent", IsNative = false },
                    new() { Name = "French", Code = "fr", ProficiencyLevel = "Intermediate", IsNative = false }
                }
            }
        };

        foreach (var serviceWorkerData in serviceWorkers)
        {
            try
            {
                // Check if user already exists
                var existingUser = await userManager.FindByEmailAsync(serviceWorkerData.Email);
                if (existingUser != null)
                {
                    logger.LogInformation("Service worker with email {Email} already exists, skipping...", serviceWorkerData.Email);
                    continue;
                }

                // Create the user
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    UserName = serviceWorkerData.Email,
                    Email = serviceWorkerData.Email,
                    NormalizedUserName = serviceWorkerData.Email.ToUpper(),
                    NormalizedEmail = serviceWorkerData.Email.ToUpper(),
                    EmailConfirmed = true,
                    PhoneNumber = serviceWorkerData.PhoneNumber,
                    PhoneNumberConfirmed = true,
                    FirstName = serviceWorkerData.FirstName,
                    LastName = serviceWorkerData.LastName,
                    Bio = serviceWorkerData.Bio,
                    StreetAddress = serviceWorkerData.StreetAddress,
                    City = serviceWorkerData.City,
                    Country = serviceWorkerData.Country,
                    PostalCode = serviceWorkerData.PostalCode,
                    Latitude = serviceWorkerData.Latitude,
                    Longitude = serviceWorkerData.Longitude,
                    DateOfBirth = serviceWorkerData.DateOfBirth,
                    IsActive = true,
                    IsVerified = true,
                    IsProfileComplete = true,
                    IsProfileApproved = true,
                    CreatedAt = DateTime.UtcNow,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };

                // Create user with default password
                var result = await userManager.CreateAsync(user, "ServiceWorker123!");
                if (result.Succeeded)
                {
                    logger.LogInformation("Successfully created service worker user: {Email}", serviceWorkerData.Email);

                    // Add user to ServiceWorker role
                    var roleResult = await userManager.AddToRoleAsync(user, UserType.ServiceWorker);
                    if (roleResult.Succeeded)
                    {
                        logger.LogInformation("Successfully assigned ServiceWorker role to: {Email}", serviceWorkerData.Email);
                    }
                    else
                    {
                        logger.LogError("Failed to assign ServiceWorker role to {Email}. Errors: {Errors}",
                            serviceWorkerData.Email, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                    }

                    // Create ServiceWorkerUser profile
                    var serviceWorkerProfile = new ServiceWorkerUser
                    {
                        Id = 0, // Auto-generated
                        UserId = user.Id,
                        Bio = serviceWorkerData.Bio,
                        Address = serviceWorkerData.StreetAddress,
                        City = serviceWorkerData.City,
                        Country = serviceWorkerData.Country,
                        PostalCode = serviceWorkerData.PostalCode,
                        Latitude = serviceWorkerData.Latitude,
                        Longitude = serviceWorkerData.Longitude,
                        HourlyRate = serviceWorkerData.HourlyRate,
                        DateOfBirth = serviceWorkerData.DateOfBirth,
                        ServicesJson = JsonSerializer.Serialize(serviceWorkerData.Services),
                        LanguagesJson = JsonSerializer.Serialize(serviceWorkerData.Languages),
                        CreatedAt = DateTime.UtcNow
                    };

                    context.ServiceWorkerUsers.Add(serviceWorkerProfile);
                    await context.SaveChangesAsync();

                    logger.LogInformation("Successfully created service worker profile for: {Email}", serviceWorkerData.Email);
                }
                else
                {
                    logger.LogError("Failed to create service worker user {Email}. Errors: {Errors}",
                        serviceWorkerData.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception occurred while creating service worker user: {Email}", serviceWorkerData.Email);
            }
        }

        logger.LogInformation("Service worker seeding process completed.");
    }
}
