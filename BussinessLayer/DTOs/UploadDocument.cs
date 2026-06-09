using Microsoft.AspNetCore.Http;

namespace BussinessLayer.DTOs
{
    public class UploadDocumentDto
    {
        public long ClubId { get; set; }
        public long DocumentTypeId { get; set; }

        // Nếu tài liệu chung thì để null
        // Nếu tài liệu sự kiện thì truyền EventId
        public long? EventId { get; set; }

        public string AccessLevel { get; set; } = "Nội bộ";

        public List<IFormFile> Files { get; set; } = new();
    }
}