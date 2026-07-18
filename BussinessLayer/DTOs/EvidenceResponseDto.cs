using System;

namespace BussinessLayer.DTOs
{
    public class EvidenceResponseDto
    {
        public long EvidenceId { get; set; }
        public long ParticipantId { get; set; }
        public long EventId { get; set; }
        public string EvidenceName { get; set; } = null!;
        public string FileUrl { get; set; } = null!;
        public string IsVerified { get; set; } = null!;
        public DateTime UploadedAt { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string ParticipantName { get; set; } = null!;

        // ── Bổ sung cho Evidence Listing ──
        public string? EventName { get; set; }
        public string? ClubName { get; set; }
        public long? ClubId { get; set; }
    }
}
