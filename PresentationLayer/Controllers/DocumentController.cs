using BussinessLayer.DTOs;
using BussinessLayer.Interfaces;
using DataAccessLayer.Models;
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
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromForm] UploadDocumentDto dto)
        {
            try
            {
                var result = await _service.UploadAsync(dto);

                return Ok(new
                {
                    message = "Upload tài liệu thành công.",
                    total = result.Count,
                    data = result.Select(MapToDocumentResponse)
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
        public async Task<IActionResult> Update(
            long documentId,
            [FromBody] UpdateDocumentDto dto)
        {
            try
            {
                var result = await _service.UpdateAsync(documentId, dto);

                return Ok(new
                {
                    message = "Cập nhật tài liệu thành công.",
                    data = MapToDocumentResponse(result)
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
        public async Task<IActionResult> Delete(long documentId)
        {
            try
            {
                await _service.DeleteAsync(documentId);

                return Ok(new
                {
                    message = "Xóa tài liệu thành công."
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