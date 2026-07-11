using BussinessLayer.DTOs;
using BussinessLayer.Interfaces;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories;

namespace BussinessLayer.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _repo;
        private readonly IClubRepository _clubRepo;

        public DocumentService(IDocumentRepository repo, IClubRepository clubRepo)
        {
            _repo = repo;
            _clubRepo = clubRepo;
        }

        public async Task<List<Document>> UploadAsync(UploadDocumentDto dto, long currentUserId, string currentUserRole)
        {
            // Kiểm tra phân quyền: chỉ ADMIN hoặc Leader của CLB mới được phép upload
            var isAdmin = string.Equals(currentUserRole, "ADMIN", StringComparison.OrdinalIgnoreCase);
            var isLeader = await _clubRepo.IsLeaderOfClubAsync(currentUserId, dto.ClubId);
            if (!isAdmin && !isLeader)
                throw new UnauthorizedAccessException("Chỉ Leader của CLB hoặc Admin mới được phép tải lên tài liệu.");

            if (dto.Files == null || dto.Files.Count == 0)
                throw new Exception("Vui lòng chọn ít nhất 1 file.");

            if (dto.Files.Count > 5)
                throw new Exception("Chỉ được upload tối đa 5 file.");

            if (dto.AccessLevel != "Công khai" && dto.AccessLevel != "Nội bộ")
                throw new Exception("AccessLevel chỉ được là 'Công khai' hoặc 'Nội bộ'.");

            var allowedExtensions = new[]
            {
                ".pdf", ".doc", ".docx", ".xls", ".xlsx",
                ".ppt", ".pptx", ".png", ".jpg", ".jpeg", ".txt"
            };

            var uploadFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "documents"
            );

            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            var result = new List<Document>();

            foreach (var file in dto.Files)
            {
                if (file.Length <= 0)
                    continue;

                if (file.Length > 10 * 1024 * 1024)
                    throw new Exception($"File {file.FileName} vượt quá 10MB.");

                var extension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                    throw new Exception($"File {file.FileName} không được hỗ trợ.");

                var storedFileName = $"{Guid.NewGuid()}{extension}";
                var fullPath = Path.Combine(uploadFolder, storedFileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var document = new Document
                {
                    Clubid = dto.ClubId,
                    Documenttypeid = dto.DocumentTypeId,
                    Eventid = dto.EventId,
                    Documentname = file.FileName,
                    Fileurl = $"/uploads/documents/{storedFileName}",
                    Filesize = file.Length,
                    Downloadcount = 0,
                    Accesslevel = dto.AccessLevel,
                    Uploadedat = DateTime.Now
                };

                var created = await _repo.CreateAsync(document);
                result.Add(created);
            }

            return result;
        }

        public async Task<Document?> GetByIdAsync(long documentId)
        {
            return await _repo.GetByIdAsync(documentId);
        }

        public async Task<List<Document>> GetByClubAsync(long clubId)
        {
            return await _repo.GetByClubAsync(clubId);
        }

        public async Task<List<Document>> GetByEventAsync(long eventId)
        {
            return await _repo.GetByEventAsync(eventId);
        }

        public async Task<List<Document>> GetByTypeAsync(long documentTypeId)
        {
            return await _repo.GetByTypeAsync(documentTypeId);
        }

        public async Task<Document> UpdateAsync(long documentId, UpdateDocumentDto dto, long currentUserId, string currentUserRole)
        {
            var document = await _repo.GetByIdAsync(documentId);

            if (document == null)
                throw new Exception("Không tìm thấy tài liệu.");

            // Kiểm tra phân quyền: chỉ ADMIN hoặc Leader của CLB mới được phép cập nhật
            var isAdmin = string.Equals(currentUserRole, "ADMIN", StringComparison.OrdinalIgnoreCase);
            var isLeader = await _clubRepo.IsLeaderOfClubAsync(currentUserId, document.Clubid);
            if (!isAdmin && !isLeader)
                throw new UnauthorizedAccessException("Chỉ Leader của CLB hoặc Admin mới được phép chỉnh sửa tài liệu.");

            if (dto.AccessLevel != "Công khai" && dto.AccessLevel != "Nội bộ")
                throw new Exception("AccessLevel chỉ được là 'Công khai' hoặc 'Nội bộ'.");

            document.Documentname = dto.DocumentName;
            document.Documenttypeid = dto.DocumentTypeId;
            document.Eventid = dto.EventId;
            document.Accesslevel = dto.AccessLevel;
            document.Updatedat = DateTime.Now;

            await _repo.UpdateAsync(document);

            return document;
        }

        public async Task<Document> GetForDownloadAsync(long documentId)
        {
            var document = await _repo.GetByIdAsync(documentId);

            if (document == null)
                throw new Exception("Không tìm thấy tài liệu.");

            document.Downloadcount += 1;
            await _repo.UpdateAsync(document);

            return document;
        }

        public async Task DeleteAsync(long documentId, long currentUserId, string currentUserRole)
        {
            var document = await _repo.GetByIdAsync(documentId);

            if (document == null)
                throw new Exception("Không tìm thấy tài liệu.");

            // Kiểm tra phân quyền: chỉ ADMIN hoặc Leader của CLB mới được phép xóa
            var isAdmin = string.Equals(currentUserRole, "ADMIN", StringComparison.OrdinalIgnoreCase);
            var isLeader = await _clubRepo.IsLeaderOfClubAsync(currentUserId, document.Clubid);
            if (!isAdmin && !isLeader)
                throw new UnauthorizedAccessException("Chỉ Leader của CLB hoặc Admin mới được phép xóa tài liệu.");

            var filePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                document.Fileurl.TrimStart('/')
            );

            if (File.Exists(filePath))
                File.Delete(filePath);

            await _repo.DeleteAsync(document);
        }
    }
}