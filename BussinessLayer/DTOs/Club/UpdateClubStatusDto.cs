using System.ComponentModel.DataAnnotations;

namespace BussinessLayer.DTOs.Club
{
    public class UpdateClubStatusDto
    {
        /// <summary>Trạng thái mới: "Đang hoạt động" | "Tạm dừng" | "Giải thể"</summary>
        [Required]
        public string Status { get; set; } = null!;
    }
}
