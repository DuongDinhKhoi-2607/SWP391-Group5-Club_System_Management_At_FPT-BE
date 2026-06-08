using DataAccessLayer.Models;
using System.Threading.Tasks;

namespace DataAccessLayer.Repositories
{
    public interface IClubRepository
    {
        /// <summary>Kiểm tra tên CLB đã tồn tại chưa</summary>
        Task<bool> ClubNameExistsAsync(string clubName);

        /// <summary>Kiểm tra mã CLB đã tồn tại chưa</summary>
        Task<bool> ClubCodeExistsAsync(string clubCode);

        /// <summary>Tìm student theo MSSV trong bảng student (danh sách toàn trường)</summary>
        Task<Student?> GetStudentByIdAsync(string studentId);

        /// <summary>
        /// Tạo CLB kèm Manager (Phương án B - tự động tạo tài khoản):
        ///   1. Nếu student chưa có User → tạo User mới (systemRole = 'Manager')
        ///                                → tạo Userinformation liên kết student ↔ user
        ///   2. Tạo Club
        ///   3. Tạo Membership (user vào club)
        ///   4. Tạo Clubboard (Ban chấp hành Khóa 1)
        ///   5. Tạo Boardmember (position = 'Leader')
        /// </summary>
        Task<Club> CreateClubWithManagerAsync(Club club, Student managerStudent);
    }
}
