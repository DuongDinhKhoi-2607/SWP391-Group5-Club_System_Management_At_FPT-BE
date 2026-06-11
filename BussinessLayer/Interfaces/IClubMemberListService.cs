using BussinessLayer.DTOs;

namespace BussinessLayer.Interfaces
{
    public interface IClubMemberListService
    {
        Task<List<ClubMemberListDto>> GetActiveMembersByClubAsync(
            long clubId,
            long currentUserId);

        Task<ClubMemberListDto> AddMemberByStudentIdAsync(
            AddClubMemberDto dto,
            long currentUserId);
    }
}