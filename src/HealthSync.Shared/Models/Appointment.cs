using System.ComponentModel.DataAnnotations.Schema;

namespace HealthSync.Shared.Models;

public class Appointment
{
    public int Id { get; set; }
    public int PatientProfileId { get; set; }
    public int DoctorProfileId { get; set; }
    public DateTime AppointmentDate { get; set; }

    // Ujednolicone z testami (lowercase)
    public string Status { get; set; } = "scheduled";

    // Zainicjalizowane, żeby nie były null
    public PatientProfile PatientProfile { get; set; } = new();
    public DoctorProfile  DoctorProfile  { get; set; } = new();

    public MedicalRecord? MedicalRecord { get; set; }
    public List<Prescription> Prescriptions { get; set; } = new();

    [NotMapped]
    public string DisplayName => $"{PatientProfile.FirstName} {PatientProfile.LastName} — {AppointmentDate:g}";
}
