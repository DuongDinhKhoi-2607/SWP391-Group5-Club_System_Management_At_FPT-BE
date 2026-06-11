using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly ClubSystemDbContext _context;

    public AuthRepository(ClubSystemDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _context.Users
            .Include(u => u.Userinformation)
                .ThenInclude(ui => ui!.Student)
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<List<Membership>> GetUserMembershipsWithClubAsync(long userId)
    {
        return await _context.Memberships
            .Include(m => m.User)
            .Include(m => m.Club)
            .Include(m => m.Boardmembers)
                .ThenInclude(bm => bm.Board)   // Clubboard — dùng Board.Status để biết có đang đương nhiệm ko
            .Where(m => m.Userid == userId && m.Status == "Đang sinh hoạt")
            .ToListAsync();
    }

    public async Task UpdateLastLoginAsync(long userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.Lastloginat = DateTime.Now;
            await _context.SaveChangesAsync();
        }
    }
}
