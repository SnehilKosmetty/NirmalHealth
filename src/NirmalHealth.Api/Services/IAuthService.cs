using NirmalHealth.Api.Models.Auth;

namespace NirmalHealth.Api.Services;

public interface IAuthService
{
    Task<LoginResultDto> RegisterAsync(RegisterRequestDto dto, CancellationToken ct = default);
    Task<LoginResultDto> LoginAsync(LoginRequestDto dto, CancellationToken ct = default);
    Task<LoginResultDto?> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    Task<UserInfoDto?> GetUserInfoAsync(int userId, CancellationToken ct = default);
    Task<UserInfoDto?> UpdateProfileAsync(int userId, UpdateProfileRequestDto dto, CancellationToken ct = default);
}
