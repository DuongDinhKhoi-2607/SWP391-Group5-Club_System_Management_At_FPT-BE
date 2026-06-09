namespace BussinessLayer.DTOs
{
    public class EventResponseDto
    {
        public long EventId { get; set; }
        public long ClubId { get; set; }

        public string EventName { get; set; } = null!;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public string? PlanBudget { get; set; }

        public int TargetParticipants { get; set; }
        public int ActualParticipants { get; set; }

        public string Status { get; set; } = null!;

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public EventClubDto? Club { get; set; }
    }

    public class EventClubDto
    {
        public long ClubId { get; set; }
        public string ClubCode { get; set; } = null!;
        public string ClubName { get; set; } = null!;
        public string? Description { get; set; }
        public string? LogoImage { get; set; }
        public string? FanpageUrl { get; set; }
        public string Status { get; set; } = null!;
    }
}