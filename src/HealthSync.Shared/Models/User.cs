using Microsoft.AspNetCore.Identity;

namespace HealthSync.Shared.Models;

public class User : IdentityUser
{
    public PatientProfile? PatientProfile { get; set; }
    public DoctorProfile? DoctorProfile { get; set; }
}