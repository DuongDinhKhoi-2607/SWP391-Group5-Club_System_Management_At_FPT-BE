namespace BussinessLayer.DTOs
{
    public class ClubMemberListDto
    {
        public long MembershipId { get; set; }
        public long UserId { get; set; }
        public string? StudentId { get; set; }
        public string? FullName { get; set; }
        public string? SchoolEmail { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Avatar { get; set; }
        public string? Major { get; set; }
        public string? AcademicBatch { get; set; }
        public string MembershipStatus { get; set; } = null!;
        public DateOnly JoinDate { get; set; }
        public string CurrentPosition { get; set; } = "Member";
    }
}