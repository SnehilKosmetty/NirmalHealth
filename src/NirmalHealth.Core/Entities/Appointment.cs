namespace NirmalHealth.Core.Entities;

public class Appointment
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public User Patient { get; set; } = null!;
    public int SlotId { get; set; }
    public AppointmentSlot Slot { get; set; } = null!;
    public string? ChiefComplaint { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    public string? VideoMeetingUrl { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public bool ReminderSent { get; set; }
}

public enum AppointmentStatus
{
    Scheduled = 0,
    Completed = 1,
    Cancelled = 2,
    NoShow = 3
}
