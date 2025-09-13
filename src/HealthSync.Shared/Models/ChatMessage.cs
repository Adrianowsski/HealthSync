using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthSync.Shared.Models;

public class ChatMessage
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string ReceiverId { get; set; } = string.Empty;

    // Kolumna w bazie:
    public string Content { get; set; } = string.Empty;

    // Alias zgodny z dawną nazwą, bez dodatkowej kolumny:
    [NotMapped]
    public string Message
    {
        get => Content;
        set => Content = value;
    }

    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    [ValidateNever]
    public Appointment Appointment { get; set; } = null!;
}
