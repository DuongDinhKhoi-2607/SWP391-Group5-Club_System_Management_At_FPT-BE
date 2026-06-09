namespace BussinessLayer.DTOs
{
    public class UpdateDocumentDto
    {
        public string DocumentName { get; set; } = null!;
        public long DocumentTypeId { get; set; }
        public long? EventId { get; set; }
        public string AccessLevel { get; set; } = "Nội bộ";
    }
}