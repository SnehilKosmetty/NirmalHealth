namespace NirmalHealth.Core.Entities;

/// <summary>Concrete bookable slot on a specific date.</summary>
public class AppointmentSlot
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public ConsultationType ConsultationType { get; set; }
    public bool IsBooked { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public Appointment? Appointment { get; set; }
}
