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

        // ─────────────────────────────────────────────────────────────
        // VALIDATION
        // ─────────────────────────────────────────────────────────────

        public async Task<bool> ClubNameExistsAsync(string clubName)
            => await _context.Clubs.AnyAsync(c => c.Clubname == clubName);

        public async Task<bool> ClubCodeExistsAsync(string clubCode)
            => await _context.Clubs.AnyAsync(c => c.Clubcode == clubCode);

        /// <summary>Trả về Student kèm Userinformation (nếu có) và User (nếu có).</summary>
        public async Task<Student?> GetStudentByIdAsync(string studentId)
            => await _context.Students
                .Include(s => s.Userinformation)
                    .ThenInclude(ui => ui!.User)
                .FirstOrDefaultAsync(s => s.Studentid == studentId);

        /// <summary>
        /// Kiểm tra sinh viên đã có tài khoản User chưa.
        /// True = đã tồn tại User → không cho tạo (business rule).
        /// False = chưa có User → sẽ tự động tạo.
        /// </summary>
        public async Task<bool> UserExistsByStudentIdAsync(string studentId)
            => await _context.Userinformations
                .AnyAsync(ui => ui.Studentid == studentId);

        // ─────────────────────────────────────────────────────────────
        // CREATE
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Tạo CLB mới kèm leader theo quy tắc:
        ///   - Sinh viên ĐÃ có tài khoản User → dùng luôn account đó, gán Membership + Leader.
        ///   - Sinh viên CHƯA có tài khoản → tạo User mới với systemrole = "MEMBER",
        ///     rồi gán position = "Leader" trong Boardmember.
        /// </summary>
        public async Task<Club> CreateClubWithLeaderAsync(Club club, Student leaderStudent)
        {
            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Xác định userId
                long leaderId;
                if (leaderStudent.Userinformation != null)
                {
                    // Sinh viên ĐÃ có tài khoản → dùng luôn, không tạo mới
                    leaderId = leaderStudent.Userinformation.Userid;
                }
                else
                {
                    // Sinh viên CHƯA có tài khoản → tạo mới với role = MEMBER
                    var newUser = new User
                    {
                        Username     = leaderStudent.Studentid,
                        Passwordhash = HashSha256(leaderStudent.Studentid),
                        Systemrole   = "Member",
                        Status       = "Hoạt động",   // phải khớp với ck_user_status constraint trong DB
                        Createdat    = DateTime.Now
                    };
                    _context.Users.Add(newUser);
                    await _context.SaveChangesAsync();

                    // Liên kết User ↔ Student qua Userinformation
                    _context.Userinformations.Add(new Userinformation
                    {
                        Userid        = newUser.Userid,
                        Studentid     = leaderStudent.Studentid,
                        Isalumni      = false,
                        Infoupdatedat = DateTime.Now
                    });
                    await _context.SaveChangesAsync();

                    leaderId = newUser.Userid;
                }

                // 2. Tạo Club
                club.Createdat = DateTime.Now;
                _context.Clubs.Add(club);
                await _context.SaveChangesAsync();

                // 3. Membership — gán leaderId vào CLB
                var membership = new Membership
                {
                    Clubid     = club.Clubid,
                    Userid     = leaderId,
                    Joindate   = DateOnly.FromDateTime(DateTime.Now),
                    Status     = "Đang sinh hoạt",
                    Joinreason = "Người sáng lập CLB"
                };
                _context.Memberships.Add(membership);
                await _context.SaveChangesAsync();

                // 4. Clubboard — ban chấp hành đầu tiên
                var board = new Clubboard
                {
                    Clubid      = club.Clubid,
                    Boardname   = $"Ban chấp hành {club.Clubname} - Khóa 1",
                    Status      = "Đang đương nhiệm",
                    Description = "Ban chấp hành khóa đầu tiên",
                    Createdat   = DateTime.Now
                };
                _context.Clubboards.Add(board);
                await _context.SaveChangesAsync();

                // 5. Boardmember với Position = "Leader"
                _context.Boardmembers.Add(new Boardmember
                {
                    Boardid         = board.Boardid,
                    Membershipid    = membership.Membershipid,
                    Position        = "Leader",
                    Appointedat     = DateTime.Now,
                    Dutydescription = "Chủ nhiệm CLB"
                });
                await _context.SaveChangesAsync();

                await tx.CommitAsync();
                return club;
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                // Lấy lỗi gốc từ DB (PostgreSQL constraint, duplicate key, etc.)
                var innerMsg = ex.InnerException?.InnerException?.Message
                            ?? ex.InnerException?.Message
                            ?? ex.Message;
                throw new Exception($"Lỗi khi tạo CLB: {innerMsg}", ex);
            }

        }

        // ─────────────────────────────────────────────────────────────
        // READ
        // ─────────────────────────────────────────────────────────────

        public async Task<Club?> GetByIdAsync(long clubId)
            => await _context.Clubs.FindAsync(clubId);

        /// <summary>Lấy chi tiết CLB kèm thông tin Leader hiện tại.</summary>
        public async Task<(Club club, long? leaderId, string? leaderStudentId, string? leaderFullName, string? leaderEmail)?> GetWithLeaderByIdAsync(long clubId)
        {
            var club = await _context.Clubs.FirstOrDefaultAsync(c => c.Clubid == clubId);
            if (club == null) return null;

            // Tìm Leader: Boardmember có Position = "Leader" thuộc Board đang đương nhiệm của CLB
            var leaderData = await _context.Boardmembers
                .Include(bm => bm.Membership)
                    .ThenInclude(ms => ms.User)
                        .ThenInclude(u => u.Userinformation)
                            .ThenInclude(ui => ui!.Student)
                .Include(bm => bm.Board)
                .Where(bm =>
                    bm.Board.Clubid == clubId &&
                    bm.Board.Status == "Đang đương nhiệm" &&
                    bm.Position == "Leader" &&
                    bm.Membership.Clubid == clubId &&
                    bm.Membership.Status == "Đang sinh hoạt")
                .Select(bm => new
                {
                    LeaderId        = bm.Membership.Userid,
                    LeaderStudentId = bm.Membership.User.Userinformation != null ? bm.Membership.User.Userinformation.Studentid : null,
                    LeaderFullName  = bm.Membership.User.Userinformation != null ? bm.Membership.User.Userinformation.Student.Fullname : null,
                    LeaderEmail     = bm.Membership.User.Userinformation != null ? bm.Membership.User.Userinformation.Student.Schoolemail : null
                })
                .FirstOrDefaultAsync();

            return (
                club,
                leaderData?.LeaderId,
                leaderData?.LeaderStudentId,
                leaderData?.LeaderFullName,
                leaderData?.LeaderEmail
            );
        }

        /// <summary>Lấy danh sách CLB, có thể filter theo status.</summary>
        public async Task<IEnumerable<Club>> GetAllAsync(string? statusFilter)
        {
            var query = _context.Clubs.AsQueryable();
            if (!string.IsNullOrWhiteSpace(statusFilter))
                query = query.Where(c => c.Status == statusFilter);
            return await query.OrderByDescending(c => c.Createdat).ToListAsync();
        }

        public async Task<int> CountClubsAsync(string? statusFilter)
        {
            var query = _context.Clubs.AsQueryable();
            if (!string.IsNullOrWhiteSpace(statusFilter))
                query = query.Where(c => c.Status == statusFilter);
            return await query.CountAsync();
        }

        // ─────────────────────────────────────────────────────────────
        // UPDATE
        // ─────────────────────────────────────────────────────────────

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

        public async Task UpdateStatusAsync(long clubId, string newStatus)
        {
            var club = await _context.Clubs.FindAsync(clubId)
                ?? throw new Exception($"Không tìm thấy CLB với ID {clubId}.");
            club.Status = newStatus;
            await _context.SaveChangesAsync();
        }


        // ─────────────────────────────────────────────────────────────
        // HELPERS
        // ─────────────────────────────────────────────────────────────

        private static string HashSha256(string input)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            return Convert.ToHexString(
                sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input))).ToLower();
        }
    }
}
