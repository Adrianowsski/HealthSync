using System.ComponentModel.DataAnnotations;

namespace HealthSync.Intranet.Models;

public class LoginViewModel
{
    [Required]
    public string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }
}