using System;

namespace BussinessLayer.DTOs
{
    public class CreateClubDto
    {
        /// <summary>Tên CLB (phải unique)</summary>
        public string ClubName { get; set; } = null!;

        /// <summary>Mã CLB (phải unique, ví dụ: ATTT)</summary>
        public string ClubCode { get; set; } = null!;

        public string? Description { get; set; }
        public string? FanpageUrl { get; set; }
        public string? LogoImage { get; set; }
        public DateOnly? FoundedDate { get; set; }

        /// <summary>
        /// MSSV của sinh viên sẽ làm Manager của CLB (Leader hoặc Mentor).
        /// - Nếu sinh viên chưa có tài khoản hệ thống → tự động tạo User với systemRole = 'Manager'.
        /// - Sau khi tạo CLB, user này sẽ có Membership và được gán vào Boardmember với position = 'Leader'.
        /// </summary>
        public string ManagerStudentId { get; set; } = null!;
    }
}
