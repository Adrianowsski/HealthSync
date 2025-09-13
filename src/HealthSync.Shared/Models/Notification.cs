namespace HealthSync.Shared.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // relacja do statusów odczytu
        public ICollection<NotificationReadStatus> ReadStatuses { get; set; }
            = new List<NotificationReadStatus>();
    }
}