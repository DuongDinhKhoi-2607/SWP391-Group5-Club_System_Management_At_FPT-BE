using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DataAccessLayer.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly ClubSystemDbContext _context;

        public EventRepository(ClubSystemDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CanCreateEventAsync(long userId, long clubId)
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
                    (bm.Position == "Leader" || bm.Position == "Mentor"));
        }

        public async Task<Event> CreateAsync(Event newEvent)
        {
            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();
            return newEvent;
        }
    }
}