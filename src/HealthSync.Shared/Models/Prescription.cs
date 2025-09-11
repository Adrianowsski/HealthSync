using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace HealthSync.Shared.Models;

public class Prescription
{
    public int Id { get; set; }

    [Required, Display(Name = "Appointment")]
    public int AppointmentId { get; set; }

    [Required, Display(Name = "Medication Name")]
    public string MedicationName { get; set; } = string.Empty;

    [Required] public string Dosage   { get; set; } = string.Empty;
    [Required] public string Duration { get; set; } = string.Empty;

    public string Instructions { get; set; } = string.Empty;

    public string AccessCode { get; set; } =
        Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();

    [ValidateNever]
    public Appointment? Appointment { get; set; }

    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
}
