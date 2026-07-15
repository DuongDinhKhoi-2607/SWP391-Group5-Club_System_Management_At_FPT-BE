using System;
namespace BussinessLayer.DTOs
{
    public class CreateClubDto
    {
        public string ClubName { get; set; } = null!;
        public string ClubCode { get; set; } = null!;
        public string? Description { get; set; }
        public string? FanpageUrl { get; set; }
        public string? LogoImage { get; set; }
        public DateOnly? FoundedDate { get; set; }
        /// <summary>MSSV cua sinh vien se lam Leader. Tu dong tao tai khoan neu chua co.</summary>
        public string LeaderStudentId { get; set; } = null!;
    }
}
