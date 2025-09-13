using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthSync.Shared.Models;

public class Appointment
{
    public int Id { get; set; }

    // FK (czytelne i wygodne)
    public int PatientProfileId { get; set; }
    public int DoctorProfileId  { get; set; }

    // Nawigacje – wypełnia EF
    public PatientProfile PatientProfile { get; set; } = null!;
    public DoctorProfile  DoctorProfile  { get; set; } = null!;

    public DateTime AppointmentDate { get; set; }

    [MaxLength(32)]
    public string Status { get; set; } = "Scheduled";

    public MedicalRecord? MedicalRecord { get; set; }
    public List<Prescription> Prescriptions { get; set; } = new();

    [NotMapped]
    public string DisplayName =>
        $"{PatientProfile?.FirstName} {PatientProfile?.LastName} — {AppointmentDate:g}";
}
