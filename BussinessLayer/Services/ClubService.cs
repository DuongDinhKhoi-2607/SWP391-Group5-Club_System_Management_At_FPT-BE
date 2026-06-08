using BussinessLayer.DTOs;
using BussinessLayer.Interfaces;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories;
using System;
using System.Threading.Tasks;

namespace BussinessLayer.Services
{
    public class ClubService : IClubService
    {
        private readonly IClubRepository _repo;

        public ClubService(IClubRepository repo)
        {
            _repo = repo;
        }

        public async Task<Club> CreateClubAsync(CreateClubDto dto)
        {
            // Validate tên CLB không trùng
            if (await _repo.ClubNameExistsAsync(dto.ClubName))
                throw new Exception($"Tên CLB '{dto.ClubName}' đã tồn tại.");

            // Validate mã CLB không trùng
            if (await _repo.ClubCodeExistsAsync(dto.ClubCode))
                throw new Exception($"Mã CLB '{dto.ClubCode}' đã tồn tại.");

            // Tìm sinh viên theo MSSV trong bảng student
            var student = await _repo.GetStudentByIdAsync(dto.ManagerStudentId);
            if (student == null)
                throw new Exception($"Không tìm thấy sinh viên với MSSV '{dto.ManagerStudentId}' trong hệ thống.");

            // Chỉ cho phép sinh viên đang học làm Manager
            if (student.Status != "Đang học")
                throw new Exception($"Sinh viên '{student.Fullname}' (MSSV: {dto.ManagerStudentId}) " +
                                    $"không đủ điều kiện (trạng thái: {student.Status}).");

            var club = new Club
            {
                Clubname          = dto.ClubName,
                Clubcode          = dto.ClubCode,
                Description       = dto.Description,
                Fanpageurl        = dto.FanpageUrl,
                Logoimage         = dto.LogoImage,
                Foundeddate       = dto.FoundedDate,
                Status            = "Đang hoạt động",
                Totalactivemembers = 1  // Manager là thành viên đầu tiên
            };

            return await _repo.CreateClubWithManagerAsync(club, student);
        }
    }
}
