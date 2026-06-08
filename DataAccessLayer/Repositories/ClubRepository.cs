using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories
{
    public class ClubRepository : IClubRepository
    {
        private readonly ClubSystemDbContext _context;

        public ClubRepository(ClubSystemDbContext context)
        {
            _context = context;
        }

        public async Task<Club?> GetByIdAsync(long clubId)
        {
            return await _context.Clubs.FindAsync(clubId);
        }

        public async Task<bool> IsLeaderOfClubAsync(long userId, long clubId)
        {
            return await _context.Boardmembers
                .Include(bm => bm.Membership)
                .Include(bm => bm.Board)
                .AnyAsync(bm =>
                    bm.Membership.Userid == userId &&
                    bm.Membership.Clubid == clubId &&
                    bm.Membership.Status == "Đang sinh hoạt" &&
                    bm.Board.Clubid == clubId &&
                    bm.Board.Status == "Đang đương nhiệm" &&
                    bm.Position == "Leader");
        }

        public async Task UpdateAsync(Club club)
        {
            _context.Clubs.Update(club);
            await _context.SaveChangesAsync();
        }
    }
}