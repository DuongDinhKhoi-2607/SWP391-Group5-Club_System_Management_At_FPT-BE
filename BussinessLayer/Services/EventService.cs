using BussinessLayer.DTOs;
using BussinessLayer.Interfaces;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories;
using System;
using System.Threading.Tasks;

namespace BussinessLayer.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _repo;

        public EventService(IEventRepository repo)
        {
            _repo = repo;
        }

        public async Task<Event> CreateEventAsync(CreateEventDto dto, long currentUserId)
        {
            if (dto.StartTime >= dto.EndTime)
                throw new Exception("Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc.");

            //var canCreate = await _repo.CanCreateEventAsync(currentUserId, dto.ClubId);

            //if (!canCreate)
            //    throw new UnauthorizedAccessException("Bạn không có quyền tạo sự kiện cho CLB này.");

            var newEvent = new Event
            {
                Clubid = dto.ClubId,
                Eventname = dto.EventName,
                Description = dto.Description,
                Location = dto.Location,
                Planbudget = dto.PlanBudget,
                Targetparticipants = dto.TargetParticipants,
                Actualparticipants = 0,
                Status = "Lập kế hoạch",
                Starttime = dto.StartTime,
                Endtime = dto.EndTime
            };

            return await _repo.CreateAsync(newEvent);
        }
    }
}