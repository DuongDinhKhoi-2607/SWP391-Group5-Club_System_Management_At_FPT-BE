using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ClubSystemDbContext _context;

    public UserRepository(ClubSystemDbContext context)
    {
        _context = context;
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _context.Users
            .Include(u => u.Userinformation)
                .ThenInclude(ui => ui!.Student)
            .OrderByDescending(u => u.Createdat)
            .ToListAsync();
    }

    public async Task<User?> GetUserByIdAsync(long userId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Userid == userId);
    }

    public async Task<User?> GetUserDetailByIdAsync(long userId)
    {
        return await _context.Users
            .Include(u => u.Userinformation)
                .ThenInclude(ui => ui!.Student)
            .FirstOrDefaultAsync(u => u.Userid == userId);
    }

    public async Task<Student?> GetStudentByIdAsync(string studentId)
    {
        return await _context.Students
            .FirstOrDefaultAsync(s => s.Studentid == studentId);
    }

    public async Task<bool> UserExistsByStudentIdAsync(string studentId)
    {
        return await _context.Userinformations
            .AnyAsync(ui => ui.Studentid == studentId);
    }

    public async Task<bool> UserExistsByUsernameAsync(string username)
    {
        return await _context.Users
            .AnyAsync(u => u.Username.ToLower() == username.ToLower());
    }

    public async Task<User> CreateUserAsync(User user, Userinformation userInfo)
    {
        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync(); // Sinh Userid

            userInfo.Userid = user.Userid;
            _context.Userinformations.Add(userInfo);
            await _context.SaveChangesAsync();

            await tx.CommitAsync();
            return user;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<User> CreateUserOnlyAsync(User user)
    {
        try
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }
        catch (DbUpdateException ex)
        {
            var innerMessage = ex.InnerException?.InnerException?.Message ?? ex.InnerException?.Message ?? ex.Message;
            throw new Exception($"Database Error: {innerMessage}");
        }
    }

    public async Task UpdateUserAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }
}
