namespace HealthSync.Intranet.Services;

public static class ReportTemplateService
{
    public static Dictionary<string, string> Templates => new()
    {
        { "Appointments Summary", "All scheduled appointments with patient and status info" },
        { "Prescription Overview", "Most frequently prescribed medications" },
        { "Chat Overview", "Recent patient-doctor communication" },
        { "Patient List", "List of all registered patients with PESEL" },
        { "Medical Records Summary", "Overview of medical records and diagnoses" }
    };

    public static List<string> Titles => Templates.Keys.ToList();
}