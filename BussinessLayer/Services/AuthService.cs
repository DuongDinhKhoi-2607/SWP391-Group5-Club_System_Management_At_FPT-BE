using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BussinessLayer.DTOs.Auth;
using BussinessLayer.Interfaces;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BussinessLayer.Services;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepo;
    private readonly IConfiguration _config;

    // Lấy từ appsettings.json → JwtSettings
    private string SecretKey => _config["JwtSettings:SecretKey"]!;
    private string Issuer    => _config["JwtSettings:Issuer"]!;
    private string Audience  => _config["JwtSettings:Audience"]!;
    private int AccessTokenExpiryMinutes => int.Parse(_config["JwtSettings:AccessTokenExpiryMinutes"]!);
    private int TempTokenExpiryMinutes   => int.Parse(_config["JwtSettings:TempTokenExpiryMinutes"]!);

    public AuthService(IAuthRepository authRepo, IConfiguration config)
    {
        _authRepo = authRepo;
        _config = config;
    }

    // ──────────────────────────────────────────────
    // BƯỚC 1: Login
    // ──────────────────────────────────────────────
    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto)
    {
        // 1. Tìm user
        var user = await _authRepo.GetUserByUsernameAsync(dto.Username)
            ?? throw new UnauthorizedAccessException("Tên đăng nhập hoặc mật khẩu không đúng.");

        // 2. Kiểm tra mật khẩu (SHA256)
        if (!VerifyPassword(dto.Password, user.Passwordhash))
            throw new UnauthorizedAccessException("Tên đăng nhập hoặc mật khẩu không đúng.");

        // 3. Kiểm tra tài khoản có được kích hoạt chưa
        if (user.Status != "Hoạt động")
            throw new UnauthorizedAccessException("Tài khoản chưa được kích hoạt hoặc đã bị khóa.");

        // 4. Cập nhật lastloginat
        await _authRepo.UpdateLastLoginAsync(user.Userid);

        var userInfo = BuildUserInfo(user);

        // 5. Admin → token ngay, không cần chọn CLB
        if (user.Systemrole == "Admin")
        {
            var adminToken = GenerateAccessToken(user.Userid, user.Username, user.Systemrole,
                                                  clubId: null, clubRole: null);
            return new LoginResponseDto
            {
                RequireClubSelection = false,
                AccessToken = adminToken,
                UserInfo = userInfo
            };
        }

        // 6. Lấy danh sách CLB đang sinh hoạt
        var memberships = await _authRepo.GetUserMembershipsWithClubAsync(user.Userid);

        if (memberships.Count == 0)
        {
            // User chưa thuộc CLB nào — vẫn cấp token nhưng không có club context
            var noClubToken = GenerateAccessToken(user.Userid, user.Username, user.Systemrole,
                                                   clubId: null, clubRole: null);
            return new LoginResponseDto
            {
                RequireClubSelection = false,
                AccessToken = noClubToken,
                UserInfo = userInfo
            };
        }

        if (memberships.Count == 1)
        {
            // Chỉ 1 CLB → tự chọn luôn
            var membership = memberships[0];
            var clubRole = DetermineClubRole(membership);
            var singleToken = GenerateAccessToken(user.Userid, user.Username, user.Systemrole,
                                                   clubId: membership.Clubid, clubRole: clubRole);
            userInfo.ClubId   = membership.Clubid;
            userInfo.ClubRole = clubRole;
            return new LoginResponseDto
            {
                RequireClubSelection = false,
                AccessToken = singleToken,
                UserInfo = userInfo
            };
        }

        // 7. Nhiều CLB → sinh tempToken + trả danh sách CLB
        var tempToken = GenerateTempToken(user.Userid);
        var clubList = memberships.Select(m => new ClubSelectionDto
        {
            ClubId   = m.Clubid,
            ClubName = m.Club.Clubname,
            ClubCode = m.Club.Clubcode,
            LogoImage = m.Club.Logoimage,
            ClubRole  = DetermineClubRole(m)
        }).ToList();

        return new LoginResponseDto
        {
            RequireClubSelection = true,
            TempToken = tempToken,
            AvailableClubs = clubList,
            UserInfo = userInfo
        };
    }

    // ──────────────────────────────────────────────
    // BƯỚC 2: Chọn CLB
    // ──────────────────────────────────────────────
    public async Task<LoginResponseDto> SelectClubAsync(SelectClubRequestDto dto)
    {
        // 1. Verify tempToken và lấy userId
        var userId = ValidateTempToken(dto.TempToken);

        // 2. Kiểm tra user có thuộc CLB được chọn không
        var memberships = await _authRepo.GetUserMembershipsWithClubAsync(userId);
        var selected = memberships.FirstOrDefault(m => m.Clubid == dto.ClubId)
            ?? throw new UnauthorizedAccessException("Bạn không thuộc câu lạc bộ này.");

        // 3. Lấy thông tin user (include Userinformation.Student)
        var user = await _authRepo.GetUserByUsernameAsync(selected.User.Username)
            ?? throw new UnauthorizedAccessException("Không tìm thấy người dùng.");

        // 4. Xác định clubRole
        var clubRole = DetermineClubRole(selected);

        // 5. Sinh JWT chính thức
        var accessToken = GenerateAccessToken(user.Userid, user.Username, user.Systemrole,
                                               clubId: dto.ClubId, clubRole: clubRole);

        // 6. Điền clubId + clubRole vào userInfo để FE không cần decode JWT
        var userInfo = BuildUserInfo(user);
        userInfo.ClubId   = dto.ClubId;
        userInfo.ClubRole = clubRole;

        return new LoginResponseDto
        {
            RequireClubSelection = false,
            AccessToken = accessToken,
            UserInfo = userInfo
        };
    }

    // ──────────────────────────────────────────────
    // HELPERS
    // ──────────────────────────────────────────────

    /// <summary>Hash SHA256 và so sánh với passwordhash trong DB</summary>
    private static bool VerifyPassword(string plain, string hash)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plain));
        var computed = Convert.ToHexString(bytes).ToLowerInvariant();
        return computed == hash.ToLowerInvariant();
    }

    /// <summary>
    /// Xác định role trong CLB:
    /// - Có Boardmember với Board (Clubboard) đang đương nhiệm → "Manager"
    /// - Chỉ có Membership thường → "Member"
    /// </summary>
    private static string DetermineClubRole(Membership membership)
    {
        var isManager = membership.Boardmembers
            .Any(bm => bm.Board.Status == "Đang đương nhiệm");
        return isManager ? "Manager" : "Member";
    }

    /// <summary>Tạo JWT chính thức</summary>
    private string GenerateAccessToken(long userId, string username, string systemRole,
                                        long? clubId, string? clubRole)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new("username", username),
            new("system_role", systemRole),
        };

        if (clubId.HasValue)
            claims.Add(new Claim("club_id", clubId.Value.ToString()));

        if (clubRole != null)
            claims.Add(new Claim("club_role", clubRole));

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(AccessTokenExpiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>Tạo JWT tạm 5 phút dùng cho bước chọn CLB</summary>
    private string GenerateTempToken(long userId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new("purpose", "club_selection")
        };

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(TempTokenExpiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>Verify tempToken và trả về userId. Ném exception nếu không hợp lệ.</summary>
    private long ValidateTempToken(string tempToken)
    {
        var handler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));

        var principal = handler.ValidateToken(tempToken, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = true,
            ValidIssuer = Issuer,
            ValidateAudience = true,
            ValidAudience = Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        }, out _);

        // Đảm bảo đây là tempToken dùng để chọn CLB
        var purpose = principal.FindFirst("purpose")?.Value;
        if (purpose != "club_selection")
            throw new UnauthorizedAccessException("Token không phải token chọn CLB.");

        var sub = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("Token không hợp lệ.");

        return long.Parse(sub);
    }


    private static UserInfoDto BuildUserInfo(User user)
    {
        // FullName lấy từ User → Userinformation → Student.Fullname
        var fullName = user.Userinformation?.Student?.Fullname;
        var avatar   = user.Userinformation?.Avatar;

        return new UserInfoDto
        {
            UserId     = user.Userid,
            Username   = user.Username,
            SystemRole = user.Systemrole,
            FullName   = fullName,
            Avatar     = avatar
        };
    }
}
