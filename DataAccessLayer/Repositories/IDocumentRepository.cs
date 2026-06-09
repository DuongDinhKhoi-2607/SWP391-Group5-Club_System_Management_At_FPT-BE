using DataAccessLayer.Models;

namespace DataAccessLayer.Repositories
{
    public interface IDocumentRepository
    {
        Task<Document> CreateAsync(Document document);
        Task<Document?> GetByIdAsync(long documentId);
        Task<List<Document>> GetByClubAsync(long clubId);
        Task<List<Document>> GetByEventAsync(long eventId);
        Task<List<Document>> GetByTypeAsync(long documentTypeId);
        Task UpdateAsync(Document document);
        Task DeleteAsync(Document document);
    }
}