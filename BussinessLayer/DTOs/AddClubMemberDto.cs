namespace BussinessLayer.DTOs
{
    public class AddClubMemberDto
    {
        public long ClubId { get; set; }
        public string StudentId { get; set; } = null!;
        public string? JoinReason { get; set; }
        public string? PersonalGoal { get; set; }
    }
}