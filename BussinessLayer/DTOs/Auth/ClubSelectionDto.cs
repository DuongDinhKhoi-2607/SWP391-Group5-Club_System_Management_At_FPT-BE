namespace BussinessLayer.DTOs.Auth;

public class ClubSelectionDto
{
    public long ClubId { get; set; }
    public string ClubName { get; set; } = null!;
    public string ClubCode { get; set; } = null!;
    public string? LogoImage { get; set; }

    /// <summary>"Manager" nếu user có record Boardmember active, ngược lại "Member"</summary>
    public string ClubRole { get; set; } = null!;
}
