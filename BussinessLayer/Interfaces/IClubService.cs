using BussinessLayer.DTOs;
using DataAccessLayer.Models;
using System.Threading.Tasks;

namespace BussinessLayer.Interfaces
{
    public interface IClubService
    {
        Task<Club> CreateClubAsync(CreateClubDto dto);
    }
}
