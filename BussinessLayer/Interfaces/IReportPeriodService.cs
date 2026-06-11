using BussinessLayer.DTOs.ReportPeriod;

namespace BussinessLayer.Interfaces;

public interface IReportPeriodService
{
    Task<List<ReportPeriodResponseDto>> GetAllReportPeriodsAsync(long? semesterId);
    Task<ReportPeriodResponseDto> GetReportPeriodByIdAsync(long id);
    Task<ReportPeriodResponseDto> CreateReportPeriodAsync(CreateReportPeriodRequestDto requestDto);
    Task<ReportPeriodResponseDto> UpdateReportPeriodAsync(long id, UpdateReportPeriodRequestDto requestDto);
}
