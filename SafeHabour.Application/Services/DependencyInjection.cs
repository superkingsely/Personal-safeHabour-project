using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SafeHabour.Application.Interfaces;
using SafeHabour.Application.Managers;
using SafeHabour.Infrastructure.Interfaces;
using SafeHabour.Infrastructure.Repositories;
using SafeHabour.Models.Configuration;

namespace SafeHabour.Application.Services;

public static class DependencyInjection
{
    /// <summary>
    /// Registers strongly-typed configuration classes
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddConfigurationSettings(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind the main AppSettings class
        services.Configure<AppSettings>(configuration);
        
        // Bind individual configuration sections for easier injection
        services.Configure<ConnectionStrings>(configuration.GetSection("ConnectionStrings"));
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        services.Configure<SendGridSettings>(configuration.GetSection("SendGrid"));
        services.Configure<LoggingSettings>(configuration.GetSection("Logging"));
        services.Configure<SerilogSettings>(configuration.GetSection("Serilog"));

        return services;
    }

    /// <summary>
    /// Registers all application services and repositories
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register Application Services
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IPushNotificationService, PushNotificationService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ISuperAdminService, SuperAdminService>();
        services.AddScoped<IClientUserService, ClientUserService>();
        services.AddScoped<IServiceWorkerService, ServiceWorkerService>();

        return services;
    }

    /// <summary>
    /// Registers all repository services
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddRepositoryServices(this IServiceCollection services)
    {
        // Register Repository Services
        services.AddScoped<ISuperAdminRepository, SuperAdminRepository>();
        services.AddScoped<IClientUserRepository, ClientUserRepository>();
        services.AddScoped<IServiceWorkerRepository, ServiceWorkerRepository>();

        return services;
    }

    /// <summary>
    /// Registers all application and repository services in one call
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddSafeHabourServices(this IServiceCollection services)
    {
        services.AddApplicationServices();
        services.AddRepositoryServices();

        return services;
    }
}
