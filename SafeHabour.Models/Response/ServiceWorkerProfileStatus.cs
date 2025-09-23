using System;

namespace SafeHabour.Models.Response;

public class ServiceWorkerProfileStatus
{
    public bool IsComplete { get; set; }
    public double CompletionPercentage { get; set; }
    public List<string> MissingFields { get; set; } = new List<string>();
    public List<string> CompletedFields { get; set; } = new List<string>();
    public string Message { get; set; } = string.Empty;
}
