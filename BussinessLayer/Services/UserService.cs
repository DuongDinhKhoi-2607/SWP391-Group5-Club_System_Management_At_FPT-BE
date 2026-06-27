using BussinessLayer.DTOs.User;
using BussinessLayer.Interfaces;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories;

namespace BussinessLayer.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repo;

    public UserService(IUserRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<UserListDto>> GetAllUsersAsync()
    {
        var users = await _repo.GetAllUsersAsync();
        return users.Select(u => new UserListDto
        {
            UserId = u.Userid,
            Username = u.Username,
            SystemRole = u.Systemrole,
            Status = u.Status,
            StudentId = u.Userinformation?.Studentid,
            FullName = u.Userinformation?.Student?.Fullname,
            SchoolEmail = u.Userinformation?.Student?.Schoolemail,
            Avatar = u.Userinformation?.Avatar,
            CreatedAt = u.Createdat,
            LastLoginAt = u.Lastloginat
        }).ToList();
    }

    public async Task<UserDetailDto> GetUserByIdAsync(long userId)
    {
        var user = await _repo.GetUserDetailByIdAsync(userId);
        if (user == null)
            throw new Exception("Không tìm thấy người dùng.");

        return new UserDetailDto
        {
            UserId = user.Userid,
            Username = user.Username,
            SystemRole = user.Systemrole,
            Status = user.Status,
            StudentId = user.Userinformation?.Studentid,
            FullName = user.Userinformation?.Student?.Fullname,
            SchoolEmail = user.Userinformation?.Student?.Schoolemail,
            Avatar = user.Userinformation?.Avatar,
            Phone = user.Userinformation?.Phonenumber,
            Gender = user.Userinformation?.Student?.Gender,
            Major = user.Userinformation?.Student?.Major,
            AcademicBatch = user.Userinformation?.Student?.Academicbatch,
            IsAlumni = user.Userinformation?.Isalumni,
            GraduationDate = user.Userinformation?.Graduationdate,
            CreatedAt = user.Createdat,
            LastLoginAt = user.Lastloginat
        };
    }

    public async Task<UserListDto> CreateStaffUserAsync(CreateStaffUserDto dto)
    {
        dto.SystemRole = dto.SystemRole.ToUpper() == "ADMIN" ? "ADMIN" : 
                         dto.SystemRole.Equals("MANAGER", StringComparison.OrdinalIgnoreCase) ? "Manager" : dto.SystemRole;

        if (dto.SystemRole != "ADMIN" && dto.SystemRole != "Manager")
            throw new Exception("Chức vụ hệ thống (SystemRole) chỉ được phép là 'ADMIN' hoặc 'Manager'.");

        var exists = await _repo.UserExistsByUsernameAsync(dto.Username);
        if (exists)
            throw new Exception("Tên đăng nhập này đã tồn tại.");

        var newUser = new User
        {
            Username = dto.Username,
            Passwordhash = HashSha256(dto.Password),
            Systemrole = dto.SystemRole,
            Status = "Hoạt động",
            Createdat = DateTime.Now
        };

        var createdUser = await _repo.CreateUserOnlyAsync(newUser);

        return new UserListDto
        {
            UserId = createdUser.Userid,
            Username = createdUser.Username,
            SystemRole = createdUser.Systemrole,
            Status = createdUser.Status,
            StudentId = null,
            FullName = null,
            SchoolEmail = null,
            Avatar = null,
            CreatedAt = createdUser.Createdat,
            LastLoginAt = null
        };
    }

    public async Task BlockUserAsync(long userId)
    {
        var user = await _repo.GetUserByIdAsync(userId);
        if (user == null)
            throw new Exception("Không tìm thấy người dùng.");

        if (user.Systemrole == "ADMIN")
            throw new Exception("Không thể khóa tài khoản ADMIN.");

        user.Status = "Bị khóa";
        await _repo.UpdateUserAsync(user);
    }

    public async Task UnblockUserAsync(long userId)
    {
        var user = await _repo.GetUserByIdAsync(userId);
        if (user == null)
            throw new Exception("Không tìm thấy người dùng.");

        user.Status = "Hoạt động";
        await _repo.UpdateUserAsync(user);
    }

    private static string HashSha256(string input)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        return Convert.ToHexString(
            sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input))).ToLower();
    }
}
