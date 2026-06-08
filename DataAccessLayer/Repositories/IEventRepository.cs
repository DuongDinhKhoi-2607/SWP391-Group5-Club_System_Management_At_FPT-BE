using DataAccessLayer.Models;

namespace DataAccessLayer.Repositories
{
    public interface IEventRepository
    {
        Task<Event> CreateAsync(Event newEvent);
        Task<Event?> GetByIdAsync(long eventId);
        Task<List<Event>> GetByClubIdAsync(long clubId);
        Task<List<Event>> GetApprovedByClubIdAsync(long clubId);
        Task<bool> IsEventTimeConflictAsync(long clubId, DateTime startTime, DateTime endTime, long? ignoreEventId = null);
        Task<bool> ExistsDuplicateEventAsync(long clubId, string eventName, DateTime startTime, long? ignoreEventId = null);
        Task UpdateAsync(Event ev);
    }
}