using BussinessLayer.DTOs.ReportPeriod;
using BussinessLayer.DTOs.Semester;
using BussinessLayer.DTOs.Notification;
using BussinessLayer.Interfaces;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories;

namespace BussinessLayer.Services;

public class ReportPeriodService : IReportPeriodService
{
    private readonly IReportPeriodRepository _reportPeriodRepository;
    private readonly ISemesterRepository _semesterRepository;
    private readonly INotificationService _notificationService;

    public ReportPeriodService(IReportPeriodRepository reportPeriodRepository, ISemesterRepository semesterRepository, INotificationService notificationService)
    {
        _reportPeriodRepository = reportPeriodRepository;
        _semesterRepository = semesterRepository;
        _notificationService = notificationService;
    }

    public async Task<List<ReportPeriodResponseDto>> GetAllReportPeriodsAsync(long? semesterId)
    {
        var periods = await _reportPeriodRepository.GetAllAsync(semesterId);
        return periods.Select(p => MapToResponseDto(p)).ToList();
    }

    public async Task<ReportPeriodResponseDto> GetReportPeriodByIdAsync(long id)
    {
        var period = await _reportPeriodRepository.GetByIdAsync(id);
        if (period == null)
            throw new Exception("Report period not found.");

        return MapToResponseDto(period);
    }

    public async Task<ReportPeriodResponseDto> CreateReportPeriodAsync(CreateReportPeriodRequestDto requestDto, long adminId)
    {
        var semester = await _semesterRepository.GetByIdAsync(requestDto.SemesterId);
        if (semester == null)
            throw new Exception("Semester not found.");

        ValidateDeadline(requestDto.Deadline, semester.Startdate, semester.Enddate);

        var period = new Reportperiod
        {
            Semesterid = requestDto.SemesterId,
            Periodname = requestDto.PeriodName,
            Description = requestDto.Description,
            Deadline = DateTime.SpecifyKind(requestDto.Deadline, DateTimeKind.Unspecified),
            Status = "Mở cổng nhận",
            Createdat = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified)
        };

        await _reportPeriodRepository.AddAsync(period);
        period.Semester = semester; // Set for response mapping

        // TỰ ĐỘNG GỬI THÔNG BÁO Ở ĐÂY
        var notiDto = new CreateNotificationDto
        {
            Title = $"Mở kỳ báo cáo mới: {period.Periodname}",
            Content = $"Yêu cầu các CLB nộp báo cáo trước ngày {period.Deadline:dd/MM/yyyy}. {period.Description}",
            NotificationType = "Báo cáo",
            TargetType = "Toàn hệ thống",
            ReportPeriodId = period.Reportperiodid
        };
        await _notificationService.SendNotificationAsync(adminId, notiDto);

        return MapToResponseDto(period);
    }

    public async Task<ReportPeriodResponseDto> UpdateReportPeriodAsync(long id, UpdateReportPeriodRequestDto requestDto)
    {
        var period = await _reportPeriodRepository.GetByIdAsync(id);
        if (period == null)
            throw new Exception("Report period not found.");

        var semester = await _semesterRepository.GetByIdAsync(requestDto.SemesterId);
        if (semester == null)
            throw new Exception("Semester not found.");

        ValidateDeadline(requestDto.Deadline, semester.Startdate, semester.Enddate);

        period.Semesterid = requestDto.SemesterId;
        period.Periodname = requestDto.PeriodName;
        period.Description = requestDto.Description;
        period.Deadline = DateTime.SpecifyKind(requestDto.Deadline, DateTimeKind.Unspecified);
        period.Status = requestDto.Status;

        await _reportPeriodRepository.UpdateAsync(period);
        period.Semester = semester; // Ensure latest info
        return MapToResponseDto(period);
    }

    private void ValidateDeadline(DateTime deadline, DateOnly semesterStart, DateOnly semesterEnd)
    {
        var deadlineDateOnly = DateOnly.FromDateTime(deadline);
        if (deadlineDateOnly < semesterStart || deadlineDateOnly > semesterEnd)
        {
            throw new Exception($"Deadline must be between semester start ({semesterStart}) and end ({semesterEnd}).");
        }
    }

    private ReportPeriodResponseDto MapToResponseDto(Reportperiod period)
    {
        var dto = new ReportPeriodResponseDto
        {
            ReportPeriodId = period.Reportperiodid,
            SemesterId = period.Semesterid,
            PeriodName = period.Periodname,
            Description = period.Description,
            Deadline = period.Deadline,
            Status = period.Status,
            CreatedAt = period.Createdat
        };

        if (period.Semester != null)
        {
            dto.Semester = new SemesterResponseDto
            {
                SemesterId = period.Semester.Semesterid,
                SemesterName = period.Semester.Semestername,
                Description = period.Semester.Description,
                Status = period.Semester.Status,
                StartDate = period.Semester.Startdate,
                EndDate = period.Semester.Enddate
            };
        }

        return dto;
    }

    public async Task<int> GetPendingReportsCountAsync()
    {
        return await _reportPeriodRepository.CountPendingReportsAsync();
    }
}
