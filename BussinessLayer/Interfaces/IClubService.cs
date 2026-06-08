using BussinessLayer.DTOs;
using DataAccessLayer.Models;

namespace BussinessLayer.Interfaces
{
    public interface IClubService
    {
        Task<Club> CreateClubAsync(CreateClubDto dto);
        Task<Club> UpdateClubAsync(long clubId, UpdateClubDto dto, long currentUserId);
    }
}