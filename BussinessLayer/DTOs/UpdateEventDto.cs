namespace BussinessLayer.DTOs
{
    public class UpdateEventDto
    {
        public string EventName { get; set; } = null!;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public string? PlanBudget { get; set; }
        public int TargetParticipants { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
    }
}