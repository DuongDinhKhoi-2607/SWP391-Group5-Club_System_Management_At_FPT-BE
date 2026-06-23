namespace BussinessLayer.DTOs.Club
{
    public class ClubListDto
    {
        public long ClubId { get; set; }
        public string ClubCode { get; set; } = null!;
        public string ClubName { get; set; } = null!;
        public string? Description { get; set; }
        public string? LogoImage { get; set; }
        public string? FanpageUrl { get; set; }
        public int TotalActiveMembers { get; set; }
        public string Status { get; set; } = null!;
        public DateOnly? FoundedDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
