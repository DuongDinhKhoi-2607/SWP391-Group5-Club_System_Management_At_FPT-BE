using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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

        public async Task<Event> CreateAsync(Event newEvent)
        {
            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();
            return newEvent;
        }

        public async Task<Event?> GetByIdAsync(long eventId)
        {
            return await _context.Events
                .Include(e => e.Club)
                .FirstOrDefaultAsync(e => e.Eventid == eventId);
        }

        public async Task<List<Event>> GetAllAsync(string? statusFilter)
        {
            var query = _context.Events.Include(e => e.Club).AsQueryable();

            if (!string.IsNullOrEmpty(statusFilter))
                query = query.Where(e => e.Status == statusFilter);

            return await query.OrderByDescending(e => e.Starttime).ToListAsync();
        }

        /// <summary>
        /// Tìm sự kiện ĐÃ DUYỆT có cùng địa điểm và thời gian bị chồng lấp.
        /// Điều kiện overlap: existingStart &lt; newEnd AND existingEnd &gt; newStart
        /// </summary>
        public async Task<Event?> GetConflictByLocationAsync(
            long excludeEventId, string location, DateTime startTime, DateTime endTime)
        {
            return await _context.Events
                .Include(e => e.Club)
                .Where(e =>
                    e.Eventid != excludeEventId &&
                    (e.Status == "Đã duyệt" || e.Status == "Đang diễn ra") &&
                    e.Location == location &&
                    e.Starttime < endTime &&
                    e.Endtime > startTime)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Tìm sự kiện ĐÃ DUYỆT của cùng CLB có thời gian bị chồng lấp.
        /// </summary>
        public async Task<Event?> GetConflictByClubAsync(
            long excludeEventId, long clubId, DateTime startTime, DateTime endTime)
        {
            return await _context.Events
                .Where(e =>
                    e.Eventid != excludeEventId &&
                    (e.Status == "Đã duyệt" || e.Status == "Đang diễn ra") &&
                    e.Clubid == clubId &&
                    e.Starttime < endTime &&
                    e.Endtime > startTime)
                .FirstOrDefaultAsync();
        }

        public async Task<Event> UpdateStatusAsync(Event ev, string newStatus)
        {
            try
            {
                ev.Status = newStatus;
                await _context.SaveChangesAsync();
                return ev;
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? ex.Message;
                throw new Exception($"Lỗi khi cập nhật status '{newStatus}': {inner}", ex);
            }
        }
    }
}
