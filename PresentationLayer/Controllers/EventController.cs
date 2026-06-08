using BussinessLayer.DTOs;
using BussinessLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

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

        /// <summary>
        /// [Manager] Tạo sự kiện mới — gửi lên Admin chờ duyệt.
        /// Status tự động = "Chờ duyệt"
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto dto)
        {
            try
            {
                var result = await _service.CreateEventAsync(dto);
                return Ok(new
                {
                    message = "Tạo sự kiện thành công. Đang chờ Admin duyệt.",
                    data = new
                    {
                        result.Eventid,
                        result.Eventname,
                        result.Location,
                        result.Starttime,
                        result.Endtime,
                        result.Status
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// [Admin] Xem danh sách sự kiện. Filter theo status nếu cần.
        /// Ví dụ: GET /api/events?status=Chờ duyệt
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllEvents([FromQuery] string? status)
        {
            var events = await _service.GetAllEventsAsync(status);
            return Ok(new
            {
                total = events.Count,
                data = events.Select(e => new
                {
                    e.Eventid,
                    e.Eventname,
                    e.Location,
                    e.Starttime,
                    e.Endtime,
                    e.Status,
                    club = new { e.Club.Clubid, e.Club.Clubname }
                })
            });
        }

        /// <summary>
        /// [Admin] Duyệt sự kiện.
        /// Tự động kiểm tra:
        ///   1. Trùng địa điểm + thời gian với sự kiện đã duyệt khác
        ///   2. Cùng CLB đã có sự kiện khác cùng thời gian
        /// </summary>
        [HttpPut("{id}/approve")]
        public async Task<IActionResult> ApproveEvent(long id)
        {
            try
            {
                var result = await _service.ApproveEventAsync(id);
                return Ok(new
                {
                    message = $"Sự kiện '{result.Eventname}' đã được duyệt thành công.",
                    data = new
                    {
                        result.Eventid,
                        result.Eventname,
                        result.Location,
                        result.Starttime,
                        result.Endtime,
                        result.Status
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// [Admin] Từ chối sự kiện kèm lý do.
        /// Body: { "rejectReason": "Lý do từ chối" }
        /// </summary>
        [HttpPut("{id}/reject")]
        public async Task<IActionResult> RejectEvent(long id, [FromBody] RejectEventDto dto)
        {
            try
            {
                var result = await _service.RejectEventAsync(id, dto);
                return Ok(new
                {
                    message = $"Sự kiện '{result.Eventname}' đã bị từ chối.",
                    rejectReason = dto.RejectReason,
                    data = new
                    {
                        result.Eventid,
                        result.Eventname,
                        result.Status
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
