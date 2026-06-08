using BussinessLayer.DTOs;
using BussinessLayer.Interfaces;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories;

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
            dto.StartTime = DateTime.SpecifyKind(dto.StartTime, DateTimeKind.Unspecified);
            dto.EndTime = DateTime.SpecifyKind(dto.EndTime, DateTimeKind.Unspecified);

            if (dto.StartTime >= dto.EndTime)
                throw new Exception("Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc.");

            var duplicate = await _repo.ExistsDuplicateEventAsync(
                dto.ClubId,
                dto.EventName,
                dto.StartTime
            );

            if (duplicate)
                throw new Exception("Sự kiện này đã tồn tại.");

            var conflict = await _repo.IsEventTimeConflictAsync(
                dto.ClubId,
                dto.StartTime,
                dto.EndTime
            );

            if (conflict)
                throw new Exception("CLB đã có sự kiện trong khoảng thời gian này.");

            var newEvent = new Event
            {
                Clubid = dto.ClubId,
                Eventname = dto.EventName,
                Description = dto.Description,
                Location = dto.Location,
                Planbudget = dto.PlanBudget,
                Targetparticipants = dto.TargetParticipants,
                Actualparticipants = 0,
                Status = "Chờ duyệt",
                Starttime = dto.StartTime,
                Endtime = dto.EndTime
            };

            return await _repo.CreateAsync(newEvent);
        }

        public async Task<Event?> GetEventByIdAsync(long eventId)
        {
            return await _repo.GetByIdAsync(eventId);
        }

        public async Task<List<Event>> GetEventsByClubAsync(long clubId)
        {
            return await _repo.GetByClubIdAsync(clubId);
        }

        public async Task<List<Event>> GetApprovedEventsByClubAsync(long clubId)
        {
            return await _repo.GetApprovedByClubIdAsync(clubId);
        }

        public async Task<Event> UpdateEventAsync(long eventId, UpdateEventDto dto)
        {
            var ev = await _repo.GetByIdAsync(eventId);

            if (ev == null)
                throw new Exception("Không tìm thấy sự kiện.");

            if (ev.Status == "Đã kết thúc")
                throw new Exception("Sự kiện đã kết thúc, không thể sửa.");

            if (ev.Status == "Đã hủy")
                throw new Exception("Sự kiện đã hủy, không thể sửa.");

            dto.StartTime = DateTime.SpecifyKind(dto.StartTime, DateTimeKind.Unspecified);
            dto.EndTime = DateTime.SpecifyKind(dto.EndTime, DateTimeKind.Unspecified);

            if (dto.StartTime >= dto.EndTime)
                throw new Exception("Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc.");

            var duplicate = await _repo.ExistsDuplicateEventAsync(
                ev.Clubid,
                dto.EventName,
                dto.StartTime,
                eventId
            );

            if (duplicate)
                throw new Exception("Sự kiện này đã tồn tại.");

            var conflict = await _repo.IsEventTimeConflictAsync(
                ev.Clubid,
                dto.StartTime,
                dto.EndTime,
                eventId
            );

            if (conflict)
                throw new Exception("CLB đã có sự kiện khác trong khoảng thời gian này.");

            ev.Eventname = dto.EventName;
            ev.Description = dto.Description;
            ev.Location = dto.Location;
            ev.Planbudget = dto.PlanBudget;
            ev.Targetparticipants = dto.TargetParticipants;
            ev.Starttime = dto.StartTime;
            ev.Endtime = dto.EndTime;

            if (ev.Status == "Đã duyệt")
                ev.Status = "Chờ duyệt";

            await _repo.UpdateAsync(ev);

            return ev;
        }

        public async Task CancelEventAsync(long eventId)
        {
            var ev = await _repo.GetByIdAsync(eventId);

            if (ev == null)
                throw new Exception("Không tìm thấy sự kiện.");

            if (ev.Status == "Đã kết thúc")
                throw new Exception("Sự kiện đã kết thúc, không thể hủy.");

            if (ev.Status == "Đã hủy")
                throw new Exception("Sự kiện đã được hủy trước đó.");

            ev.Status = "Đã hủy";

            await _repo.UpdateAsync(ev);
        }
    }
}