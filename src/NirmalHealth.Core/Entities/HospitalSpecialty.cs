namespace NirmalHealth.Core.Entities;

public class HospitalSpecialty
{
    public int HospitalId { get; set; }
    public Hospital Hospital { get; set; } = null!;
    public int SpecialtyId { get; set; }
    public Specialty Specialty { get; set; } = null!;
}
