namespace SafeHabour.Models.Response;

public class ServiceWorkerUserDto
{
    public int Id { get; set; }
    public string? Bio { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Services { get; set; } = string.Empty;
    public string Languages { get; set; } = string.Empty;
}
