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
        [Authorize(Roles = "ADMIN,Manager")]
        public async Task<IActionResult> GetTotalEvents() => Ok(await _service.GetTotalEventsAsync(null));

        [HttpGet("count/pending")]
        [Authorize(Roles = "ADMIN,Manager")]
        public async Task<IActionResult> GetPendingEvents() => Ok(await _service.GetTotalEventsAsync("Chờ duyệt"));

        private static EventResponseDto MapToResponse(Event e)
        {
            var vnZone = TimeSpan.FromHours(7);
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
                StartTime = new DateTimeOffset(e.Starttime, vnZone),
                EndTime = new DateTimeOffset(e.Endtime, vnZone),

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
        [Authorize]
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
        [Authorize]
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
        [Authorize(Roles = "ADMIN,Manager")]   // ADMIN và Manager xem tất cả sự kiện
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
        [Authorize(Roles = "ADMIN,Manager")]   // ADMIN và Manager duyệt sự kiện
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
        [Authorize(Roles = "ADMIN,Manager")]   // ADMIN và Manager từ chối sự kiện
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

        [HttpPut("request-edit/{eventId}")]
        [Authorize(Roles = "ADMIN,Manager")]   // ADMIN và Manager yêu cầu chỉnh sửa sự kiện
        public async Task<IActionResult> RequestEditEvent(
            long eventId,
            [FromBody] RejectEventDto dto) // Sử dụng chung DTO với RejectEvent vì cùng yêu cầu lý do
        {
            try
            {
                var result = await _service.RequestEditEventAsync(eventId, dto.RejectReason);

                return Ok(new
                {
                    message = $"Yêu cầu chỉnh sửa sự kiện '{result.Eventname}' thành công.",
                    reason = dto.RejectReason,
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
                var msg = ex.InnerException != null ? $"{ex.Message} Inner: {ex.InnerException.Message}" : ex.Message;
                return BadRequest(new { message = msg });
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

        // ─────────────────────────────────────────────────────────────
        // PATCH /api/events/evidence/{evidenceId}/review
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// [ADMIN,Manager] Kiểm tra evidence tổng hợp của sự kiện; xác nhận evidence hợp lệ hoặc yêu cầu bổ sung
        /// </summary>
        [HttpPatch("evidence/{evidenceId:long}/review")]
        [Authorize(Roles = "ADMIN,Manager")]
        public async Task<IActionResult> ReviewEvidence(long evidenceId, [FromBody] ReviewEvidenceDto dto)
        {
            try
            {
                var result = await _service.ReviewEvidenceAsync(evidenceId, dto.Status);

                return Ok(new
                {
                    message = $"Đã cập nhật trạng thái minh chứng thành '{result.Isverified}'.",
                    data = new
                    {
                        evidenceId = result.Evidenceid,
                        isVerified = result.Isverified,
                        verifiedAt = result.Verifiedat
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────
        // GET /api/events/{eventId}/evidences
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// [Leader, ADMIN, Manager] Lấy danh sách evidence theo sự kiện.
        /// </summary>
        [HttpGet("{eventId:long}/evidences")]
        [Authorize]
        public async Task<IActionResult> GetEvidencesByEvent(long eventId)
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!long.TryParse(userIdStr, out long currentUserId))
                    return Unauthorized(new { message = "Token không hợp lệ hoặc thiếu userId." });

                bool isAdminOrManager = User.IsInRole("ADMIN") || User.IsInRole("Manager");

                var evidences = await _service.GetEvidencesByEventAsync(currentUserId, eventId, isAdminOrManager);
                return Ok(new
                {
                    message = "Lấy danh sách evidence thành công.",
                    total = evidences.Count,
                    data = evidences
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────
        // GET /api/events/evidences/pending
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// [ADMIN,Manager] Lấy tất cả evidence đang chờ duyệt trong toàn hệ thống.
        /// </summary>
        [HttpGet("evidences/pending")]
        [Authorize(Roles = "ADMIN,Manager")]
        public async Task<IActionResult> GetPendingEvidences()
        {
            try
            {
                var evidences = await _service.GetPendingEvidencesAsync();
                return Ok(new
                {
                    message = "Lấy danh sách evidence chờ duyệt thành công.",
                    total = evidences.Count,
                    data = evidences
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────
        // PATCH /api/events/evidence/{evidenceId}/leader-review
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// [Leader] Kiểm tra evidence của thành viên tham gia sự kiện.
        /// </summary>
        [HttpPatch("evidence/{evidenceId:long}/leader-review")]
        [Authorize]
        public async Task<IActionResult> ReviewEvidenceByLeader(long evidenceId, [FromBody] ReviewEvidenceDto dto)
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!long.TryParse(userIdStr, out long currentUserId))
                    return Unauthorized(new { message = "Token không hợp lệ hoặc thiếu userId." });

                var result = await _service.ReviewEvidenceByLeaderAsync(currentUserId, evidenceId, dto.Status);

                return Ok(new
                {
                    message = $"Đã cập nhật trạng thái minh chứng thành '{result.Isverified}'.",
                    data = new
                    {
                        evidenceId = result.Evidenceid,
                        isVerified = result.Isverified,
                        verifiedAt = result.Verifiedat
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────
        // GET /api/events/evidences/pending-leader
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// [Leader] Lấy tất cả evidence đang chờ Leader duyệt của một CLB.
        /// </summary>
        [HttpGet("evidences/pending-leader")]
        [Authorize]
        public async Task<IActionResult> GetPendingEvidencesForLeader([FromQuery] long clubId)
        {
            try
            {
                // Ở đây ta nhận clubId từ query params để xác định đang xem cho CLB nào.
                var evidences = await _service.GetPendingEvidencesForLeaderAsync(clubId);
                return Ok(new
                {
                    message = "Lấy danh sách evidence chờ Leader duyệt thành công.",
                    total = evidences.Count,
                    data = evidences
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────
        // GET /api/events/{eventId}/participants
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// [Leader, ADMIN, Manager] Xem danh sách thành viên tham gia sự kiện.
        /// </summary>
        [HttpGet("{eventId:long}/participants")]
        [Authorize]
        public async Task<IActionResult> GetParticipantsByEvent(long eventId)
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!long.TryParse(userIdStr, out long currentUserId))
                    return Unauthorized(new { message = "Token không hợp lệ hoặc thiếu userId." });

                bool isAdminOrManager = User.IsInRole("ADMIN") || User.IsInRole("Manager");

                var participants = await _service.GetParticipantsByEventAsync(currentUserId, eventId, isAdminOrManager);
                return Ok(new
                {
                    message = "Lấy danh sách tham gia thành công.",
                    total = participants.Count,
                    data = participants
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}