namespace NirmalHealth.Api.Models.Appointments;

public class SlotDto
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public int HospitalId { get; set; }
    public string HospitalName { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string ConsultationType { get; set; } = "InPerson";
}
