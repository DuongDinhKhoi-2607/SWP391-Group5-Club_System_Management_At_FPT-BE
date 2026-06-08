using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories
{
    public class ClubRepository : IClubRepository
    {
        private readonly ClubSystemDbContext _context;

        public ClubRepository(ClubSystemDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ClubNameExistsAsync(string clubName)
            => await _context.Clubs.AnyAsync(c => c.Clubname == clubName);

        public async Task<bool> ClubCodeExistsAsync(string clubCode)
            => await _context.Clubs.AnyAsync(c => c.Clubcode == clubCode);

        public async Task<Student?> GetStudentByIdAsync(string studentId)
            => await _context.Students
                .Include(s => s.Userinformation).ThenInclude(ui => ui.User)
                .FirstOrDefaultAsync(s => s.Studentid == studentId);

        public async Task<Club> CreateClubWithManagerAsync(Club club, Student managerStudent)
        {
            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Xac dinh userId
                long managerId;
                if (managerStudent.Userinformation != null)
                {
                    managerId = managerStudent.Userinformation.Userid;
                    var u = managerStudent.Userinformation.User;
                    if (u.Systemrole != "Manager") { u.Systemrole = "Manager"; u.Updatedat = DateTime.Now; await _context.SaveChangesAsync(); }
                }
                else
                {
                    var newUser = new User { Username = managerStudent.Studentid, Passwordhash = HashSha256(managerStudent.Studentid), Systemrole = "Manager", Status = "Ch\u1edd k\u00edch ho\u1ea1t", Createdat = DateTime.Now };
                    _context.Users.Add(newUser);
                    await _context.SaveChangesAsync();
                    _context.Userinformations.Add(new Userinformation { Userid = newUser.Userid, Studentid = managerStudent.Studentid, Isalumni = false, Infoupdatedat = DateTime.Now });
                    await _context.SaveChangesAsync();
                    managerId = newUser.Userid;
                }
                // 2. Tao Club
                _context.Clubs.Add(club); await _context.SaveChangesAsync();
                // 3. Membership
                var ms = new Membership { Clubid = club.Clubid, Userid = managerId, Joindate = DateOnly.FromDateTime(DateTime.Now), Status = "\u0110ang sinh ho\u1ea1t", Joinreason = "Ng\u01b0\u1eddi s\u00e1ng l\u1eadp CLB" };
                _context.Memberships.Add(ms); await _context.SaveChangesAsync();
                // 4. Clubboard
                var board = new Clubboard { Clubid = club.Clubid, Boardname = $"Ban ch\u1ea5p h\u00e0nh {club.Clubname} - Kh\u00f3a 1", Status = "\u0110ang \u0111\u01b0\u01a1ng nhi\u1ec7m", Description = "Ban ch\u1ea5p h\u00e0nh kh\u00f3a \u0111\u1ea7u ti\u00ean" };
                _context.Clubboards.Add(board); await _context.SaveChangesAsync();
                // 5. Boardmember Leader
                _context.Boardmembers.Add(new Boardmember { Boardid = board.Boardid, Membershipid = ms.Membershipid, Position = "Leader", Appointedat = DateTime.Now, Dutydescription = "Ch\u1ee7 nhi\u1ec7m CLB" });
                await _context.SaveChangesAsync();
                await tx.CommitAsync();
                return club;
            }
            catch (Exception ex) { await tx.RollbackAsync(); var inner = ex.InnerException?.Message ?? ex.Message; throw new Exception($"L\u1ed7i t\u1ea1o CLB: {inner}", ex); }
        }

        private static string HashSha256(string input)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            return Convert.ToHexString(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input))).ToLower();
        }

        public async Task<Club?> GetByIdAsync(long clubId)
        {
            return await _context.Clubs.FindAsync(clubId);
        }

        public async Task<bool> IsLeaderOfClubAsync(long userId, long clubId)
        {
            return await _context.Boardmembers
                .Include(bm => bm.Membership)
                .Include(bm => bm.Board)
                .AnyAsync(bm =>
                    bm.Membership.Userid == userId &&
                    bm.Membership.Clubid == clubId &&
                    bm.Membership.Status == "Đang sinh hoạt" &&
                    bm.Board.Clubid == clubId &&
                    bm.Board.Status == "Đang đương nhiệm" &&
                    bm.Position == "Leader");
        }

        public async Task UpdateAsync(Club club)
        {
            _context.Clubs.Update(club);
            await _context.SaveChangesAsync();
        }
    }
}