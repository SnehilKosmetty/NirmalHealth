namespace NirmalHealth.Core.Entities;

public class Hospital
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Area { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public HospitalType Type { get; set; }
    public int BedCount { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsEmergency { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<HospitalSpecialty> HospitalSpecialties { get; set; } = new List<HospitalSpecialty>();
    public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    public ICollection<User> HospitalAdmins { get; set; } = new List<User>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}

public enum HospitalType
{
    Government = 0,
    Private = 1,
    MultiSpecialty = 2,
    NursingHome = 3
}
