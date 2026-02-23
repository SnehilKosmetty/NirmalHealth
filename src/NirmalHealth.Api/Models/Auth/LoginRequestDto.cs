using System.Text.Json.Serialization;

namespace NirmalHealth.Api.Models.Auth;

public class LoginRequestDto
{
    [JsonPropertyName("emailOrPhone")]
    public string EmailOrPhone { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}
