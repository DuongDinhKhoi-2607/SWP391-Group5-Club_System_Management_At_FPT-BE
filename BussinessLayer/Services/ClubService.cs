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

        public async Task<Club> CreateClubAsync(CreateClubDto dto)
        {
            if (await _repo.ClubNameExistsAsync(dto.ClubName))
                throw new Exception($"Tên CLB '{dto.ClubName}' đã tồn tại.");
            if (await _repo.ClubCodeExistsAsync(dto.ClubCode))
                throw new Exception($"Mã CLB '{dto.ClubCode}' đã tồn tại.");

            var student = await _repo.GetStudentByIdAsync(dto.ManagerStudentId);
            if (student == null)
                throw new Exception($"Không tìm thấy sinh viên MSSV '{dto.ManagerStudentId}'.");
            if (student.Status != "Đang học")
                throw new Exception($"Sinh viên '{student.Fullname}' không đủ điều kiện (trạng thái: {student.Status}).");

            var club = new Club
            {
                Clubname = dto.ClubName, Clubcode = dto.ClubCode,
                Description = dto.Description, Fanpageurl = dto.FanpageUrl,
                Logoimage = dto.LogoImage, Foundeddate = dto.FoundedDate,
                Status = "Đang hoạt động", Totalactivemembers = 1
            };
            return await _repo.CreateClubWithManagerAsync(club, student);
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