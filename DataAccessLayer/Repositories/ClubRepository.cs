using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

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
        {
            return await _context.Clubs.AnyAsync(c => c.Clubname == clubName);
        }

        public async Task<bool> ClubCodeExistsAsync(string clubCode)
        {
            return await _context.Clubs.AnyAsync(c => c.Clubcode == clubCode);
        }

        public async Task<Student?> GetStudentByIdAsync(string studentId)
        {
            // Load Userinformation → User để kiểm tra tài khoản hiện có
            // EF Core tự xử lý null navigation property, không cần null check trong ThenInclude
            return await _context.Students
                .Include(s => s.Userinformation)
                    .ThenInclude(ui => ui.User)
                .FirstOrDefaultAsync(s => s.Studentid == studentId);
        }

        public async Task<Club> CreateClubWithManagerAsync(Club club, Student managerStudent)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // ─── BƯỚC 1: Xác định userId của Manager ─────────────────────
                long managerId;

                if (managerStudent.Userinformation?.Userid != null)
                {
                    // Student ĐÃ CÓ tài khoản → cập nhật systemRole thành Manager
                    managerId = managerStudent.Userinformation.Userid;

                    var existingUser = managerStudent.Userinformation.User;
                    if (existingUser.Systemrole != "Manager")
                    {
                        existingUser.Systemrole = "Manager";
                        existingUser.Updatedat = DateTime.Now;
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    // Student CHƯA CÓ tài khoản → tự động tạo User mới
                    var newUser = new User
                    {
                        Username    = managerStudent.Studentid,
                        Passwordhash = HashPassword(managerStudent.Studentid), // mật khẩu mặc định = MSSV
                        Systemrole  = "Manager",
                        Status      = "Chờ kích hoạt",
                        Createdat   = DateTime.Now
                    };
                    _context.Users.Add(newUser);
                    await _context.SaveChangesAsync();

                    // Tạo Userinformation liên kết student ↔ user
                    var newUserInfo = new Userinformation
                    {
                        Userid         = newUser.Userid,
                        Studentid      = managerStudent.Studentid,
                        Isalumni       = false,
                        Infoupdatedat  = DateTime.Now
                    };
                    _context.Userinformations.Add(newUserInfo);
                    await _context.SaveChangesAsync();

                    managerId = newUser.Userid;
                }

                // ─── BƯỚC 2: Tạo CLB ─────────────────────────────────────────
                _context.Clubs.Add(club);
                await _context.SaveChangesAsync();

                // ─── BƯỚC 3: Tạo Membership (Manager gia nhập CLB) ───────────
                var membership = new Membership
                {
                    Clubid    = club.Clubid,
                    Userid    = managerId,
                    Joindate  = DateOnly.FromDateTime(DateTime.Now),
                    Status    = "Đang sinh hoạt",
                    Joinreason = "Người sáng lập và quản lý CLB"
                };
                _context.Memberships.Add(membership);
                await _context.SaveChangesAsync();

                // ─── BƯỚC 4: Tạo Clubboard (Ban chấp hành đầu tiên) ──────────
                var board = new Clubboard
                {
                    Clubid     = club.Clubid,
                    Boardname  = $"Ban chấp hành {club.Clubname} - Khóa 1",
                    Status     = "Đang đương nhiệm",
                    Description = "Ban chấp hành khóa đầu tiên"
                };
                _context.Clubboards.Add(board);
                await _context.SaveChangesAsync();

                // ─── BƯỚC 5: Tạo Boardmember với position = 'Leader' ─────────
                var boardMember = new Boardmember
                {
                    Boardid         = board.Boardid,
                    Membershipid    = membership.Membershipid,
                    Position        = "Leader",
                    Appointedat     = DateTime.Now,
                    Dutydescription = "Chủ nhiệm CLB - điều hành toàn bộ hoạt động"
                };
                _context.Boardmembers.Add(boardMember);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return club;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Ném lại với inner exception để thấy lỗi DB thật sự
                var innerMsg = ex.InnerException?.Message ?? ex.Message;
                throw new Exception($"Lỗi khi tạo CLB: {innerMsg}", ex);
            }
        }

        /// <summary>
        /// Hash mật khẩu mặc định bằng SHA256.
        /// Mật khẩu mặc định = MSSV của sinh viên.
        /// TODO: Thay bằng BCrypt.Net khi tích hợp thư viện.
        /// </summary>
        private static string HashPassword(string input)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(input);
            var hash  = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash).ToLower();
        }
    }
}
