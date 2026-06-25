namespace BussinessLayer.DTOs.User;

public class UserListDto
{
    public long UserId { get; set; }
    public string Username { get; set; } = null!;
    public string SystemRole { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string? StudentId { get; set; }
    public string? FullName { get; set; }
    public string? SchoolEmail { get; set; }
    public string? Avatar { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}
