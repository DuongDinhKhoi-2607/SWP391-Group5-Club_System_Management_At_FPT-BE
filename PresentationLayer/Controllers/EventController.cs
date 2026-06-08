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
    }
}