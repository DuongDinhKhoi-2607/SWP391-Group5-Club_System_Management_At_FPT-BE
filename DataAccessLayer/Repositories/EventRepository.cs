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

        public async Task<int> CountEventsAsync(string? statusFilter)
        {
            var query = _context.Events.AsQueryable();
            if (!string.IsNullOrEmpty(statusFilter))
                query = query.Where(e => e.Status == statusFilter);
            return await query.CountAsync();
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

        public async Task<Event?> GetConflictByLocationAsync(long excludeEventId, string location, DateTime startTime, DateTime endTime)
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

        public async Task UpdateAsync(Event ev)
        {
            _context.Events.Update(ev);
            await _context.SaveChangesAsync();
        }

        public async Task<Participant?> GetParticipantAsync(long eventId, long userId)
        {
            return await _context.Participants
                .FirstOrDefaultAsync(p => p.Eventid == eventId && p.Userid == userId);
        }

        public async Task<Participant> AddParticipantAsync(Participant participant)
        {
            _context.Participants.Add(participant);
            await _context.SaveChangesAsync();
            return participant;
        }

        public async Task UpdateParticipantAsync(Participant participant)
        {
            _context.Participants.Update(participant);
            await _context.SaveChangesAsync();
        }

        public async Task<int> CountParticipantsAsync(long eventId)
        {
            return await _context.Participants
                .Where(p => p.Eventid == eventId)
                .CountAsync();
        }

        public async Task<bool> IsUserInClubAsync(long userId, long clubId)
        {
            return await _context.Memberships
                .AnyAsync(m => m.Userid == userId && m.Clubid == clubId && m.Status == "Đang sinh hoạt");
        }

        public async Task<Evidence?> GetEvidenceByIdAsync(long evidenceId)
        {
            return await _context.Evidences
                .Include(e => e.Participant)
                .ThenInclude(p => p.Event)
                .FirstOrDefaultAsync(e => e.Evidenceid == evidenceId);
        }

        public async Task UpdateEvidenceAsync(Evidence evidence)
        {
            _context.Evidences.Update(evidence);
            await _context.SaveChangesAsync();
        }


    }
}