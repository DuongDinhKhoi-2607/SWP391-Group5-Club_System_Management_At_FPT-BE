using DataAccessLayer.Models;

namespace DataAccessLayer.Repositories
{
    public interface IEventRepository
    {
        Task<Event> CreateAsync(Event newEvent);
        Task<List<Event>> GetAllAsync(string? statusFilter);
        Task<int> CountEventsAsync(string? statusFilter);
        Task<Event?> GetByIdAsync(long eventId);
        Task<List<Event>> GetByClubIdAsync(long clubId);
        Task<List<Event>> GetApprovedByClubIdAsync(long clubId);
        Task<bool> IsEventTimeConflictAsync(long clubId, DateTime startTime, DateTime endTime, long? ignoreEventId = null);
        Task<bool> ExistsDuplicateEventAsync(long clubId, string eventName, DateTime startTime, long? ignoreEventId = null);
        Task<Event?> GetConflictByLocationAsync(long excludeEventId, string location, DateTime startTime, DateTime endTime);
        Task UpdateAsync(Event ev);
        
        Task<Participant?> GetParticipantAsync(long eventId, long userId);
        Task<Participant> AddParticipantAsync(Participant participant);
        Task UpdateParticipantAsync(Participant participant);
        Task<int> CountParticipantsAsync(long eventId);
        Task<bool> IsUserInClubAsync(long userId, long clubId);
        Task<Evidence?> GetEvidenceByIdAsync(long evidenceId);
        Task UpdateEvidenceAsync(Evidence evidence);

        /// <summary>Lấy danh sách evidence theo sự kiện (bao gồm thông tin participant, event, club).</summary>
        Task<List<Evidence>> GetEvidencesByEventIdAsync(long eventId);

        /// <summary>Lấy tất cả evidence đang chờ duyệt (status = "Đang chờ").</summary>
        Task<List<Evidence>> GetPendingEvidencesAsync();

    }
}