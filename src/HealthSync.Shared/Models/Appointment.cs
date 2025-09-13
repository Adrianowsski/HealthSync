using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthSync.Shared.Models;

public class Appointment
{
    public int Id { get; set; }

    // --- Foreign Keys (czytelne, przydatne w EF i w widokach) ---
    public int PatientProfileId { get; set; }
    public int DoctorProfileId  { get; set; }

    // --- Navigations (wypełniane przez EF) ---
    public PatientProfile PatientProfile { get; set; } = null!;
    public DoctorProfile  DoctorProfile  { get; set; } = null!;

    // --- Dane spotkania ---
    public DateTime AppointmentDate { get; set; }

    [MaxLength(32)]
    public string Status { get; set; } = "Scheduled";

    // --- Relacje powiązane ---
    // 1:1 (opcjonalne) – np. rekord medyczny tworzony po wizycie
    public MedicalRecord? MedicalRecord { get; set; }

    // 1:Many – recepty wystawione w ramach wizyty
    public List<Prescription> Prescriptions { get; set; } = new();

    // --- Wygodny opis w UI ---
    [NotMapped]
    public string DisplayName =>
        $"{PatientProfile?.FirstName} {PatientProfile?.LastName} — {AppointmentDate:g}";
}
