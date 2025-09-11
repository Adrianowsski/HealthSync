namespace HealthSync.Shared.Models
{
    public class NotificationReadStatus
    {
        public int Id { get; set; }

        public int NotificationId { get; set; }
        public Notification Notification { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public DateTime ReadAt { get; set; } = DateTime.UtcNow;
    }
}