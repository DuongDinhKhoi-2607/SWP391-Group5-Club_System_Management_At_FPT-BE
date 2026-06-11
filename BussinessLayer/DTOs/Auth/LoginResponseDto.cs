namespace BussinessLayer.DTOs.Auth;

public class LoginResponseDto
{
    /// <summary>true nếu user thuộc nhiều CLB và cần chọn CLB</summary>
    public bool RequireClubSelection { get; set; }

    /// <summary>Token tạm thời 5 phút, dùng khi RequireClubSelection = true</summary>
    public string? TempToken { get; set; }

    /// <summary>JWT chính thức, có khi login 1 CLB hoặc sau khi select-club</summary>
    public string? AccessToken { get; set; }

    /// <summary>Danh sách CLB để hiện màn hình chọn (khi RequireClubSelection = true)</summary>
    public List<ClubSelectionDto>? AvailableClubs { get; set; }

    public UserInfoDto? UserInfo { get; set; }
}
