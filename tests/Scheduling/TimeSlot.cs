namespace HealthSync.Shared.Scheduling;

public readonly record struct TimeSlot(DateTime Start, DateTime End)
{
    public bool IsValid => Start < End;

    /// <summary>Czy zakresy czasu nachodzą na siebie (styk NIE jest kolizją)?</summary>
    public bool Overlaps(TimeSlot other)
        => Start < other.End && other.Start < End;

    public TimeSpan Duration => End - Start;
}
