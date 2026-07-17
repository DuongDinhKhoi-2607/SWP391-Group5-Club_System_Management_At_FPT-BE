using BussinessLayer.DTOs;
using DataAccessLayer.Models;

namespace BussinessLayer.Interfaces
{
    public interface IEventService
    {
        Task<Event> CreateEventAsync(CreateEventDto dto, long currentUserId);
        Task<List<Event>> GetAllEventsAsync(string? statusFilter);
        Task<int> GetTotalEventsAsync(string? statusFilter);
        Task<Event?> GetEventByIdAsync(long eventId);
        Task<List<Event>> GetEventsByClubAsync(long clubId);
        Task<List<Event>> GetApprovedEventsByClubAsync(long clubId);
        Task<Event> UpdateEventAsync(long eventId, UpdateEventDto dto);
        Task<Event> ApproveEventAsync(long eventId);
        Task<Event> RejectEventAsync(long eventId, RejectEventDto dto);
        Task<Event> RequestEditEventAsync(long eventId, string reason);
        Task CancelEventAsync(long eventId);
        Task<Participant> RegisterParticipantAsync(long userId, long eventId, RegisterEventRequestDto dto);
        Task<Participant> UploadEvidenceAsync(long userId, long eventId, UploadEventEvidenceDto dto);
        Task<Evidence> ReviewEvidenceAsync(long evidenceId, string status);

    }
}