using DataAccessLayer.Models;

namespace DataAccessLayer.Repositories;

public interface IClubReportRepository
{
    /// <summary>Lấy toàn bộ báo cáo, filter tuỳ chọn theo reportPeriodId / clubId / status.</summary>
    Task<List<Clubreport>> GetAllAsync(long? reportPeriodId, long? clubId, string? status);

    /// <summary>Lấy báo cáo dành cho Admin — chỉ các status Admin được phép xem (sau khi Manager đã forward).</summary>
    Task<List<Clubreport>> GetAllForAdminAsync(long? reportPeriodId, long? clubId, string? status, HashSet<string> allowedStatuses);

    /// <summary>Lấy chi tiết 1 báo cáo kèm Club và ReportPeriod.</summary>
    Task<Clubreport?> GetByIdAsync(long clubReportId);

    /// <summary>Cập nhật báo cáo (dùng cho review).</summary>
    Task UpdateAsync(Clubreport report);

    /// <summary>Tạo báo cáo mới.</summary>
    Task AddAsync(Clubreport report);
}
