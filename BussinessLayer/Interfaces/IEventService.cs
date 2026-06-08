using BussinessLayer.DTOs;
using DataAccessLayer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BussinessLayer.Interfaces
{
    public interface IEventService
    {
        Task<Event> CreateEventAsync(CreateEventDto dto);
        Task<List<Event>> GetAllEventsAsync(string? statusFilter);
        Task<Event> ApproveEventAsync(long eventId);
        Task<Event> RejectEventAsync(long eventId, RejectEventDto dto);
    }
}
