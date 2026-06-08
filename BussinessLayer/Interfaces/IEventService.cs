using BussinessLayer.DTOs;
using DataAccessLayer.Models;

namespace BussinessLayer.Interfaces
{
    public interface IEventService
    {
        Task<Event> CreateEventAsync(CreateEventDto dto, long currentUserId);
        Task<Event?> GetEventByIdAsync(long eventId);
        Task<List<Event>> GetEventsByClubAsync(long clubId);
        Task<List<Event>> GetApprovedEventsByClubAsync(long clubId);
        Task<Event> UpdateEventAsync(long eventId, UpdateEventDto dto);
        Task CancelEventAsync(long eventId);
    }
}