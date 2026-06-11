using System;
using BussinessLayer.DTOs.Semester;

namespace BussinessLayer.DTOs.ReportPeriod;

public class ReportPeriodResponseDto
{
    public long ReportPeriodId { get; set; }
    public long SemesterId { get; set; }
    public string PeriodName { get; set; } = null!;
    public string? Description { get; set; }
    public string Status { get; set; } = null!;
    public DateTime Deadline { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public SemesterResponseDto? Semester { get; set; }
}
