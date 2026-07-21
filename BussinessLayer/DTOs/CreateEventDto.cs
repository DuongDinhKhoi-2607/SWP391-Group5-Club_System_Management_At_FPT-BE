using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace BussinessLayer.DTOs
{
    public class CreateEventDto
    {
        public long ClubId { get; set; }
        public string EventName { get; set; } = null!;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public string? PlanBudget { get; set; }
        public int TargetParticipants { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }

        public List<IFormFile>? Files { get; set; }
    }
}