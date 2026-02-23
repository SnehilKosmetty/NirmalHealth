namespace NirmalHealth.Api.Models.Auth;

public class LoginResultDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public UserInfoDto User { get; set; } = null!;
}
