using System;

namespace BussinessLayer.DTOs.Semester;

public class SemesterResponseDto
{
    public long SemesterId { get; set; }
    public string SemesterName { get; set; } = null!;
    public string? Description { get; set; }
    public string Status { get; set; } = null!;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}
