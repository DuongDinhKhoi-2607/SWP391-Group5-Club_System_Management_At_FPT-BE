using DataAccessLayer.Models;

namespace DataAccessLayer.Repositories;

public interface IUserRepository
{
    Task<List<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(long userId);
    Task<User?> GetUserDetailByIdAsync(long userId);
    Task<Student?> GetStudentByIdAsync(string studentId);
    Task<bool> UserExistsByStudentIdAsync(string studentId);
    Task<bool> UserExistsByUsernameAsync(string username);
    Task<User> CreateUserAsync(User user, Userinformation userInfo);
    Task<User> CreateUserOnlyAsync(User user);
    Task UpdateUserAsync(User user);
}
