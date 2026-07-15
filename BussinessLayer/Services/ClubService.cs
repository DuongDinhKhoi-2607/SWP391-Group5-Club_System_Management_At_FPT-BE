using BussinessLayer.DTOs;
using BussinessLayer.DTOs.Club;
using BussinessLayer.Interfaces;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories;

namespace BussinessLayer.Services
{
    public class ClubService : IClubService
    {
        private readonly IClubRepository _repo;

        private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "Đang hoạt động", "Tạm dừng", "Giải thể"
        };

        public ClubService(IClubRepository repo)
        {
            _repo = repo;
        }

        // ─────────────────────────────────────────────────────────────
        // CREATE
        // ─────────────────────────────────────────────────────────────

        public async Task<Club> CreateClubAsync(CreateClubDto dto)
        {
            // Validate tên & mã CLB
            if (await _repo.ClubNameExistsAsync(dto.ClubName))
                throw new Exception($"Tên CLB '{dto.ClubName}' đã tồn tại.");
            if (await _repo.ClubCodeExistsAsync(dto.ClubCode))
                throw new Exception($"Mã CLB '{dto.ClubCode}' đã tồn tại.");

            // Kiểm tra sinh viên
            var student = await _repo.GetStudentByIdAsync(dto.LeaderStudentId);
            if (student == null)
                throw new Exception($"Không tìm thấy sinh viên có MSSV '{dto.LeaderStudentId}' trong hệ thống.");
            if (student.Status != "Đang học")
                throw new Exception($"Sinh viên '{student.Fullname}' không đủ điều kiện (trạng thái: {student.Status}).");



            var club = new Club
            {
                Clubname           = dto.ClubName,
                Clubcode           = dto.ClubCode,
                Description        = dto.Description,
                Fanpageurl         = dto.FanpageUrl,
                Logoimage          = dto.LogoImage,
                Foundeddate        = dto.FoundedDate,
                Status             = "Đang hoạt động",
                Totalactivemembers = 1,
                Createdat          = DateTime.Now
            };

            return await _repo.CreateClubWithLeaderAsync(club, student);
        }

        // ─────────────────────────────────────────────────────────────
        // READ
        // ─────────────────────────────────────────────────────────────

        public async Task<IEnumerable<ClubListDto>> GetAllAsync(string? statusFilter)
        {
            var clubs = await _repo.GetAllAsync(statusFilter);
            return clubs.Select(c => new ClubListDto
            {
                ClubId             = c.Clubid,
                ClubCode           = c.Clubcode,
                ClubName           = c.Clubname,
                Description        = c.Description,
                LogoImage          = c.Logoimage,
                FanpageUrl         = c.Fanpageurl,
                TotalActiveMembers = c.Totalactivemembers,
                Status             = c.Status,
                FoundedDate        = c.Foundeddate,
                CreatedAt          = c.Createdat
            });
        }

        public async Task<int> GetTotalClubsAsync(string? statusFilter)
        {
            return await _repo.CountClubsAsync(statusFilter);
        }

        public async Task<ClubDetailDto?> GetByIdAsync(long clubId)
        {
            var result = await _repo.GetWithLeaderByIdAsync(clubId);
            if (result == null) return null;

            var (club, leaderId, leaderStudentId, leaderFullName, leaderEmail) = result.Value;

            return new ClubDetailDto
            {
                ClubId             = club.Clubid,
                ClubCode           = club.Clubcode,
                ClubName           = club.Clubname,
                Description        = club.Description,
                LogoImage          = club.Logoimage,
                FanpageUrl         = club.Fanpageurl,
                TotalActiveMembers = club.Totalactivemembers,
                Status             = club.Status,
                FoundedDate        = club.Foundeddate,
                CreatedAt          = club.Createdat,
                Leader = leaderId.HasValue
                    ? new ClubLeaderDto
                    {
                        UserId      = leaderId.Value,
                        StudentId   = leaderStudentId ?? "",
                        FullName    = leaderFullName ?? "",
                        SchoolEmail = leaderEmail
                    }
                    : null
            };
        }

        // ─────────────────────────────────────────────────────────────
        // UPDATE
        // ─────────────────────────────────────────────────────────────

        public async Task<Club> UpdateClubByAdminAsync(long clubId, UpdateClubDto dto)
        {
            var club = await _repo.GetByIdAsync(clubId)
                ?? throw new Exception("Không tìm thấy câu lạc bộ.");

            club.Clubname    = dto.ClubName;
            club.Description = dto.Description;
            club.Logoimage   = dto.LogoImage;
            club.Fanpageurl  = dto.FanpageUrl;

            if (dto.FoundedDate.HasValue)
                club.Foundeddate = DateOnly.FromDateTime(dto.FoundedDate.Value);

            await _repo.UpdateAsync(club);
            return club;
        }

        public async Task<Club> UpdateClubAsync(long clubId, UpdateClubDto dto, long currentUserId)
        {
            var club = await _repo.GetByIdAsync(clubId)
                ?? throw new Exception("Không tìm thấy câu lạc bộ.");

            // Kiểm tra quyền: phải là Leader hoặc ADMIN (Admin check ở controller)
            var isLeader = await _repo.IsLeaderOfClubAsync(currentUserId, clubId);
            if (!isLeader)
                throw new UnauthorizedAccessException("Chỉ Leader của CLB hoặc Admin mới được chỉnh sửa thông tin.");

            club.Clubname   = dto.ClubName;
            club.Description = dto.Description;
            club.Logoimage  = dto.LogoImage;
            club.Fanpageurl = dto.FanpageUrl;

            if (dto.FoundedDate.HasValue)
                club.Foundeddate = DateOnly.FromDateTime(dto.FoundedDate.Value);

            await _repo.UpdateAsync(club);
            return club;
        }

        public async Task UpdateStatusAsync(long clubId, string newStatus)
        {
            if (!AllowedStatuses.Contains(newStatus))
                throw new Exception(
                    $"Trạng thái '{newStatus}' không hợp lệ. " +
                    $"Các giá trị cho phép: 'Đang hoạt động', 'Tạm dừng', 'Giải thể'.");

            if (!await _repo.GetByIdAsync(clubId).ContinueWith(t => t.Result != null))
                throw new Exception($"Không tìm thấy CLB với ID {clubId}.");

            await _repo.UpdateStatusAsync(clubId, newStatus);
        }
    }
}