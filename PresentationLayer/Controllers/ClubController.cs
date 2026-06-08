using BussinessLayer.DTOs;
using BussinessLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace PresentationLayer.Controllers
{
    [Route("api/clubs")]
    [ApiController]
    public class ClubController : ControllerBase
    {
        private readonly IClubService _service;

        public ClubController(IClubService service)
        {
            _service = service;
        }

        /// <summary>
        /// [Admin - Phòng IC] Tạo CLB mới và gán Manager từ danh sách sinh viên.
        /// 
        /// Luồng xử lý:
        ///   1. Kiểm tra MSSV trong bảng student
        ///   2. Nếu chưa có tài khoản → tự động tạo User (systemRole = Manager)
        ///   3. Tạo Club → Membership → Clubboard → Boardmember (position = Leader)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateClub([FromBody] CreateClubDto dto)
        {
            try
            {
                var result = await _service.CreateClubAsync(dto);

                return Ok(new
                {
                    message = "Tạo CLB thành công.",
                    data = new
                    {
                        result.Clubid,
                        result.Clubname,
                        result.Clubcode,
                        result.Description,
                        result.Status,
                        result.Totalactivemembers,
                        result.Createdat,
                        manager = new
                        {
                            studentId  = dto.ManagerStudentId,
                            systemRole = "Manager",
                            position   = "Leader"
                        }
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
