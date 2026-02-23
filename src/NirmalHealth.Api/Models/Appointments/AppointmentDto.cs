namespace NirmalHealth.Api.Models.Appointments;

public class AppointmentDto
{
    public int Id { get; set; }
    public int SlotId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public string ConsultationType { get; set; } = "InPerson";
    public string? ChiefComplaint { get; set; }
    public string Status { get; set; } = "Scheduled";
    public string? VideoMeetingUrl { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public int HospitalId { get; set; }
    public string HospitalName { get; set; } = string.Empty;
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string? PatientPhone { get; set; }
}
