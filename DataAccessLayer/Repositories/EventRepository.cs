using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly ClubSystemDbContext _context;

        public EventRepository(ClubSystemDbContext context)
        {
            _context = context;
        }

        public async Task<Event> CreateAsync(Event newEvent)
        {
            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();
            return newEvent;
        }

        public async Task<Event?> GetByIdAsync(long eventId)
        {
            return await _context.Events
                .FirstOrDefaultAsync(e => e.Eventid == eventId);
        }

        public async Task<List<Event>> GetByClubIdAsync(long clubId)
        {
            return await _context.Events
                .Where(e => e.Clubid == clubId)
                .OrderByDescending(e => e.Starttime)
                .ToListAsync();
        }

        public async Task<List<Event>> GetApprovedByClubIdAsync(long clubId)
        {
            return await _context.Events
                .Where(e =>
                    e.Clubid == clubId &&
                    (
                        e.Status == "Đã duyệt" ||
                        e.Status == "Đang diễn ra" ||
                        e.Status == "Đã kết thúc"
                    ))
                .OrderByDescending(e => e.Starttime)
                .ToListAsync();
        }

        public async Task<bool> IsEventTimeConflictAsync(
            long clubId,
            DateTime startTime,
            DateTime endTime,
            long? ignoreEventId = null)
        {
            return await _context.Events.AnyAsync(e =>
                e.Clubid == clubId &&
                e.Status != "Đã hủy" &&
                e.Status != "Bị từ chối" &&
                (!ignoreEventId.HasValue || e.Eventid != ignoreEventId.Value) &&
                startTime < e.Endtime &&
                endTime > e.Starttime);
        }

        public async Task<bool> ExistsDuplicateEventAsync(
            long clubId,
            string eventName,
            DateTime startTime,
            long? ignoreEventId = null)
        {
            return await _context.Events.AnyAsync(e =>
                e.Clubid == clubId &&
                e.Eventname.ToLower() == eventName.ToLower() &&
                e.Starttime == startTime &&
                (!ignoreEventId.HasValue || e.Eventid != ignoreEventId.Value));
        }

        public async Task UpdateAsync(Event ev)
        {
            _context.Events.Update(ev);
            await _context.SaveChangesAsync();
        }
    }
}