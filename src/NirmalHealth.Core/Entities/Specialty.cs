namespace NirmalHealth.Core.Entities;

public class Specialty
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // General Medicine, Cardiology, etc.

    public ICollection<HospitalSpecialty> HospitalSpecialties { get; set; } = new List<HospitalSpecialty>();
    public ICollection<DoctorSpecialty> DoctorSpecialties { get; set; } = new List<DoctorSpecialty>();
}
