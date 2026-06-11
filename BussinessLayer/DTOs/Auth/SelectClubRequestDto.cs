namespace BussinessLayer.DTOs.Auth;

public class SelectClubRequestDto
{
    public string TempToken { get; set; } = null!;
    public long ClubId { get; set; }
}
