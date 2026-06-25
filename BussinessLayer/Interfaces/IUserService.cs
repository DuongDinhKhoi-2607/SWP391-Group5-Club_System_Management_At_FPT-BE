using BussinessLayer.DTOs.User;

namespace BussinessLayer.Interfaces;

public interface IUserService
{
    Task<List<UserListDto>> GetAllUsersAsync();
    Task<UserListDto> CreateStaffUserAsync(CreateStaffUserDto dto);
    Task BlockUserAsync(long userId);
    Task UnblockUserAsync(long userId);
}
