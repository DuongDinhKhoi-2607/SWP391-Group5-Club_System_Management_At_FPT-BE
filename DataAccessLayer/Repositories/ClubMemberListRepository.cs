using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories
{
    public class ClubMemberListRepository : IClubMemberListRepository
    {
        private readonly ClubSystemDbContext _context;

        public ClubMemberListRepository(ClubSystemDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsManagerOfClubAsync(long userId, long clubId)
        {
            return await _context.Memberships
                .Include(m => m.Boardmembers)
                .AnyAsync(m =>
                    m.Userid == userId &&
                    m.Clubid == clubId &&
                    m.Status == "Đang sinh hoạt" &&
                    m.Boardmembers.Any());
        }

        public async Task<List<Membership>> GetActiveMembersByClubAsync(long clubId)
        {
            return await _context.Memberships
                .Include(m => m.User)
                    .ThenInclude(u => u.Userinformation)
                        .ThenInclude(ui => ui.Student)
                .Include(m => m.Boardmembers)
                    .ThenInclude(bm => bm.Board)
                .Where(m =>
                    m.Clubid == clubId &&
                    m.Status == "Đang sinh hoạt")
                .OrderBy(m => m.User.Userinformation.Student.Fullname)
                .ToListAsync();
        }

        public async Task<Student?> GetStudentByIdAsync(string studentId)
        {
            return await _context.Students
                .FirstOrDefaultAsync(s => s.Studentid == studentId);
        }

        public async Task<User?> GetUserByStudentIdAsync(string studentId)
        {
            return await _context.Users
                .Include(u => u.Userinformation)
                .FirstOrDefaultAsync(u =>
                    u.Userinformation != null &&
                    u.Userinformation.Studentid == studentId);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == email);
        }

        public async Task<bool> IsActiveMemberAsync(long userId, long clubId)
        {
            return await _context.Memberships.AnyAsync(m =>
                m.Userid == userId &&
                m.Clubid == clubId &&
                m.Status == "Đang sinh hoạt");
        }

        public async Task<Membership> AddMemberByStudentIdAsync(
            Student student,
            long clubId,
            string? joinReason,
            string? personalGoal)
        {
            await using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var user = await GetUserByStudentIdAsync(student.Studentid);

                if (user == null)
                    user = await GetUserByEmailAsync(student.Schoolemail);

                if (user == null)
                {
                    user = new User
                    {
                        Username = student.Schoolemail,
                        Passwordhash = HashSha256(student.Studentid),
                        Systemrole = "Member",
                        Status = "Hoạt động",
                        Createdat = DateTime.Now
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    var userInfo = new Userinformation
                    {
                        Userid = user.Userid,
                        Studentid = student.Studentid,
                        Isalumni = false,
                        Infoupdatedat = DateTime.Now
                    };

                    _context.Userinformations.Add(userInfo);
                    await _context.SaveChangesAsync();
                }

                var membership = new Membership
                {
                    Userid = user.Userid,
                    Clubid = clubId,
                    Joinreason = joinReason,
                    Personalgoal = personalGoal,
                    Status = "Đang sinh hoạt",
                    Joindate = DateOnly.FromDateTime(DateTime.Now)
                };

                _context.Memberships.Add(membership);

                var club = await _context.Clubs.FindAsync(clubId);
                if (club != null)
                    club.Totalactivemembers += 1;

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return membership;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        private static string HashSha256(string input)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();

            return Convert.ToHexString(
                sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input))
            ).ToLower();
        }

        public async Task<Membership?> GetMemberDetailByMembershipIdAsync(long membershipId)
        {
            return await _context.Memberships
                .Include(m => m.User)
                    .ThenInclude(u => u.Userinformation)
                        .ThenInclude(ui => ui.Student)
                .Include(m => m.Boardmembers)
                    .ThenInclude(bm => bm.Board)
                .FirstOrDefaultAsync(m => m.Membershipid == membershipId);
        }

        public async Task<Membership?> GetMembershipByIdAsync(long membershipId)
        {
            return await _context.Memberships
                .FirstOrDefaultAsync(m => m.Membershipid == membershipId);
        }

        public async Task UpdateMembershipAsync(Membership membership)
        {
            _context.Memberships.Update(membership);
            await _context.SaveChangesAsync();
        }
    }
}