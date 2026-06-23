using BussinessLayer.DTOs.Semester;
using BussinessLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers;

[Route("api/semesters")]
[ApiController]
[Authorize] // Require authentication for all endpoints
public class SemesterController : ControllerBase
{
    private readonly ISemesterService _semesterService;

    public SemesterController(ISemesterService semesterService)
    {
        _semesterService = semesterService;
    }

    // GET /api/semesters
    [HttpGet]
    public async Task<IActionResult> GetAllSemesters()
    {
        try
        {
            var semesters = await _semesterService.GetAllSemestersAsync();
            return Ok(semesters);
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            return BadRequest(new { message = msg, stackTrace = ex.StackTrace });
        }
    }

    // POST /api/semesters
    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> CreateSemester([FromBody] CreateSemesterRequestDto requestDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var semester = await _semesterService.CreateSemesterAsync(requestDto);
            return Ok(semester);
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            return BadRequest(new { message = msg, stackTrace = ex.StackTrace });
        }
    }

    // PUT /api/semesters/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> UpdateSemester(long id, [FromBody] UpdateSemesterRequestDto requestDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var semester = await _semesterService.UpdateSemesterAsync(id, requestDto);
            return Ok(semester);
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            return BadRequest(new { message = msg, stackTrace = ex.StackTrace });
        }
    }
}
