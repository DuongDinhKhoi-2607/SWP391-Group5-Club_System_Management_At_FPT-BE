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

        public async Task<List<Event>> GetAllEventsAsync(string? statusFilter)
        {
            return await _repo.GetAllAsync(statusFilter);
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

        public async Task<Event> ApproveEventAsync(long eventId)
        {
            var ev = await _repo.GetByIdAsync(eventId)
                     ?? throw new Exception($"Không tìm thấy sự kiện ID = {eventId}.");

            if (ev.Status != "Chờ duyệt")
                throw new Exception($"Sự kiện đang ở trạng thái '{ev.Status}', không thể duyệt.");

            if (string.IsNullOrWhiteSpace(ev.Location))
                throw new Exception("Sự kiện này chưa có địa điểm tổ chức, không thể duyệt.");

            // ── Kiểm tra 1: Trùng địa điểm + thời gian ──────────────────────
            var locationConflict = await _repo.GetConflictByLocationAsync(
                eventId, ev.Location!, ev.Starttime, ev.Endtime);

            if (locationConflict != null)
                throw new Exception(
                    $"Không thể duyệt: Địa điểm '{ev.Location}' đã có sự kiện " +
                    $"'{locationConflict.Eventname}' (CLB: {locationConflict.Club?.Clubname}) " +
                    $"vào lúc {locationConflict.Starttime:dd/MM/yyyy HH:mm} - {locationConflict.Endtime:HH:mm}.");

            // ── Kiểm tra 2: Cùng CLB trùng thời gian ────────────────────────
            var clubConflict = await _repo.IsEventTimeConflictAsync(
                ev.Clubid, ev.Starttime, ev.Endtime, eventId);

            if (clubConflict)
                throw new Exception(
                    $"Không thể duyệt: CLB đã có sự kiện khác bị trùng thời gian. " +
                    $"Một CLB không thể tổ chức 2 sự kiện cùng lúc.");

            ev.Status = "Đã duyệt";
            await _repo.UpdateAsync(ev);
            return ev;
        }

        public async Task<Event> RejectEventAsync(long eventId, RejectEventDto dto)
        {
            var ev = await _repo.GetByIdAsync(eventId)
                     ?? throw new Exception($"Không tìm thấy sự kiện ID = {eventId}.");

            if (ev.Status != "Chờ duyệt")
                throw new Exception($"Sự kiện đang ở trạng thái '{ev.Status}', không thể từ chối.");

            if (string.IsNullOrWhiteSpace(dto.RejectReason))
                throw new Exception("Lý do từ chối không được để trống.");

            ev.Status = "Bị từ chối";
            await _repo.UpdateAsync(ev);
            return ev;
        }
    }
}