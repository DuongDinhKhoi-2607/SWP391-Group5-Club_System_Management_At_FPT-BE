using DataAccessLayer.Models;

namespace DataAccessLayer.Repositories
{
    public interface IClubMemberListRepository
    {
        Task<bool> IsManagerOfClubAsync(long userId, long clubId);
        Task<List<Membership>> GetActiveMembersByClubAsync(
            long clubId);

        Task<Student?> GetStudentByIdAsync(
            string studentId);

        Task<User?> GetUserByStudentIdAsync(
            string studentId);

        Task<User?> GetUserByEmailAsync(
            string email);

        Task<bool> IsActiveMemberAsync(
            long userId,
            long clubId);

        Task<Membership> AddMemberByStudentIdAsync(
            Student student,
            long clubId,
            string? joinReason,
            string? personalGoal);

        // ===== Xem chi tiết =====
        Task<Membership?> GetMemberDetailByMembershipIdAsync(
            long membershipId);

        // ===== Xóa mềm =====
        Task<Membership?> GetMembershipByIdAsync(
            long membershipId);

        Task UpdateMembershipAsync(
            Membership membership);

        Task<(User user, Membership membership, bool isNewUser, string? plainPassword)> ActivateMemberAsync(long userId, long clubId);
    }
}