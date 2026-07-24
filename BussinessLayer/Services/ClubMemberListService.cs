using BussinessLayer.DTOs;
using BussinessLayer.Interfaces;
using DataAccessLayer.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BussinessLayer.Services
{
    public class ClubMemberListService : IClubMemberListService
    {
        private readonly IClubMemberListRepository _repo;
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClubMemberListService(
            IClubMemberListRepository repo, 
            IAuthService authService, 
            IEmailService emailService,
            IConfiguration config,
            IHttpContextAccessor httpContextAccessor)
        {
            _repo = repo;
            _authService = authService;
            _emailService = emailService;
            _config = config;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<ClubMemberListDto>> GetActiveMembersByClubAsync(
            long clubId,
            long currentUserId)
        {
            var isMember = await _repo.IsActiveMemberAsync(currentUserId, clubId);

            if (!isMember)
                throw new UnauthorizedAccessException("Bạn không có quyền xem danh sách thành viên của CLB này.");

            var members = await _repo.GetActiveMembersByClubAsync(clubId);

            var result = members.Select(m => new ClubMemberListDto
            {
                MembershipId = m.Membershipid,
                UserId = m.Userid,
                StudentId = m.User.Userinformation?.Studentid,
                FullName = m.User.Userinformation?.Student?.Fullname,
                SchoolEmail = m.User.Userinformation?.Student?.Schoolemail ?? m.User.Username,
                PhoneNumber = m.User.Userinformation?.Phonenumber,
                Avatar = m.User.Userinformation?.Avatar,
                Major = m.User.Userinformation?.Student?.Major,
                AcademicBatch = m.User.Userinformation?.Student?.Academicbatch,
                MembershipStatus = m.Status,
                JoinDate = m.Joindate,

                CurrentPosition = m.Boardmembers
                    .FirstOrDefault(bm => bm.Board.Status == "Đang đương nhiệm")
                    ?.Position ?? "Member"
            }).ToList();

            // Đưa Leader và Boardmember lên đầu
            return result.OrderBy(m =>
            {
                if (string.Equals(m.CurrentPosition, "Leader", StringComparison.OrdinalIgnoreCase)) return 0;
                if (m.CurrentPosition != "Member") return 1;
                return 2;
            }).ThenBy(m => m.FullName).ToList();
        }

        public async Task<List<ClubMemberListDto>> GetAlumniMembersByClubAsync(
            long clubId,
            long currentUserId,
            string? searchQuery)
        {
            var isMember = await _repo.IsActiveMemberAsync(currentUserId, clubId);

            if (!isMember)
                throw new UnauthorizedAccessException("Bạn không có quyền xem danh sách cựu thành viên của CLB này.");

            var members = await _repo.GetAlumniMembersByClubAsync(clubId, searchQuery);

            var result = members.Select(m => new ClubMemberListDto
            {
                MembershipId = m.Membershipid,
                UserId = m.Userid,
                StudentId = m.User.Userinformation?.Studentid,
                FullName = m.User.Userinformation?.Student?.Fullname,
                SchoolEmail = m.User.Userinformation?.Student?.Schoolemail ?? m.User.Username,
                PhoneNumber = m.User.Userinformation?.Phonenumber,
                Avatar = m.User.Userinformation?.Avatar,
                Major = m.User.Userinformation?.Student?.Major,
                AcademicBatch = m.User.Userinformation?.Student?.Academicbatch,
                MembershipStatus = m.Status,
                JoinDate = m.Joindate,

                CurrentPosition = "Cựu thành viên"
            }).ToList();

            return result;
        }
        public async Task<ClubMemberListDto> AddMemberByStudentIdAsync(
    AddClubMemberDto dto,
    long clubId,
    long currentUserId)
        {
            var isLeader = await _repo.IsLeaderOfClubAsync(currentUserId, clubId);

            if (!isLeader)
                throw new UnauthorizedAccessException("Chỉ Leader của CLB mới được thêm thành viên.");

            var student = await _repo.GetStudentByIdAsync(dto.StudentId);

            if (student == null)
                throw new Exception($"Không tìm thấy sinh viên có MSSV '{dto.StudentId}'.");

            if (student.Status != "Đang học")
                throw new Exception($"Sinh viên '{student.Fullname}' không đủ điều kiện tham gia CLB.");

            var existingUser = await _repo.GetUserByStudentIdAsync(student.Studentid)
                ?? await _repo.GetUserByEmailAsync(student.Schoolemail);

            if (existingUser != null)
            {
                var isAlreadyMember = await _repo.IsActiveMemberAsync(
                    existingUser.Userid,
                    clubId
                );

                if (isAlreadyMember)
                    throw new Exception("Sinh viên này đã là thành viên của CLB.");
            }

            var membership = await _repo.AddMemberByStudentIdAsync(
                student,
                clubId,
                dto.JoinReason,
                dto.PersonalGoal
            );

            // Gửi email xác nhận kích hoạt tài khoản
            var token = _authService.GenerateActivationToken(membership.Userid, membership.Clubid);
            var request = _httpContextAccessor.HttpContext?.Request;
            var baseUrl = request != null 
                ? $"{request.Scheme}://{request.Host}"
                : _config["AppSettings:BaseUrl"] ?? "http://localhost:5242";
            var activationLink = $"{baseUrl}/api/member/confirm-activation?token={token}";

            var subject = "Xác nhận tham gia Câu lạc bộ và kích hoạt tài khoản";
            var body = $@"
                <h3>Chào bạn {student.Fullname},</h3>
                <p>Bạn đã được mời tham gia câu lạc bộ với vai trò là thành viên.</p>
                <p>Vui lòng nhấn vào liên kết bên dưới để kích hoạt tài khoản và xác nhận gia nhập câu lạc bộ:</p>
                <p><a href='{activationLink}' style='background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; display: inline-block; border-radius: 4px; font-weight: bold;'>Xác nhận tham gia</a></p>
                <p>Liên kết này có hiệu lực trong vòng 24 giờ.</p>
                <p>Nếu bạn không yêu cầu tham gia câu lạc bộ, vui lòng bỏ qua email này.</p>";

            // Gửi email xác nhận (chạy ngầm để không làm treo API)
            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendEmailAsync(student.Schoolemail, subject, body);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Lỗi gửi email]: {ex.Message}");
                }
            });

            return new ClubMemberListDto
            {
                MembershipId = membership.Membershipid,
                UserId = membership.Userid,
                StudentId = student.Studentid,
                FullName = student.Fullname,
                SchoolEmail = student.Schoolemail,
                Major = student.Major,
                AcademicBatch = student.Academicbatch,
                MembershipStatus = membership.Status,
                JoinDate = membership.Joindate,
                CurrentPosition = "Member"
            };


        }
        public async Task<ClubMemberDetailDto> GetMemberDetailAsync(long membershipId)
        {
            var member = await _repo.GetMemberDetailByMembershipIdAsync(membershipId);

            if (member == null)
                throw new Exception("Không tìm thấy thành viên trong CLB.");

            return new ClubMemberDetailDto
            {
                MembershipId = member.Membershipid,
                UserId = member.Userid,
                ClubId = member.Clubid,

                StudentId = member.User.Userinformation?.Studentid,
                FullName = member.User.Userinformation?.Student?.Fullname,
                SchoolEmail = member.User.Userinformation?.Student?.Schoolemail ?? member.User.Username,
                PhoneNumber = member.User.Userinformation?.Phonenumber,
                Avatar = member.User.Userinformation?.Avatar,
                Major = member.User.Userinformation?.Student?.Major,
                AcademicBatch = member.User.Userinformation?.Student?.Academicbatch,
                Gender = member.User.Userinformation?.Student?.Gender,
                DateOfBirth = member.User.Userinformation?.Student?.Dateofbirth,

                MembershipStatus = member.Status,
                JoinDate = member.Joindate,
                LeftDate = member.Leftdate,
                JoinReason = member.Joinreason,
                PersonalGoal = member.Personalgoal,

                CurrentPosition = member.Boardmembers
                    .FirstOrDefault(bm => bm.Board.Status == "Đang đương nhiệm")
                    ?.Position ?? "Member"
            };
        }

        public async Task RemoveMemberAsync(long membershipId, long currentUserId)
        {
            var membership = await _repo.GetMembershipByIdAsync(membershipId);

            if (membership == null)
                throw new Exception("Không tìm thấy thành viên trong CLB.");

            var isLeader = await _repo.IsLeaderOfClubAsync(currentUserId, membership.Clubid);

            if (!isLeader)
                throw new UnauthorizedAccessException("Chỉ Leader của CLB mới được xóa thành viên.");

            if (membership.Status == "Đã rút lui")
                throw new Exception("Thành viên này đã rút khỏi CLB trước đó.");

            membership.Status = "Đã rút lui";
            membership.Leftdate = DateOnly.FromDateTime(DateTime.Now);

            await _repo.UpdateMembershipAsync(membership);
        }

        public async Task ActivateMemberAsync(string token)
        {
            var (userId, clubId) = _authService.ValidateActivationToken(token);

            var (user, membership, isNewUser, plainPassword) = await _repo.ActivateMemberAsync(userId, clubId);

            var student = user.Userinformation?.Student;
            if (student == null)
                throw new Exception("Không tìm thấy thông tin sinh viên liên kết với tài khoản.");

            string subject = "Kích hoạt tài khoản và xác nhận gia nhập CLB thành công!";
            string body;

            if (isNewUser && !string.IsNullOrWhiteSpace(plainPassword))
            {
                body = $@"
                    <h3>Chào bạn {student.Fullname},</h3>
                    <p>Tài khoản của bạn đã được kích hoạt thành công trên hệ thống Quản lý Câu lạc bộ!</p>
                    <p>Dưới đây là thông tin đăng nhập chính thức của bạn:</p>
                    <ul>
                        <li><strong>Tài khoản (Email):</strong> {student.Schoolemail}</li>
                        <li><strong>Mật khẩu:</strong> {plainPassword}</li>
                    </ul>
                    <p>Vui lòng đổi mật khẩu ngay sau khi đăng nhập lần đầu để đảm bảo an toàn cho tài khoản.</p>
                    <p>Chúc bạn có những trải nghiệm tuyệt vời cùng câu lạc bộ!</p>";
            }
            else
            {
                body = $@"
                    <h3>Chào bạn {student.Fullname},</h3>
                    <p>Bạn đã xác nhận gia nhập câu lạc bộ thành công!</p>
                    <p>Tài khoản của bạn đã ở trạng thái hoạt động. Bạn có thể sử dụng tài khoản và mật khẩu hiện tại của mình để đăng nhập và bắt đầu tham gia sinh hoạt cùng CLB.</p>
                    <p>Chúc bạn có những trải nghiệm tuyệt vời cùng câu lạc bộ!</p>";
            }

            await _emailService.SendEmailAsync(student.Schoolemail, subject, body);
        }
    }
}