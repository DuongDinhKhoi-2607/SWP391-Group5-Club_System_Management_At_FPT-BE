using BussinessLayer.DTOs.Auth;

namespace BussinessLayer.Interfaces;

public interface IAuthService
{
    /// <summary>
    /// Bước 1: Đăng nhập với username/password.
    /// - Admin → trả AccessToken ngay.
    /// - 1 CLB → tự chọn, trả AccessToken ngay.
    /// - Nhiều CLB → trả TempToken + AvailableClubs, RequireClubSelection = true.
    /// </summary>
    Task<LoginResponseDto> LoginAsync(LoginRequestDto dto);

    /// <summary>
    /// Bước 2: Chọn CLB sau khi nhận RequireClubSelection = true.
    /// Verify TempToken → sinh AccessToken chính thức có clubId + clubRole.
    /// </summary>
    Task<LoginResponseDto> SelectClubAsync(SelectClubRequestDto dto);
    string GenerateActivationToken(long userId, long clubId);
    (long userId, long clubId) ValidateActivationToken(string token);
}
