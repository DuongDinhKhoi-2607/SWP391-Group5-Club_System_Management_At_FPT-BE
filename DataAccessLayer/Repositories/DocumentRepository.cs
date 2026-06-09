using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly ClubSystemDbContext _context;

        public DocumentRepository(ClubSystemDbContext context)
        {
            _context = context;
        }

        public async Task<Document> CreateAsync(Document document)
        {
            _context.Documents.Add(document);
            await _context.SaveChangesAsync();
            return document;
        }

        public async Task<Document?> GetByIdAsync(long documentId)
        {
            return await _context.Documents
                .Include(d => d.Club)
                .Include(d => d.Documenttype)
                .Include(d => d.Event)
                .FirstOrDefaultAsync(d => d.Documentid == documentId);
        }

        public async Task<List<Document>> GetByClubAsync(long clubId)
        {
            return await _context.Documents
                .Include(d => d.Documenttype)
                .Include(d => d.Event)
                .Where(d => d.Clubid == clubId)
                .OrderByDescending(d => d.Uploadedat)
                .ToListAsync();
        }

        public async Task<List<Document>> GetByEventAsync(long eventId)
        {
            return await _context.Documents
                .Include(d => d.Documenttype)
                .Where(d => d.Eventid == eventId)
                .OrderByDescending(d => d.Uploadedat)
                .ToListAsync();
        }

        public async Task<List<Document>> GetByTypeAsync(long documentTypeId)
        {
            return await _context.Documents
                .Include(d => d.Club)
                .Include(d => d.Event)
                .Where(d => d.Documenttypeid == documentTypeId)
                .OrderByDescending(d => d.Uploadedat)
                .ToListAsync();
        }

        public async Task UpdateAsync(Document document)
        {
            _context.Documents.Update(document);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Document document)
        {
            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();
        }
    }
}