namespace BussinessLayer.DTOs
{
    public class UpdateClubDto
    {
        public string ClubName { get; set; } = null!;
        public string? Description { get; set; }
        public string? LogoImage { get; set; }
        public string? FanpageUrl { get; set; }
        public DateTime? FoundedDate { get; set; }
    }
}