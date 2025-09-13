using System.ComponentModel.DataAnnotations.Schema;

namespace HealthSync.Shared.Models;

public class Appointment
{
    public int Id { get; set; }
    public int PatientProfileId { get; set; }
    public int DoctorProfileId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string Status { get; set; } = "Scheduled";

    public PatientProfile PatientProfile { get; set; }
    public DoctorProfile DoctorProfile { get; set; }

    public MedicalRecord? MedicalRecord { get; set; }
    public List<Prescription> Prescriptions { get; set; } = new();


    [NotMapped]
    public string DisplayName => $"{PatientProfile?.FirstName} {PatientProfile?.LastName} — {AppointmentDate:g}";
}