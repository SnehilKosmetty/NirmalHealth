namespace NirmalHealth.Core.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    /// <summary>Masked Aadhaar e.g. XXXXXX1234. Full number never stored.</summary>
    public string? AadhaarMasked { get; set; }
    public bool AadhaarVerified { get; set; }
    public string PreferredLanguage { get; set; } = "en";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    /// <summary>Only set for HospitalAdmin role: which hospital this user manages.</summary>
    public int? HospitalId { get; set; }
    public Hospital? Hospital { get; set; }
    public ICollection<Appointment> AppointmentsAsPatient { get; set; } = new List<Appointment>();
}
