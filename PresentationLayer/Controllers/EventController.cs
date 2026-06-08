using BussinessLayer.DTOs;
using BussinessLayer.Interfaces;
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

        [HttpPost]
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
                    data = result
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

        [HttpGet("{eventId}")]
        public async Task<IActionResult> GetById(long eventId)
        {
            var result = await _service.GetEventByIdAsync(eventId);

            if (result == null)
                return NotFound(new { message = "Không tìm thấy sự kiện." });

            return Ok(result);
        }

        [HttpGet("club/{clubId}")]
        public async Task<IActionResult> GetByClub(long clubId)
        {
            var result = await _service.GetEventsByClubAsync(clubId);
            return Ok(result);
        }

        [HttpGet("club/{clubId}/approved")]
        public async Task<IActionResult> GetApprovedByClub(long clubId)
        {
            var result = await _service.GetApprovedEventsByClubAsync(clubId);
            return Ok(result);
        }

        [HttpPut("{eventId}")]
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
                    data = result
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

        [HttpPut("{eventId}/cancel")]
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

        /// <summary>[Admin] Xem danh sách tất cả sự kiện (có thể lọc theo status)</summary>
        [HttpGet]
        public async Task<IActionResult> GetAllEvents([FromQuery] string? status)
        {
            var events = await _service.GetAllEventsAsync(status);
            return Ok(new
            {
                total = events.Count,
                data = events.Select(e => new
                {
                    e.Eventid, e.Eventname, e.Location, e.Starttime, e.Endtime, e.Status,
                    club = new { e.Club?.Clubid, e.Club?.Clubname }
                })
            });
        }

        /// <summary>[Admin] Duyệt sự kiện — kiểm tra trùng địa điểm + thời gian</summary>
        [HttpPut("{eventId}/approve")]
        public async Task<IActionResult> ApproveEvent(long eventId)
        {
            try
            {
                var r = await _service.ApproveEventAsync(eventId);
                return Ok(new
                {
                    message = $"Sự kiện '{r.Eventname}' đã được duyệt.",
                    data = new { r.Eventid, r.Eventname, r.Location, r.Starttime, r.Endtime, r.Status }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, inner = ex.InnerException?.Message });
            }
        }

        /// <summary>[Admin] Từ chối sự kiện kèm lý do</summary>
        [HttpPut("{eventId}/reject")]
        public async Task<IActionResult> RejectEvent(long eventId, [FromBody] RejectEventDto dto)
        {
            try
            {
                var r = await _service.RejectEventAsync(eventId, dto);
                return Ok(new
                {
                    message = $"Sự kiện '{r.Eventname}' đã bị từ chối.",
                    rejectReason = dto.RejectReason,
                    data = new { r.Eventid, r.Eventname, r.Status }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, inner = ex.InnerException?.Message });
            }
        }
    }
}