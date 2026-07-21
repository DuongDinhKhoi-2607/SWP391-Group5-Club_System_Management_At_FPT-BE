namespace BussinessLayer.DTOs.User;

public class UserDetailDto
{
    public long UserId { get; set; }
    public string Username { get; set; } = null!;
    public string SystemRole { get; set; } = null!;
    public string Status { get; set; } = null!;

    // Thông tin sinh viên (null nếu là ADMIN/MANAGER không có StudentInfo)
    public string? StudentId { get; set; }
    public string? FullName { get; set; }
    public string? SchoolEmail { get; set; }
    public string? Avatar { get; set; }
    public string? Phone { get; set; }
    public string? Gender { get; set; }
    public string? Major { get; set; }
    public string? AcademicBatch { get; set; }
    public bool? IsAlumni { get; set; }
    public DateOnly? GraduationDate { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}
