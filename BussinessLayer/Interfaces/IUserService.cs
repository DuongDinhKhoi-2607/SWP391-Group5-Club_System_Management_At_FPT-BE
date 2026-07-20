using BussinessLayer.DTOs.User;

namespace BussinessLayer.Interfaces;

public interface IUserService
{
    Task<List<UserListDto>> GetAllUsersAsync();
    Task<UserDetailDto> GetUserByIdAsync(long userId);
    Task<UserListDto> CreateStaffUserAsync(CreateStaffUserDto dto);
    Task BlockUserAsync(long userId);
    Task UnblockUserAsync(long userId);
    Task<UserDetailDto> UpdateProfileAsync(long userId, UpdateUserProfileDto dto);
    Task<MemberActivityHistoryDto> GetMemberActivityHistoryAsync(long userId);
    Task ChangePasswordAsync(long userId, ChangePasswordDto dto);
}
