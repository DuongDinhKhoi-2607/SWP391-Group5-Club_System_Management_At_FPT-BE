using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BussinessLayer.DTOs
{
    public class UploadEventEvidenceDto
    {
        [Required(ErrorMessage = "Phải có ít nhất 1 file minh chứng.")]
        public List<IFormFile> EvidenceFiles { get; set; } = new List<IFormFile>();

        [StringLength(1000, ErrorMessage = "Feedback không vượt quá 1000 ký tự")]
        public string? Feedback { get; set; }
    }
}
