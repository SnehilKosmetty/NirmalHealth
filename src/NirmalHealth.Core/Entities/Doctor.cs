namespace NirmalHealth.Core.Entities;

public class Doctor
{
    public int Id { get; set; }
    public int HospitalId { get; set; }
    public Hospital Hospital { get; set; } = null!;
    public string FullName { get; set; } = string.Empty;
    public string? Qualification { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool SupportsInPerson { get; set; } = true;
    public bool SupportsVideo { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<DoctorSpecialty> DoctorSpecialties { get; set; } = new List<DoctorSpecialty>();
    public ICollection<SlotTemplate> SlotTemplates { get; set; } = new List<SlotTemplate>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
