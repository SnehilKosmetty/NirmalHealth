namespace NirmalHealth.Api.Models.Auth;

public class UserInfoDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PreferredLanguage { get; set; } = "en";
    /// <summary>Display only: "Verified" when Aadhaar is verified. No digits ever exposed.</summary>
    public string? AadhaarMasked { get; set; }
    public bool AadhaarVerified { get; set; }
    public string[] Roles { get; set; } = Array.Empty<string>();
    public int? HospitalId { get; set; }
}

public class UpdateProfileRequestDto
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    /// <summary>Optional. If provided, 12 digits; stored as masked only.</summary>
    public string? AadhaarNumber { get; set; }
    public string? PreferredLanguage { get; set; }
}
