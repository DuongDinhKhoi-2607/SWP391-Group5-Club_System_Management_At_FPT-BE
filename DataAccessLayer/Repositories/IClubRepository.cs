using DataAccessLayer.Models;

namespace DataAccessLayer.Repositories
{
    public interface IClubRepository
    {
        Task<bool> ClubNameExistsAsync(string clubName);
        Task<bool> ClubCodeExistsAsync(string clubCode);
        Task<Student?> GetStudentByIdAsync(string studentId);
        Task<Club> CreateClubWithManagerAsync(Club club, Student managerStudent);
        Task<Club?> GetByIdAsync(long clubId);
        Task<bool> IsLeaderOfClubAsync(long userId, long clubId);
        Task UpdateAsync(Club club);
    }
}