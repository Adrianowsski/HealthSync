namespace HealthSync.Shared.Models;

public class RegistrationCode
{
    public int Id { get; set; }
    public string Code { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsUsed { get; set; } = false;
}