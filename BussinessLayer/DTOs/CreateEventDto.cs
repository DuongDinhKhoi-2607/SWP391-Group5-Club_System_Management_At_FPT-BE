using System;

namespace BussinessLayer.DTOs
{
    /// <summary>Manager gửi để tạo sự kiện chờ Admin duyệt</summary>
    public class CreateEventDto
    {
        public long ClubId { get; set; }
        public string EventName { get; set; } = null!;
        public string? Description { get; set; }

        /// <summary>Địa điểm tổ chức (dùng để kiểm tra trùng khi duyệt)</summary>
        public string Location { get; set; } = null!;

        public string? PlanBudget { get; set; }
        public int TargetParticipants { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
