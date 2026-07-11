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

                if (user != null)
                {
                    // Kiểm tra nếu tài khoản "Chờ kích hoạt" đã quá 24h
                    if (user.Status == "Chờ kích hoạt" && user.Createdat.AddHours(24) < DateTime.Now)
                    {
                        var userInfos = await _context.Userinformations.Where(ui => ui.Userid == user.Userid).ToListAsync();
                        _context.Userinformations.RemoveRange(userInfos);

                        var memberships = await _context.Memberships.Where(m => m.Userid == user.Userid).ToListAsync();
                        _context.Memberships.RemoveRange(memberships);

                        _context.Users.Remove(user);
                        await _context.SaveChangesAsync();

                        user = null; // Gán lại null để lát tạo mới
                    }
                    else
                    {
                        // Kiểm tra nếu lượt đăng ký của CLB này "Chờ kích hoạt" đã quá hạn (được mời hơn 24h trước)
                        var existingMembership = await _context.Memberships
                            .FirstOrDefaultAsync(m => m.Userid == user.Userid && m.Clubid == clubId);

                        if (existingMembership != null && existingMembership.Status == "Chờ kích hoạt")
                        {
                            var limitDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
                            if (existingMembership.Joindate < limitDate)
                            {
                                _context.Memberships.Remove(existingMembership);
                                await _context.SaveChangesAsync();
                            }
                        }
                    }
                }

                if (user == null)
                {
                    user = new User
                    {
                        Username = student.Schoolemail,
                        Passwordhash = HashSha256(student.Studentid),
                        Systemrole = "MEMBER",
                        Status = "Chờ kích hoạt",
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

                var membership = await _context.Memberships
                    .FirstOrDefaultAsync(m => m.Userid == user.Userid && m.Clubid == clubId);

                if (membership == null)
                {
                    membership = new Membership
                    {
                        Userid = user.Userid,
                        Clubid = clubId,
                        Joinreason = joinReason,
                        Personalgoal = personalGoal,
                        Status = "Chờ kích hoạt",
                        Joindate = DateOnly.FromDateTime(DateTime.Now)
                    };
                    _context.Memberships.Add(membership);
                }
                else
                {
                    if (membership.Status == "Đang sinh hoạt")
                        throw new Exception("Sinh viên này đã là thành viên của CLB.");

                    membership.Status = "Chờ kích hoạt";
                    membership.Joinreason = joinReason;
                    membership.Personalgoal = personalGoal;
                    membership.Joindate = DateOnly.FromDateTime(DateTime.Now);
                    membership.Leftdate = null;
                }

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

        public async Task<(User user, Membership membership, bool isNewUser, string? plainPassword)> ActivateMemberAsync(long userId, long clubId)
        {
            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _context.Users
                    .Include(u => u.Userinformation)
                        .ThenInclude(ui => ui!.Student)
                    .FirstOrDefaultAsync(u => u.Userid == userId);

                if (user == null)
                    throw new Exception("Không tìm thấy người dùng.");

                var membership = await _context.Memberships
                    .FirstOrDefaultAsync(m => m.Userid == userId && m.Clubid == clubId);

                if (membership == null)
                    throw new Exception("Không tìm thấy thông tin đăng ký tham gia câu lạc bộ.");

                if (membership.Status == "Đang sinh hoạt" && user.Status == "Hoạt động")
                {
                    throw new Exception("Tài khoản và tư cách thành viên đã được kích hoạt từ trước.");
                }

                bool isNewUser = user.Status == "Chờ kích hoạt";
                string? plainPassword = null;

                if (isNewUser)
                {
                    user.Status = "Hoạt động";
                    plainPassword = GenerateRandomPassword(8);
                    user.Passwordhash = HashSha256(plainPassword);
                }

                if (membership.Status != "Đang sinh hoạt")
                {
                    membership.Status = "Đang sinh hoạt";
                    
                    var club = await _context.Clubs.FindAsync(clubId);
                    if (club != null)
                    {
                        club.Totalactivemembers += 1;
                    }
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return (user, membership, isNewUser, plainPassword);
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        private static string GenerateRandomPassword(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var password = new char[length];
            for (int i = 0; i < length; i++)
            {
                password[i] = chars[random.Next(chars.Length)];
            }
            return new string(password);
        }
    }
}
