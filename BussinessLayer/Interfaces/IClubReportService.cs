using BussinessLayer.DTOs.ClubReport;

namespace BussinessLayer.Interfaces;

public interface IClubReportService
{
    /// <summary>Admin xem toàn bộ báo cáo, filter tuỳ chọn.</summary>
    Task<List<ClubReportResponseDto>> GetAllAsync(long? reportPeriodId, long? clubId, string? status, string role = "ADMIN");

    /// <summary>Admin xem chi tiết 1 báo cáo.</summary>
    Task<ClubReportResponseDto> GetByIdAsync(long clubReportId, string role = "ADMIN");

    /// <summary>Admin duyệt hoặc từ chối báo cáo + ghi feedback.</summary>
    Task<ClubReportResponseDto> ReviewAsync(long clubReportId, ReviewClubReportRequestDto dto);
    
    Task<ClubReportResponseDto> ManagerReviewAsync(long clubReportId, ManagerReviewClubReportRequestDto dto, long managerId);
}
