using DataAccessLayer.Models;
using System.Threading.Tasks;

namespace DataAccessLayer.Repositories
{
    public interface IEventRepository
    {
        Task<bool> CanCreateEventAsync(long userId, long clubId);
        Task<Event> CreateAsync(Event newEvent);
    }
}