using DataAccessLayer.Models;

namespace DataAccessLayer.Repositories
{
    public interface IClubRepository
    {
        Task<Club?> GetByIdAsync(long clubId);
        Task<bool> IsLeaderOfClubAsync(long userId, long clubId);
        Task UpdateAsync(Club club);
    }
}