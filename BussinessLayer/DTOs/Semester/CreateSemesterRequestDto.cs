using System;
using System.ComponentModel.DataAnnotations;

namespace BussinessLayer.DTOs.Semester;

public class CreateSemesterRequestDto
{
    [Required(ErrorMessage = "Semester name is required")]
    [StringLength(100)]
    public string SemesterName { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public DateOnly StartDate { get; set; }

    [Required]
    public DateOnly EndDate { get; set; }
}
