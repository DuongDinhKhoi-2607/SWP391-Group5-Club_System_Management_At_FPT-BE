using BussinessLayer.DTOs;
using BussinessLayer.DTOs.Club;
using DataAccessLayer.Models;

namespace BussinessLayer.Interfaces
{
    public interface IClubService
    {
        /// <summary>[ADMIN] Tạo CLB mới. Sinh viên phải tồn tại, chưa có tài khoản User.</summary>
        Task<Club> CreateClubAsync(CreateClubDto dto);

        /// <summary>[ADMIN] Cập nhật thông tin CLB — bỏ qua kiểm tra Leader.</summary>
        Task<Club> UpdateClubByAdminAsync(long clubId, UpdateClubDto dto);

        /// <summary>[Leader của CLB] Cập nhật thông tin CLB — yêu cầu kiểm tra Leader.</summary>
        Task<Club> UpdateClubAsync(long clubId, UpdateClubDto dto, long currentUserId);

        /// <summary>[ADMIN / All] Danh sách CLB, filter theo status nếu truyền vào.</summary>
        Task<IEnumerable<ClubListDto>> GetAllAsync(string? statusFilter);
        Task<int> GetTotalClubsAsync(string? statusFilter);

        Task<ClubDetailDto?> GetByIdAsync(long clubId);

        /// <summary>[ADMIN] Đổi trạng thái CLB (Đang hoạt động / Tạm dừng / Giải thể).</summary>
        Task UpdateStatusAsync(long clubId, string newStatus);

        /// <summary>[ADMIN,Manager] Lấy thống kê hoạt động chi tiết của một CLB.</summary>
        Task<ClubStatsDto> GetClubStatsAsync(long clubId);

        /// <summary>[ADMIN] Xóa CLB (soft delete — đổi status thành "Giải thể").</summary>
        Task DeleteClubAsync(long clubId);
    }
}