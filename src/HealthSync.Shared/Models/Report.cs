namespace HealthSync.Shared.Models;

public class Report
{
    public int      Id          { get; set; }
    public string   Title       { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public string   GeneratedBy { get; set; } = string.Empty;
    public string   FilePath    { get; set; } = string.Empty;   // tylko PDF!

    public DateTime? PeriodFrom { get; set; }
    public DateTime? PeriodTo   { get; set; }
}