namespace NirmalHealth.Api.Models.Doctors;

public class DoctorDetailDto : DoctorListDto
{
    public string? Email { get; set; }
    public string? RegistrationNumber { get; set; }
}
