using System.Security.Claims;
using BussinessLayer.DTOs;
using BussinessLayer.Interfaces;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Authorization;
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

        [HttpGet("count/total")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetTotalEvents() => Ok(await _service.GetTotalEventsAsync(null));

        [HttpGet("count/pending")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetPendingEvents() => Ok(await _service.GetTotalEventsAsync("Lập kế hoạch"));

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
        [Authorize]   // Phải đăng nhập (Leader / Member)
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateEvent([FromForm] CreateEventDto dto)
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!long.TryParse(userIdStr, out long userId))
                    return Unauthorized(new { message = "Token không hợp lệ hoặc thiếu userId." });

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
        [Authorize(Roles = "ADMIN")]   // Chỉ ADMIN xem tất cả sự kiện
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
        [Authorize(Roles = "ADMIN")]   // Chỉ ADMIN duyệt sự kiện
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
        [Authorize(Roles = "ADMIN")]   // Chỉ ADMIN từ chối sự kiện
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

        // ─────────────────────────────────────────────────────────────
        // POST /api/events/{eventId}/register
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// [MEMBER] Đăng ký tham gia sự kiện (Không có file).
        /// </summary>
        [HttpPost("{eventId:long}/register")]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> RegisterEvent(long eventId, [FromBody] RegisterEventRequestDto dto)
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!long.TryParse(userIdStr, out long currentUserId))
                    return Unauthorized(new { message = "Token không hợp lệ hoặc thiếu userId." });

                var result = await _service.RegisterParticipantAsync(currentUserId, eventId, dto);

                return Ok(new
                {
                    message = "Đăng ký tham gia sự kiện thành công.",
                    data = new
                    {
                        participantId = result.Participantid,
                        eventId = result.Eventid,
                        userId = result.Userid,
                        role = result.Roleinevent,
                        status = result.Attendancestatus
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────
        // POST /api/events/{eventId}/evidence
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// [MEMBER] Upload minh chứng cho sự kiện đã đăng ký tham gia (Nhiều file).
        /// </summary>
        [HttpPost("{eventId:long}/evidence")]
        [Authorize(Roles = "Member")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadEvidence(long eventId, [FromForm] UploadEventEvidenceDto dto)
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!long.TryParse(userIdStr, out long currentUserId))
                    return Unauthorized(new { message = "Token không hợp lệ hoặc thiếu userId." });

                var result = await _service.UploadEvidenceAsync(currentUserId, eventId, dto);

                return Ok(new
                {
                    message = "Upload minh chứng thành công.",
                    data = new
                    {
                        participantId = result.Participantid,
                        eventId = result.Eventid,
                        userId = result.Userid,
                        evidencesCount = result.Evidences?.Count ?? 0,
                        feedback = result.Feedback
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}