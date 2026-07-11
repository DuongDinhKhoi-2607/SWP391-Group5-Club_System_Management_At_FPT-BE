using System.Security.Claims;
using BussinessLayer.DTOs;
using BussinessLayer.Interfaces;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers
{
    [Route("api/documents")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _service;

        public DocumentController(IDocumentService service)
        {
            _service = service;
        }

        private static DocumentResponseDto MapToDocumentResponse(Document d)
        {
            return new DocumentResponseDto
            {
                DocumentId = d.Documentid,
                ClubId = d.Clubid,
                DocumentTypeId = d.Documenttypeid,
                EventId = d.Eventid,

                DocumentName = d.Documentname,
                FileUrl = d.Fileurl,
                FileSize = d.Filesize,
                DownloadCount = d.Downloadcount,
                AccessLevel = d.Accesslevel,
                UploadedAt = d.Uploadedat,
                UpdatedAt = d.Updatedat,

                ClubName = d.Club?.Clubname,
                DocumentTypeName = d.Documenttype?.Typename,
                EventName = d.Event?.Eventname
            };
        }

        [HttpPost("upload")]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromForm] UploadDocumentDto dto)
        {
            try
            {
                var role = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirst("system_role")?.Value ?? "";
                var userIdStr = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!long.TryParse(userIdStr, out long currentUserId))
                    return Unauthorized(new { message = "Token không hợp lệ hoặc thiếu userId." });

                var result = await _service.UploadAsync(dto, currentUserId, role);

                return Ok(new
                {
                    message = "Upload tài liệu thành công.",
                    total = result.Count,
                    data = result.Select(MapToDocumentResponse)
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
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

        [HttpGet("detail/{documentId}")]
        public async Task<IActionResult> GetDetail(long documentId)
        {
            var result = await _service.GetByIdAsync(documentId);

            if (result == null)
                return NotFound(new { message = "Không tìm thấy tài liệu." });

            return Ok(MapToDocumentResponse(result));
        }

        [HttpGet("by-club/{clubId}")]
        public async Task<IActionResult> GetByClub(long clubId)
        {
            var result = await _service.GetByClubAsync(clubId);

            return Ok(new
            {
                total = result.Count,
                data = result.Select(MapToDocumentResponse)
            });
        }

        [HttpGet("by-event/{eventId}")]
        public async Task<IActionResult> GetByEvent(long eventId)
        {
            var result = await _service.GetByEventAsync(eventId);

            return Ok(new
            {
                total = result.Count,
                data = result.Select(MapToDocumentResponse)
            });
        }

        [HttpGet("by-type/{documentTypeId}")]
        public async Task<IActionResult> GetByType(long documentTypeId)
        {
            var result = await _service.GetByTypeAsync(documentTypeId);

            return Ok(new
            {
                total = result.Count,
                data = result.Select(MapToDocumentResponse)
            });
        }

        [HttpPut("update/{documentId}")]
        [Authorize]
        public async Task<IActionResult> Update(
            long documentId,
            [FromBody] UpdateDocumentDto dto)
        {
            try
            {
                var role = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirst("system_role")?.Value ?? "";
                var userIdStr = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!long.TryParse(userIdStr, out long currentUserId))
                    return Unauthorized(new { message = "Token không hợp lệ hoặc thiếu userId." });

                var result = await _service.UpdateAsync(documentId, dto, currentUserId, role);

                return Ok(new
                {
                    message = "Cập nhật tài liệu thành công.",
                    data = MapToDocumentResponse(result)
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
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

        [HttpGet("download/{documentId}")]
        public async Task<IActionResult> Download(long documentId)
        {
            try
            {
                var document = await _service.GetForDownloadAsync(documentId);

                var filePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    document.Fileurl.TrimStart('/')
                );

                if (!System.IO.File.Exists(filePath))
                    return NotFound(new { message = "File không tồn tại trên server." });

                var bytes = await System.IO.File.ReadAllBytesAsync(filePath);

                return File(bytes, "application/octet-stream", document.Documentname);
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

        [HttpDelete("delete/{documentId}")]
        [Authorize]
        public async Task<IActionResult> Delete(long documentId)
        {
            try
            {
                var role = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirst("system_role")?.Value ?? "";
                var userIdStr = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!long.TryParse(userIdStr, out long currentUserId))
                    return Unauthorized(new { message = "Token không hợp lệ hoặc thiếu userId." });

                await _service.DeleteAsync(documentId, currentUserId, role);

                return Ok(new
                {
                    message = "Xóa tài liệu thành công."
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
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