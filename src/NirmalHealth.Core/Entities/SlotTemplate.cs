namespace NirmalHealth.Core.Entities;

/// <summary>Recurring weekly slot pattern for a doctor (e.g. Mon 9-11, Wed 14-16).</summary>
public class SlotTemplate
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public ConsultationType ConsultationType { get; set; }
    public int DurationMinutes { get; set; } = 15;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
}

public enum ConsultationType
{
    InPerson = 0,
    Video = 1
}
