using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PresentationLayer.BackgroundServices;

public class PendingMemberCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PendingMemberCleanupService> _logger;

    public PendingMemberCleanupService(IServiceProvider serviceProvider, ILogger<PendingMemberCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PendingMemberCleanupService is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredPendingMembersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing pending member cleanup.");
            }

            // Chạy quét định kỳ mỗi 1 giờ
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }

        _logger.LogInformation("PendingMemberCleanupService is stopping.");
    }

    private async Task CleanupExpiredPendingMembersAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ClubSystemDbContext>();

        var now = DateTime.Now;
        var limitDate = DateOnly.FromDateTime(now.AddDays(-1));

        _logger.LogInformation("Starting scanning for expired pending memberships at {Time}...", now);

        // 1. Lấy tất cả lượt đăng ký ở trạng thái "Chờ kích hoạt"
        var pendingMemberships = await context.Memberships
            .Include(m => m.User)
            .Where(m => m.Status == "Chờ kích hoạt")
            .ToListAsync();

        int cleanedMembershipCount = 0;
        int cleanedUserCount = 0;

        foreach (var membership in pendingMemberships)
        {
            bool shouldDeleteMembership = false;
            bool shouldDeleteUser = false;

            if (membership.User.Status == "Chờ kích hoạt")
            {
                // Đối với user mới hoàn toàn: kiểm tra xem tài khoản được tạo quá 24h chưa
                if (membership.User.Createdat.AddHours(24) < now)
                {
                    shouldDeleteMembership = true;
                    shouldDeleteUser = true;
                }
            }
            else
            {
                // Đối với user đã kích hoạt trước đó (đã tham gia CLB khác):
                // kiểm tra xem lượt đăng ký vào CLB mới này đã quá 24h (Joindate trước hôm qua) chưa
                if (membership.Joindate < limitDate)
                {
                    shouldDeleteMembership = true;
                }
            }

            if (shouldDeleteMembership)
            {
                _logger.LogInformation("Cleaning up expired pending membership: User {UserId} in Club {ClubId}", membership.Userid, membership.Clubid);
                context.Memberships.Remove(membership);
                cleanedMembershipCount++;

                if (shouldDeleteUser)
                {
                    _logger.LogInformation("Cleaning up expired pending User account: User {UserId}", membership.Userid);
                    
                    var userInfo = await context.Userinformations
                        .FirstOrDefaultAsync(ui => ui.Userid == membership.Userid);
                    
                    if (userInfo != null)
                    {
                        context.Userinformations.Remove(userInfo);
                    }

                    context.Users.Remove(membership.User);
                    cleanedUserCount++;
                }
            }
        }

        if (cleanedMembershipCount > 0 || cleanedUserCount > 0)
        {
            await context.SaveChangesAsync();
            _logger.LogInformation("Cleanup finished. Deleted {MembershipCount} memberships and {UserCount} users.", cleanedMembershipCount, cleanedUserCount);
        }
        else
        {
            _logger.LogInformation("No expired pending memberships found to clean up.");
        }
    }
}
