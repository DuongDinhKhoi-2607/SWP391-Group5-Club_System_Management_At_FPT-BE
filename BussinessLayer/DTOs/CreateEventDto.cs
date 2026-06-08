using System;
using System.ComponentModel.DataAnnotations;

namespace BussinessLayer.DTOs
{
    public class CreateEventDto
    {
        [Required]
        public long ClubId { get; set; }

        [Required]
        public string EventName { get; set; } = null!;

        public string? Description { get; set; }

        public string? Location { get; set; }

        public string? PlanBudget { get; set; }

        public int TargetParticipants { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }
    }
}