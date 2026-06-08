using BussinessLayer.DTOs;
using BussinessLayer.Interfaces;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories;
using System;
using System.Collections.Generic;
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

        // ─── Manager tạo sự kiện ─────────────────────────────────────────────
        public async Task<Event> CreateEventAsync(CreateEventDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Location))
                throw new Exception("Địa điểm tổ chức không được để trống.");

            if (dto.StartTime >= dto.EndTime)
                throw new Exception("Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc.");

            if (dto.StartTime < DateTime.Now)
                throw new Exception("Thời gian bắt đầu phải ở tương lai.");

            var newEvent = new Event
            {
                Clubid             = dto.ClubId,
                Eventname          = dto.EventName,
                Description        = dto.Description,
                Location           = dto.Location,
                Planbudget         = dto.PlanBudget,
                Targetparticipants = dto.TargetParticipants,
                Actualparticipants = 0,
                Status             = "Chờ duyệt",   // gửi lên Admin
                Starttime          = dto.StartTime,
                Endtime            = dto.EndTime
            };

            return await _repo.CreateAsync(newEvent);
        }

        // ─── Lấy danh sách sự kiện ───────────────────────────────────────────
        public async Task<List<Event>> GetAllEventsAsync(string? statusFilter)
        {
            return await _repo.GetAllAsync(statusFilter);
        }

        // ─── Admin duyệt sự kiện (có kiểm tra xung đột) ──────────────────────
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
                    $"'{locationConflict.Eventname}' (CLB: {locationConflict.Club.Clubname}) " +
                    $"vào lúc {locationConflict.Starttime:dd/MM/yyyy HH:mm} - {locationConflict.Endtime:HH:mm}.");

            // ── Kiểm tra 2: Cùng CLB trùng thời gian ────────────────────────
            var clubConflict = await _repo.GetConflictByClubAsync(
                eventId, ev.Clubid, ev.Starttime, ev.Endtime);

            if (clubConflict != null)
                throw new Exception(
                    $"Không thể duyệt: CLB '{ev.Club.Clubname}' đã có sự kiện " +
                    $"'{clubConflict.Eventname}' vào lúc " +
                    $"{clubConflict.Starttime:dd/MM/yyyy HH:mm} - {clubConflict.Endtime:HH:mm}. " +
                    $"Một CLB không thể tổ chức 2 sự kiện cùng lúc.");

            return await _repo.UpdateStatusAsync(ev, "Đã duyệt");
        }

        // ─── Admin từ chối sự kiện ────────────────────────────────────────────
        public async Task<Event> RejectEventAsync(long eventId, RejectEventDto dto)
        {
            var ev = await _repo.GetByIdAsync(eventId)
                     ?? throw new Exception($"Không tìm thấy sự kiện ID = {eventId}.");

            if (ev.Status != "Chờ duyệt")
                throw new Exception($"Sự kiện đang ở trạng thái '{ev.Status}', không thể từ chối.");

            if (string.IsNullOrWhiteSpace(dto.RejectReason))
                throw new Exception("Lý do từ chối không được để trống.");

            return await _repo.UpdateStatusAsync(ev, "Bị từ chối");
        }
    }
}
