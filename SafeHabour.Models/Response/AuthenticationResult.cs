namespace SafeHabour.Models.Response;

public class AuthenticationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public UserDto? User { get; set; }
    public bool RequiresTwoFactor { get; set; } = false;
    public dynamic? Data { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
}
