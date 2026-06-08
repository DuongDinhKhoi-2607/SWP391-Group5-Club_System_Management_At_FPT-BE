using BussinessLayer.DTOs;
using BussinessLayer.Interfaces;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories;

namespace BussinessLayer.Services
{
    public class ClubService : IClubService
    {
        private readonly IClubRepository _repo;

        public ClubService(IClubRepository repo)
        {
            _repo = repo;
        }

        public async Task<Club> UpdateClubAsync(
            long clubId,
            UpdateClubDto dto,
            long currentUserId)
        {
            var club = await _repo.GetByIdAsync(clubId);

            if (club == null)
                throw new Exception("Không tìm thấy câu lạc bộ.");

            var isLeader = await _repo.IsLeaderOfClubAsync(currentUserId, clubId);

            if (!isLeader)
                throw new UnauthorizedAccessException("Chỉ Leader của CLB mới được chỉnh sửa thông tin.");

            club.Clubname = dto.ClubName;
            club.Description = dto.Description;
            club.Logoimage = dto.LogoImage;
            club.Fanpageurl = dto.FanpageUrl;

            if (dto.FoundedDate.HasValue)
                club.Foundeddate = DateOnly.FromDateTime(dto.FoundedDate.Value);

            await _repo.UpdateAsync(club);

            return club;
        }
    }
}