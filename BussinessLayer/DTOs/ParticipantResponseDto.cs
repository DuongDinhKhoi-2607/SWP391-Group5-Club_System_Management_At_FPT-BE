using System;

namespace BussinessLayer.DTOs
{
    public class ParticipantResponseDto
    {
        public long ParticipantId { get; set; }
        public long EventId { get; set; }
        public long UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? StudentId { get; set; } // Mã sinh viên, tuỳ chọn
        public string RoleInEvent { get; set; } = null!;
        public string AttendanceStatus { get; set; } = null!;
        public string? Feedback { get; set; }
        public decimal? EvaluationScore { get; set; }
        public DateTime? CheckedInAt { get; set; }
    }
}
