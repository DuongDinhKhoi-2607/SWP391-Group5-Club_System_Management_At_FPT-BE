namespace BussinessLayer.DTOs.Auth;

public class UserInfoDto
{
    public long UserId { get; set; }
    public string Username { get; set; } = null!;
    public string SystemRole { get; set; } = null!;
    public string? FullName { get; set; }
    public string? Avatar { get; set; }

    /// <summary>ID của CLB đang đăng nhập (null nếu là Admin hoặc chưa chọn CLB)</summary>
    public long? ClubId { get; set; }

    /// <summary>Role trong CLB đang đăng nhập: "Manager" | "Member" | null</summary>
    public string? ClubRole { get; set; }
}
