using System;
using System.ComponentModel.DataAnnotations;

namespace BussinessLayer.DTOs.ReportPeriod;

public class CreateReportPeriodRequestDto
{
    [Required]
    public long SemesterId { get; set; }

    [Required(ErrorMessage = "Period name is required")]
    [StringLength(200)]
    public string PeriodName { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public DateTime Deadline { get; set; }
}
