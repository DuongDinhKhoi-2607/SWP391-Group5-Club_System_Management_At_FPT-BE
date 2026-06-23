using DataAccessLayer.Models;

namespace DataAccessLayer.Repositories
{
    public interface IClubRepository
    {
        Task<bool> ClubNameExistsAsync(string clubName);
        Task<bool> ClubCodeExistsAsync(string clubCode);
        Task<Student?> GetStudentByIdAsync(string studentId);
        /// <summary>
        /// Kiểm tra sinh viên đã có tài khoản User chưa (qua bảng userinformation).
        /// Trả về true nếu đã tồn tại User, false nếu chưa.
        /// </summary>
        Task<bool> UserExistsByStudentIdAsync(string studentId);
        Task<Club> CreateClubWithLeaderAsync(Club club, Student leaderStudent);
        Task<Club?> GetByIdAsync(long clubId);
        Task<(Club club, long? leaderId, string? leaderStudentId, string? leaderFullName, string? leaderEmail)?> GetWithLeaderByIdAsync(long clubId);
        Task<IEnumerable<Club>> GetAllAsync(string? statusFilter);
        Task<bool> IsLeaderOfClubAsync(long userId, long clubId);
        Task UpdateAsync(Club club);
        Task UpdateStatusAsync(long clubId, string newStatus);
    }
}