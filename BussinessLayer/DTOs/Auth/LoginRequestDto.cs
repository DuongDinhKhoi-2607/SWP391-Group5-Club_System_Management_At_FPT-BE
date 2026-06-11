namespace BussinessLayer.DTOs.Auth;

public class LoginRequestDto
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
}
