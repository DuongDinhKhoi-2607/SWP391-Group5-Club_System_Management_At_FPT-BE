using BussinessLayer.DTOs.Semester;

namespace BussinessLayer.Interfaces;

public interface ISemesterService
{
    Task<List<SemesterResponseDto>> GetAllSemestersAsync();
    Task<SemesterResponseDto> GetSemesterByIdAsync(long id);
    Task<SemesterResponseDto> CreateSemesterAsync(CreateSemesterRequestDto requestDto);
    Task<SemesterResponseDto> UpdateSemesterAsync(long id, UpdateSemesterRequestDto requestDto);
}
