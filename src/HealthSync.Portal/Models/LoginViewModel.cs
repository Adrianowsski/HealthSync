using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace HealthSync.Shared.Models;

public class ChatMessage
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }

    public string SenderId   { get; set; } = string.Empty;
    public string ReceiverId { get; set; } = string.Empty;
    public string Content    { get; set; } = string.Empty;
    public DateTime SentAt   { get; set; } = DateTime.UtcNow;

    [ValidateNever]
    public Appointment? Appointment { get; set; }
}
