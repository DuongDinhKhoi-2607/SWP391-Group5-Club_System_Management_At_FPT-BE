using BussinessLayer.DTOs;
using BussinessLayer.Interfaces;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers
{
    [Route("api/events")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly IEventService _service;

        public EventController(IEventService service)
        {
            _service = service;
        }

        private static EventResponseDto MapToResponse(Event e)
        {
            return new EventResponseDto
            {
                EventId = e.Eventid,
                ClubId = e.Clubid,
                EventName = e.Eventname,
                Description = e.Description,
                Location = e.Location,
                PlanBudget = e.Planbudget,
                TargetParticipants = e.Targetparticipants,
                ActualParticipants = e.Actualparticipants,
                Status = e.Status,
                StartTime = e.Starttime,
                EndTime = e.Endtime,

                Club = e.Club == null ? null : new EventClubDto
                {
                    ClubId = e.Club.Clubid,
                    ClubCode = e.Club.Clubcode,
                    ClubName = e.Club.Clubname,
                    Description = e.Club.Description,
                    LogoImage = e.Club.Logoimage,
                    FanpageUrl = e.Club.Fanpageurl,
                    Status = e.Club.Status
                }
            };
        }

        [HttpPost("create")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateEvent([FromForm] CreateEventDto dto)
        {
            try
            {
                long userId = 2;

                var result = await _service.CreateEventAsync(dto, userId);

                return Ok(new
                {
                    message = "Gửi yêu cầu tạo sự kiện thành công. Vui lòng chờ admin duyệt.",
                    data = MapToResponse(result)
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }

        [HttpGet("detail/{eventId}")]
        public async Task<IActionResult> GetById(long eventId)
        {
            var result = await _service.GetEventByIdAsync(eventId);

            if (result == null)
                return NotFound(new { message = "Không tìm thấy sự kiện." });

            return Ok(MapToResponse(result));
        }

        [HttpGet("by-club/{clubId}")]
        public async Task<IActionResult> GetByClub(long clubId)
        {
            var result = await _service.GetEventsByClubAsync(clubId);

            return Ok(new
            {
                total = result.Count,
                data = result.Select(MapToResponse)
            });
        }

        [HttpGet("approved-by-club/{clubId}")]
        public async Task<IActionResult> GetApprovedByClub(long clubId)
        {
            var result = await _service.GetApprovedEventsByClubAsync(clubId);

            return Ok(new
            {
                total = result.Count,
                data = result.Select(MapToResponse)
            });
        }

        [HttpPut("update/{eventId}")]
        public async Task<IActionResult> UpdateEvent(
            long eventId,
            [FromBody] UpdateEventDto dto)
        {
            try
            {
                var result = await _service.UpdateEventAsync(eventId, dto);

                return Ok(new
                {
                    message = "Cập nhật sự kiện thành công.",
                    data = MapToResponse(result)
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }

        [HttpPut("cancel/{eventId}")]
        public async Task<IActionResult> CancelEvent(long eventId)
        {
            try
            {
                await _service.CancelEventAsync(eventId);

                return Ok(new
                {
                    message = "Hủy sự kiện thành công."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllEvents([FromQuery] string? status)
        {
            var events = await _service.GetAllEventsAsync(status);

            return Ok(new
            {
                total = events.Count,
                data = events.Select(MapToResponse)
            });
        }

        [HttpPut("approve/{eventId}")]
        public async Task<IActionResult> ApproveEvent(long eventId)
        {
            try
            {
                var result = await _service.ApproveEventAsync(eventId);

                return Ok(new
                {
                    message = $"Sự kiện '{result.Eventname}' đã được duyệt.",
                    data = MapToResponse(result)
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }

        [HttpPut("reject/{eventId}")]
        public async Task<IActionResult> RejectEvent(
            long eventId,
            [FromBody] RejectEventDto dto)
        {
            try
            {
                var result = await _service.RejectEventAsync(eventId, dto);

                return Ok(new
                {
                    message = $"Sự kiện '{result.Eventname}' đã bị từ chối.",
                    rejectReason = dto.RejectReason,
                    data = MapToResponse(result)
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }
    }
}