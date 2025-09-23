using SafeHabour.Models.Response;

namespace SafeHabour.Models.Response;

/// <summary>
/// Generic result class for service operations
/// </summary>
/// <typeparam name="T">The type of data returned</typeparam>
public class ServiceResult<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
    public Dictionary<string, object>? AdditionalData { get; set; }
    
    // Authentication-specific properties (optional, used only for auth endpoints)
    public bool RequiresTwoFactor { get; set; } = false;
    public UserDto? User { get; set; }

    public static ServiceResult<T> SuccessResult(T data, string message = "Operation completed successfully")
    {
        return new ServiceResult<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ServiceResult<T> FailureResult(string message, List<string>? errors = null)
    {
        return new ServiceResult<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }

    public static ServiceResult<T> FailureResult(string message, string error)
    {
        return new ServiceResult<T>
        {
            Success = false,
            Message = message,
            Errors = new List<string> { error }
        };
    }
    
    // Special factory method for 2FA scenarios
    public static ServiceResult<T> TwoFactorRequired(string message = "Two-factor authentication required")
    {
        return new ServiceResult<T>
        {
            Success = true,
            RequiresTwoFactor = true,
            Message = message
        };
    }
}

/// <summary>
/// Non-generic result class for operations that don't return data
/// </summary>
public class ServiceResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new List<string>();
    public Dictionary<string, object>? AdditionalData { get; set; }

    public static ServiceResult SuccessResult(string message = "Operation completed successfully")
    {
        return new ServiceResult
        {
            Success = true,
            Message = message
        };
    }

    public static ServiceResult FailureResult(string message, List<string>? errors = null)
    {
        return new ServiceResult
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }

    public static ServiceResult FailureResult(string message, string error)
    {
        return new ServiceResult
        {
            Success = false,
            Message = message,
            Errors = new List<string> { error }
        };
    }
}
