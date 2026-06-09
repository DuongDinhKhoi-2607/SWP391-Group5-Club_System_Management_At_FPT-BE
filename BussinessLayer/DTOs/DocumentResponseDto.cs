namespace BussinessLayer.DTOs
{
    public class DocumentResponseDto
    {
        public long DocumentId { get; set; }
        public long ClubId { get; set; }
        public long DocumentTypeId { get; set; }
        public long? EventId { get; set; }

        public string DocumentName { get; set; } = null!;
        public string FileUrl { get; set; } = null!;
        public long FileSize { get; set; }
        public int DownloadCount { get; set; }
        public string AccessLevel { get; set; } = null!;
        public DateTime UploadedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string? ClubName { get; set; }
        public string? DocumentTypeName { get; set; }
        public string? EventName { get; set; }
    }
}