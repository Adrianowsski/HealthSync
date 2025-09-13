namespace HealthSync.Shared.Models;

public class DoctorProfile
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;
    public string LastName  { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string Schedule { get; set; } = string.Empty;

    public User User { get; set; } = null!;

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
