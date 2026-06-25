using System.ComponentModel.DataAnnotations;

namespace BussinessLayer.DTOs.User;

public class CreateStaffUserDto
{
    [Required(ErrorMessage = "Tên đăng nhập (Username) là bắt buộc.")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "Mật khẩu (Password) là bắt buộc.")]
    [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "SystemRole là bắt buộc.")]
    public string SystemRole { get; set; } = null!; // ADMIN or MANAGER
}
