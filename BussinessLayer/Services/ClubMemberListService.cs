using BussinessLayer.DTOs;
using BussinessLayer.Interfaces;
using DataAccessLayer.Repositories;

namespace BussinessLayer.Services
{
    public class ClubMemberListService : IClubMemberListService
    {
        private readonly IClubMemberListRepository _repo;

        public ClubMemberListService(IClubMemberListRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<ClubMemberListDto>> GetActiveMembersByClubAsync(
            long clubId,
            long currentUserId)
        {
            var isLeader = await _repo.IsManagerOfClubAsync(currentUserId, clubId);

            if (!isLeader)
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
        public async Task<ClubMemberListDto> AddMemberByStudentIdAsync(
    AddClubMemberDto dto,
    long currentUserId)
        {
            var isLeader = await _repo.IsManagerOfClubAsync(currentUserId, dto.ClubId);

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
                    dto.ClubId
                );

                if (isAlreadyMember)
                    throw new Exception("Sinh viên này đã là thành viên của CLB.");
            }

            var membership = await _repo.AddMemberByStudentIdAsync(
                student,
                dto.ClubId,
                dto.JoinReason,
                dto.PersonalGoal
            );

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

            var isLeader = await _repo.IsManagerOfClubAsync(currentUserId, membership.Clubid);

            if (!isLeader)
                throw new UnauthorizedAccessException("Chỉ Manager của CLB mới được xóa thành viên.");

            if (membership.Status == "Đã rút lui")
                throw new Exception("Thành viên này đã rút khỏi CLB trước đó.");

            membership.Status = "Đã rút lui";
            membership.Leftdate = DateOnly.FromDateTime(DateTime.Now);

            await _repo.UpdateMembershipAsync(membership);
        }
    }
}