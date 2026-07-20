using BussinessLayer.DTOs.User;
using BussinessLayer.Interfaces;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories;

namespace BussinessLayer.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repo;
    private readonly ICloudinaryService _cloudinaryService;

    public UserService(IUserRepository repo, ICloudinaryService cloudinaryService)
    {
        _repo = repo;
        _cloudinaryService = cloudinaryService;
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

    public async Task<UserDetailDto> UpdateProfileAsync(long userId, UpdateUserProfileDto dto)
    {
        var user = await _repo.GetUserDetailByIdAsync(userId);
        if (user == null)
            throw new Exception("Không tìm thấy người dùng.");

        if (user.Userinformation == null)
            throw new Exception("Người dùng này không có hồ sơ thông tin cá nhân.");

        // Cập nhật số điện thoại
        if (dto.PhoneNumber != null)
        {
            user.Userinformation.Phonenumber = dto.PhoneNumber;
        }

        // Không cập nhật thông tin Student (Giới tính, Ngày sinh) vì đây là dữ liệu gốc của trường.

        // Tải lên avatar nếu có file gửi lên
        if (dto.AvatarFile != null)
        {
            var avatarUrl = await _cloudinaryService.UploadFileAsync(dto.AvatarFile, $"avatars/user_{userId}");
            user.Userinformation.Avatar = avatarUrl;
        }

        user.Userinformation.Infoupdatedat = DateTime.Now;

        await _repo.UpdateUserAsync(user);

        return new UserDetailDto
        {
            UserId = user.Userid,
            Username = user.Username,
            SystemRole = user.Systemrole,
            Status = user.Status,
            StudentId = user.Userinformation.Studentid,
            FullName = user.Userinformation.Student?.Fullname,
            SchoolEmail = user.Userinformation.Student?.Schoolemail,
            Avatar = user.Userinformation.Avatar,
            Phone = user.Userinformation.Phonenumber,
            Gender = user.Userinformation.Student?.Gender,
            Major = user.Userinformation.Student?.Major,
            AcademicBatch = user.Userinformation.Student?.Academicbatch,
            IsAlumni = user.Userinformation.Isalumni,
            GraduationDate = user.Userinformation.Graduationdate,
            CreatedAt = user.Createdat,
        };
    }

    public async Task<MemberActivityHistoryDto> GetMemberActivityHistoryAsync(long userId)
    {
        var user = await _repo.GetUserWithHistoryByIdAsync(userId);
        if (user == null)
            throw new Exception("Không tìm thấy người dùng.");

        var history = new MemberActivityHistoryDto();

        // 1. Map Club History
        if (user.Memberships != null)
        {
            foreach (var m in user.Memberships)
            {
                var clubDto = new MemberClubActivityDto
                {
                    ClubId = m.Clubid,
                    ClubName = m.Club.Clubname,
                    ClubCode = m.Club.Clubcode,
                    LogoImage = m.Club.Logoimage,
                    JoinDate = m.Joindate,
                    LeftDate = m.Leftdate,
                    Status = m.Status,
                    PersonalGoal = m.Personalgoal,
                    JoinReason = m.Joinreason
                };

                // Map Positions held in this membership
                if (m.Boardmembers != null)
                {
                    foreach (var bm in m.Boardmembers)
                    {
                        clubDto.Positions.Add(new MemberPositionHistoryDto
                        {
                            Position = bm.Position,
                            AppointedAt = bm.Appointedat,
                            KpiScore = bm.KpiScore,
                            BoardName = bm.Board?.Boardname ?? ""
                        });
                    }
                }

                history.ClubHistory.Add(clubDto);
            }
        }

        // 2. Map Event History
        if (user.Participants != null)
        {
            foreach (var p in user.Participants)
            {
                if (p.Event != null)
                {
                    history.EventHistory.Add(new MemberEventActivityDto
                    {
                        EventId = p.Eventid,
                        EventName = p.Event.Eventname,
                        StartTime = p.Event.Starttime,
                        EndTime = p.Event.Endtime,
                        RoleInEvent = p.Roleinevent,
                        AttendanceStatus = p.Attendancestatus,
                        CheckedInAt = p.Checkedinat,
                        EvaluationScore = p.Evaluationscore,
                        Feedback = p.Feedback,
                        ClubName = p.Event.Club?.Clubname ?? ""
                    });
                }
            }
        }

        return history;
    }

    private static string HashSha256(string input)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        return Convert.ToHexString(
            sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input))).ToLower();
    }
}
