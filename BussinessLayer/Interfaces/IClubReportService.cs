using BussinessLayer.DTOs.ClubReport;

namespace BussinessLayer.Interfaces;

public interface IClubReportService
{
    /// <summary>Admin xem toàn bộ báo cáo, filter tuỳ chọn.</summary>
    Task<List<ClubReportResponseDto>> GetAllAsync(long? reportPeriodId, long? clubId, string? status);

    /// <summary>Admin xem chi tiết 1 báo cáo.</summary>
    Task<ClubReportResponseDto> GetByIdAsync(long clubReportId);

    /// <summary>Admin duyệt hoặc từ chối báo cáo + ghi feedback.</summary>
    Task<ClubReportResponseDto> ReviewAsync(long clubReportId, ReviewClubReportRequestDto dto);
}
