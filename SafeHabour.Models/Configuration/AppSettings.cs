namespace SafeHabour.Models.Configuration;

public class AppSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public ConnectionStrings ConnectionStrings { get; set; } = new();
    public JwtSettings JwtSettings { get; set; } = new();
    public SendGridSettings SendGrid { get; set; } = new();
    public LoggingSettings Logging { get; set; } = new();
    public string AllowedHosts { get; set; } = string.Empty;
}

public class ConnectionStrings
{
    public string DefaultConnection { get; set; } = string.Empty;
}

public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryInMinutes { get; set; } = 60;
}

public class SendGridSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
}

public class LoggingSettings
{
    public LogLevel LogLevel { get; set; } = new();
}

public class LogLevel
{
    public string Default { get; set; } = "Information";
    public string MicrosoftAspNetCore { get; set; } = "Warning";
}
