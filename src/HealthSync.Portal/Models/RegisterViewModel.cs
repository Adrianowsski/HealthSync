using System.ComponentModel.DataAnnotations;

namespace HealthSync.Portal.Models;

public class RegisterViewModel
{
    [Required]
    [Display(Name = "Adres email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Hasło")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Imię")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Nazwisko")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Numer PESEL")]
    public string PESEL { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Kod rejestracyjny")]
    public string RegistrationCode { get; set; } = string.Empty;
}