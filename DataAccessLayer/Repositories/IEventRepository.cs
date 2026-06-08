using DataAccessLayer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccessLayer.Repositories
{
    public interface IEventRepository
    {
        Task<Event> CreateAsync(Event newEvent);
        Task<Event?> GetByIdAsync(long eventId);
        Task<List<Event>> GetAllAsync(string? statusFilter);

        /// <summary>
        /// Kiểm tra xung đột địa điểm + thời gian:
        /// Có sự kiện đã duyệt nào khác tại cùng location và overlap thời gian không?
        /// </summary>
        Task<Event?> GetConflictByLocationAsync(long excludeEventId, string location, DateTime startTime, DateTime endTime);

        /// <summary>
        /// Kiểm tra xung đột cùng CLB + thời gian:
        /// CLB đó đã có sự kiện đã duyệt nào khác overlap thời gian không?
        /// </summary>
        Task<Event?> GetConflictByClubAsync(long excludeEventId, long clubId, DateTime startTime, DateTime endTime);

        Task<Event> UpdateStatusAsync(Event ev, string newStatus);
    }
}
