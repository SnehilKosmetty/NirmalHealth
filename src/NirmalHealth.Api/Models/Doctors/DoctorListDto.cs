namespace NirmalHealth.Api.Models.Doctors;

public class DoctorListDto
{
    public int Id { get; set; }
    public int HospitalId { get; set; }
    public string HospitalName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Qualification { get; set; }
    public string[] Specialties { get; set; } = Array.Empty<string>();
    public bool SupportsInPerson { get; set; }
    public bool SupportsVideo { get; set; }
    public string? Phone { get; set; }
}
