using System.ComponentModel.DataAnnotations;

namespace BussinessLayer.DTOs
{
    public class RegisterEventRequestDto
    {
        [Required(ErrorMessage = "Vai trò trong sự kiện không được để trống")]
        [StringLength(50, ErrorMessage = "Vai trò không vượt quá 50 ký tự")]
        public string RoleInEvent { get; set; } = "Thành viên tham gia";
    }
}
