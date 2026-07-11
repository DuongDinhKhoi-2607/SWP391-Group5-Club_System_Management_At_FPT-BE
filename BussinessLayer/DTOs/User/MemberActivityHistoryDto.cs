using System;
using System.Collections.Generic;

namespace BussinessLayer.DTOs.User
{
    public class MemberActivityHistoryDto
    {
        public List<MemberClubActivityDto> ClubHistory { get; set; } = new();
        public List<MemberEventActivityDto> EventHistory { get; set; } = new();
    }

    public class MemberClubActivityDto
    {
        public long ClubId { get; set; }
        public string ClubName { get; set; } = null!;
        public string ClubCode { get; set; } = null!;
        public string? LogoImage { get; set; }
        public DateOnly JoinDate { get; set; }
        public DateOnly? LeftDate { get; set; }
        public string Status { get; set; } = null!;
        public string? PersonalGoal { get; set; }
        public string? JoinReason { get; set; }
        public List<MemberPositionHistoryDto> Positions { get; set; } = new();
    }

    public class MemberPositionHistoryDto
    {
        public string Position { get; set; } = null!;
        public DateTime AppointedAt { get; set; }
        public decimal? KpiScore { get; set; }
        public string BoardName { get; set; } = null!;
    }

    public class MemberEventActivityDto
    {
        public long EventId { get; set; }
        public string EventName { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string RoleInEvent { get; set; } = null!;
        public string AttendanceStatus { get; set; } = null!;
        public DateTime? CheckedInAt { get; set; }
        public decimal? EvaluationScore { get; set; }
        public string? Feedback { get; set; }
        public string ClubName { get; set; } = null!;
    }
}
