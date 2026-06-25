using BussinessLayer.DTOs.ReportPeriod;
using BussinessLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers;

[Route("api/report-periods")]
[ApiController]
[Authorize] // Require authentication for all endpoints
public class ReportPeriodController : ControllerBase
{
    private readonly IReportPeriodService _reportPeriodService;

    public ReportPeriodController(IReportPeriodService reportPeriodService)
    {
        _reportPeriodService = reportPeriodService;
    }

    [HttpGet("reports/count/pending")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> GetPendingReportsCount() => Ok(await _reportPeriodService.GetPendingReportsCountAsync());

    // GET /api/report-periods
    // Filter by semesterId: GET /api/report-periods?semesterId=1
    [HttpGet]
    public async Task<IActionResult> GetAllReportPeriods([FromQuery] long? semesterId)
    {
        try
        {
            var periods = await _reportPeriodService.GetAllReportPeriodsAsync(semesterId);
            return Ok(periods);
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            return BadRequest(new { message = msg, stackTrace = ex.StackTrace });
        }
    }

    // POST /api/report-periods
    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> CreateReportPeriod([FromBody] CreateReportPeriodRequestDto requestDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var period = await _reportPeriodService.CreateReportPeriodAsync(requestDto);
            return Ok(period);
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            return BadRequest(new { message = msg, stackTrace = ex.StackTrace });
        }
    }

    // PUT /api/report-periods/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> UpdateReportPeriod(long id, [FromBody] UpdateReportPeriodRequestDto requestDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var period = await _reportPeriodService.UpdateReportPeriodAsync(id, requestDto);
            return Ok(period);
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            return BadRequest(new { message = msg, stackTrace = ex.StackTrace });
        }
    }
}
