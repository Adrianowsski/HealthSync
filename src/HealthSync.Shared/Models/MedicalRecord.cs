using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace HealthSync.Shared.Models;

public class MedicalRecord
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Appointment")]
    
    public int AppointmentId { get; set; }

    [Required]
    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [ValidateNever]
    public Appointment Appointment { get; set; } = null!;

}
