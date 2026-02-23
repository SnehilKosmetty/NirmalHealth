namespace NirmalHealth.Api.Models.Auth;

public class UserInfoDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PreferredLanguage { get; set; } = "en";
    public bool AadhaarVerified { get; set; }
    public string[] Roles { get; set; } = Array.Empty<string>();
    public int? HospitalId { get; set; }
}
