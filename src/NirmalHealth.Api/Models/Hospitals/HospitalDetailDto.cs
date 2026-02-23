namespace NirmalHealth.Api.Models.Hospitals;

public class HospitalDetailDto : HospitalListDto
{
    public string? Email { get; set; }
    public List<DoctorSummaryDto> Doctors { get; set; } = new();
}

public class DoctorSummaryDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Qualification { get; set; }
    public string[] Specialties { get; set; } = Array.Empty<string>();
    public bool SupportsInPerson { get; set; }
    public bool SupportsVideo { get; set; }
    public string? Phone { get; set; }
}
