using BussinessLayer.DTOs;

namespace BussinessLayer.Interfaces
{
    public interface IClubMemberListService
    {
        /// <summary>
        /// Leader xem danh sách thành viên đang hoạt động của CLB
        /// </summary>
        Task<List<ClubMemberListDto>> GetActiveMembersByClubAsync(
            long clubId,
            long currentUserId);

        /// <summary>
        /// Leader thêm thành viên bằng MSSV
        /// </summary>
        Task<ClubMemberListDto> AddMemberByStudentIdAsync(
            AddClubMemberDto dto,
            long currentUserId);

        /// <summary>
        /// Xem thông tin chi tiết của một thành viên
        /// </summary>
        Task<ClubMemberDetailDto> GetMemberDetailAsync(
            long membershipId);

        /// <summary>
        /// Leader xóa mềm thành viên khỏi CLB
        /// </summary>
        Task RemoveMemberAsync(
            long membershipId,
            long currentUserId);

        Task ActivateMemberAsync(string token);
    }
}