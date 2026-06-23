using BussinessLayer.DTOs.Semester;
using BussinessLayer.Interfaces;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories;

namespace BussinessLayer.Services;

public class SemesterService : ISemesterService
{
    private readonly ISemesterRepository _semesterRepository;

    public SemesterService(ISemesterRepository semesterRepository)
    {
        _semesterRepository = semesterRepository;
    }

    public async Task<List<SemesterResponseDto>> GetAllSemestersAsync()
    {
        var semesters = await _semesterRepository.GetAllAsync();
        return semesters.Select(s => MapToResponseDto(s)).ToList();
    }

    public async Task<SemesterResponseDto> GetSemesterByIdAsync(long id)
    {
        var semester = await _semesterRepository.GetByIdAsync(id);
        if (semester == null)
            throw new Exception("Semester not found.");

        return MapToResponseDto(semester);
    }

    public async Task<SemesterResponseDto> CreateSemesterAsync(CreateSemesterRequestDto requestDto)
    {
        if (requestDto.StartDate >= requestDto.EndDate)
            throw new Exception("Start date must be before end date.");

        bool isOverlapping = await _semesterRepository.HasOverlappingSemesterAsync(requestDto.StartDate, requestDto.EndDate);
        if (isOverlapping)
            throw new Exception("The specified dates overlap with an existing semester.");

        var semester = new Semester
        {
            Semestername = requestDto.SemesterName,
            Description = requestDto.Description,
            Startdate = requestDto.StartDate,
            Enddate = requestDto.EndDate,
            Status = DetermineStatus(requestDto.StartDate, requestDto.EndDate)
        };

        await _semesterRepository.AddAsync(semester);
        return MapToResponseDto(semester);
    }

    public async Task<SemesterResponseDto> UpdateSemesterAsync(long id, UpdateSemesterRequestDto requestDto)
    {
        var semester = await _semesterRepository.GetByIdAsync(id);
        if (semester == null)
            throw new Exception("Semester not found.");

        if (requestDto.StartDate >= requestDto.EndDate)
            throw new Exception("Start date must be before end date.");

        bool isOverlapping = await _semesterRepository.HasOverlappingSemesterAsync(requestDto.StartDate, requestDto.EndDate, id);
        if (isOverlapping)
            throw new Exception("The specified dates overlap with another existing semester.");

        semester.Semestername = requestDto.SemesterName;
        semester.Description = requestDto.Description;
        semester.Startdate = requestDto.StartDate;
        semester.Enddate = requestDto.EndDate;
        semester.Status = requestDto.Status;

        await _semesterRepository.UpdateAsync(semester);
        return MapToResponseDto(semester);
    }

    private SemesterResponseDto MapToResponseDto(Semester semester)
    {
        return new SemesterResponseDto
        {
            SemesterId = semester.Semesterid,
            SemesterName = semester.Semestername,
            Description = semester.Description,
            // Tính động mỗi lần trả về — không dùng status lưu trong DB
            // để tránh stale khi ngày tháng thay đổi
            Status = DetermineStatus(semester.Startdate, semester.Enddate),
            StartDate = semester.Startdate,
            EndDate = semester.Enddate
        };
    }

    private static string DetermineStatus(DateOnly startDate, DateOnly endDate)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        if (today < startDate) return "Dự kiến";
        if (today > endDate) return "Đã kết thúc";
        return "Đang diễn ra";
    }
}
