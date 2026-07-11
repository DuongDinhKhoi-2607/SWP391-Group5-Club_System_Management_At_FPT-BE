using BussinessLayer.DTOs;
using DataAccessLayer.Models;

namespace BussinessLayer.Interfaces
{
    public interface IDocumentService
    {
        Task<List<Document>> UploadAsync(UploadDocumentDto dto, long currentUserId, string currentUserRole);
        Task<Document?> GetByIdAsync(long documentId);
        Task<List<Document>> GetByClubAsync(long clubId);
        Task<List<Document>> GetByEventAsync(long eventId);
        Task<List<Document>> GetByTypeAsync(long documentTypeId);
        Task<Document> UpdateAsync(long documentId, UpdateDocumentDto dto, long currentUserId, string currentUserRole);
        Task<Document> GetForDownloadAsync(long documentId);
        Task DeleteAsync(long documentId, long currentUserId, string currentUserRole);
    }
}