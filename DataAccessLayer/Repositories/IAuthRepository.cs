using DataAccessLayer.Models;

namespace DataAccessLayer.Repositories;

public interface IAuthRepository
{
    /// <summary>Tìm user theo username, include Userinformation và Student để lấy FullName, Avatar</summary>
    Task<User?> GetUserByUsernameAsync(string username);

    /// <summary>Lấy danh sách Membership đang sinh hoạt của user, include Club và Boardmembers (kèm Board)</summary>
    Task<List<Membership>> GetUserMembershipsWithClubAsync(long userId);

    /// <summary>Cập nhật thời gian đăng nhập cuối</summary>
    Task UpdateLastLoginAsync(long userId);

    Task<int> CountUsersAsync();
}
