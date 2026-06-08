using BussinessLayer.DTOs;
using DataAccessLayer.Models;
using System.Threading.Tasks;

namespace BussinessLayer.Interfaces
{
    public interface IEventService
    {
        Task<Event> CreateEventAsync(CreateEventDto dto, long currentUserId);
    }
}