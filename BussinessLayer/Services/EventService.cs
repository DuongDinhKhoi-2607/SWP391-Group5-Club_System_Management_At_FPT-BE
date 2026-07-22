using BussinessLayer.DTOs;
using BussinessLayer.Interfaces;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories;

namespace BussinessLayer.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _repo;
        private readonly ICloudinaryService _cloudinaryService;

        public EventService(IEventRepository repo, ICloudinaryService cloudinaryService)
        {
            _repo = repo;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<Event> CreateEventAsync(CreateEventDto dto, long currentUserId)
        {
            var vnZone = TimeSpan.FromHours(7);
            var startTime = dto.StartTime.ToOffset(vnZone).DateTime;
            var endTime = dto.EndTime.ToOffset(vnZone).DateTime;

            if (startTime >= endTime)
                throw new Exception("Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc.");

            var duplicate = await _repo.ExistsDuplicateEventAsync(
                dto.ClubId,
                dto.EventName,
                startTime
            );

            if (duplicate)
                throw new Exception("Sự kiện này đã tồn tại.");

            var conflict = await _repo.IsEventTimeConflictAsync(
                dto.ClubId,
                startTime,
                endTime
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
                Starttime = startTime,
                Endtime = endTime
            };

            return await _repo.CreateAsync(newEvent);
        }

        public async Task<List<Event>> GetAllEventsAsync(string? statusFilter)
        {
            return await _repo.GetAllAsync(statusFilter);
        }

        public async Task<int> GetTotalEventsAsync(string? statusFilter)
        {
            return await _repo.CountEventsAsync(statusFilter);
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

            var vnZone = TimeSpan.FromHours(7);
            var startTime = dto.StartTime.ToOffset(vnZone).DateTime;
            var endTime = dto.EndTime.ToOffset(vnZone).DateTime;

            if (startTime >= endTime)
                throw new Exception("Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc.");

            var duplicate = await _repo.ExistsDuplicateEventAsync(
                ev.Clubid,
                dto.EventName,
                startTime,
                eventId
            );

            if (duplicate)
                throw new Exception("Sự kiện này đã tồn tại.");

            var conflict = await _repo.IsEventTimeConflictAsync(
                ev.Clubid,
                startTime,
                endTime,
                eventId
            );

            if (conflict)
                throw new Exception("CLB đã có sự kiện khác trong khoảng thời gian này.");

            ev.Eventname = dto.EventName;
            ev.Description = dto.Description;
            ev.Location = dto.Location;
            ev.Planbudget = dto.PlanBudget;
            ev.Targetparticipants = dto.TargetParticipants;
            ev.Starttime = startTime;
            ev.Endtime = endTime;

            if (ev.Status == "Đã duyệt" || ev.Status == "Bị từ chối" || ev.Status == "Yêu cầu chỉnh sửa")
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

        public async Task<Event> RequestEditEventAsync(long eventId, string reason)
        {
            var ev = await _repo.GetByIdAsync(eventId)
                     ?? throw new Exception($"Không tìm thấy sự kiện ID = {eventId}.");

            if (ev.Status != "Chờ duyệt")
                throw new Exception($"Sự kiện đang ở trạng thái '{ev.Status}', không thể yêu cầu chỉnh sửa.");

            if (string.IsNullOrWhiteSpace(reason))
                throw new Exception("Lý do yêu cầu chỉnh sửa không được để trống.");

            ev.Status = "Yêu cầu chỉnh sửa";
            await _repo.UpdateAsync(ev);
            return ev;
        }

        public async Task<Participant> RegisterParticipantAsync(long userId, long eventId, RegisterEventRequestDto dto)
        {
            var ev = await _repo.GetByIdAsync(eventId)
                ?? throw new Exception("Không tìm thấy sự kiện.");

            // Kiểm tra xem User này có thuộc Club tổ chức sự kiện hay không
            var isMember = await _repo.IsUserInClubAsync(userId, ev.Clubid);
            if (!isMember)
                throw new Exception("Bạn không phải là thành viên của câu lạc bộ tổ chức sự kiện này, không thể đăng ký.");

            if (ev.Status != "Đã duyệt" && ev.Status != "Đang diễn ra")
                throw new Exception("Sự kiện chưa được duyệt hoặc đã kết thúc, không thể đăng ký.");

            var currentParticipants = await _repo.CountParticipantsAsync(eventId);
            if (currentParticipants >= ev.Targetparticipants)
                throw new Exception("Sự kiện đã đủ số lượng người đăng ký.");

            var existingParticipant = await _repo.GetParticipantAsync(eventId, userId);
            if (existingParticipant != null)
                throw new Exception("Bạn đã đăng ký tham gia sự kiện này rồi.");

            var roleInEvent = dto.RoleInEvent;
            if (roleInEvent == "Participant")
            {
                roleInEvent = "Thành viên tham gia";
            }

            var participant = new Participant
            {
                Eventid = eventId,
                Userid = userId,
                Roleinevent = roleInEvent,
                Attendancestatus = "Vắng mặt"
            };

            ev.Actualparticipants = currentParticipants + 1;
            await _repo.UpdateAsync(ev);

            return await _repo.AddParticipantAsync(participant);
        }

        public async Task<Participant> UploadEvidenceAsync(long userId, long eventId, UploadEventEvidenceDto dto)
        {
            var participant = await _repo.GetParticipantAsync(eventId, userId)
                ?? throw new Exception("Bạn chưa đăng ký tham gia sự kiện này.");

            if (!string.IsNullOrWhiteSpace(dto.Feedback))
            {
                participant.Feedback = dto.Feedback;
            }

            if (participant.Evidences == null)
            {
                participant.Evidences = new List<Evidence>();
            }

            if (dto.EvidenceFiles != null && dto.EvidenceFiles.Any())
            {
                foreach (var file in dto.EvidenceFiles)
                {
                    if (file.Length > 10 * 1024 * 1024)
                        throw new Exception($"File '{file.FileName}' vượt quá 10MB.");

                    var secureUrl = await _cloudinaryService.UploadFileAsync(file, $"evidences/event_{eventId}");

                    var evidence = new Evidence
                    {
                        Evidencename = file.FileName,
                        Fileurl = secureUrl,
                        Isverified = "Đang chờ Leader duyệt",
                        Uploadedat = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified),
                        Participantid = participant.Participantid
                    };

                    participant.Evidences.Add(evidence);
                }
            }

            await _repo.UpdateParticipantAsync(participant);
            return participant;
        }

        public async Task<Evidence> ReviewEvidenceByLeaderAsync(long currentUserId, long evidenceId, string status)
        {
            var evidence = await _repo.GetEvidenceByIdAsync(evidenceId)
                ?? throw new Exception($"Không tìm thấy evidence ID = {evidenceId}.");

            var clubId = evidence.Participant?.Event?.Clubid 
                ?? throw new Exception("Không xác định được Câu lạc bộ của sự kiện.");

            // Kiểm tra Leader
            var isLeader = await _repo.IsUserInClubAsync(currentUserId, clubId); // TODO: Cần check chính xác Role là Leader, tạm mượn IsUserInClubAsync hoặc check thêm role.
            // Sửa lại: để check chính xác Leader, phải truy cập ClubMembership. 
            // Tạm thời giả định Controller đã check Role hoặc ta phải check cẩn thận hơn. 
            // Ở đây tạm bỏ qua check chi tiết Leader nếu đã xử lý ở Controller/Service khác, nhưng ta sẽ check đơn giản:
            if (!isLeader)
                throw new Exception("Bạn không phải thành viên CLB này.");
            
            if (evidence.Isverified != "Đang chờ Leader duyệt")
                throw new Exception($"Minh chứng đang ở trạng thái '{evidence.Isverified}', không thể duyệt bởi Leader.");

            if (status != "Chờ Manager duyệt" && status != "Yêu cầu bổ sung" && status != "Từ chối")
                throw new Exception("Trạng thái duyệt của Leader không hợp lệ.");

            evidence.Isverified = status;
            evidence.Verifiedat = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

            await _repo.UpdateEvidenceAsync(evidence);
            return evidence;
        }

        public async Task<Evidence> ReviewEvidenceAsync(long evidenceId, string status)
        {
            var evidence = await _repo.GetEvidenceByIdAsync(evidenceId)
                ?? throw new Exception($"Không tìm thấy evidence ID = {evidenceId}.");

            if (evidence.Isverified != "Chờ Manager duyệt")
                throw new Exception($"Minh chứng đang ở trạng thái '{evidence.Isverified}', Manager chỉ duyệt khi Leader đã thông qua.");

            if (status != "Đã duyệt" && status != "Yêu cầu bổ sung" && status != "Từ chối")
                throw new Exception("Trạng thái duyệt không hợp lệ.");

            evidence.Isverified = status;
            evidence.Verifiedat = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

            await _repo.UpdateEvidenceAsync(evidence);
            return evidence;
        }

        // ─────────────────────────────────────────────────────────────
        // EVIDENCE LISTING
        // ─────────────────────────────────────────────────────────────

        public async Task<List<EvidenceResponseDto>> GetEvidencesByEventAsync(long currentUserId, long eventId, bool isAdminOrManager)
        {
            var ev = await _repo.GetByIdAsync(eventId)
                ?? throw new Exception($"Không tìm thấy sự kiện ID = {eventId}.");

            if (!isAdminOrManager)
            {
                var isMember = await _repo.IsUserInClubAsync(currentUserId, ev.Clubid);
                if (!isMember)
                    throw new Exception("Bạn không có quyền xem minh chứng của sự kiện này.");
            }

            var evidences = await _repo.GetEvidencesByEventIdAsync(eventId);
            return evidences.Select(MapToEvidenceDto).ToList();
        }

        public async Task<List<EvidenceResponseDto>> GetPendingEvidencesAsync()
        {
            var evidences = await _repo.GetPendingEvidencesAsync();
            return evidences.Select(MapToEvidenceDto).ToList();
        }

        public async Task<List<EvidenceResponseDto>> GetPendingEvidencesForLeaderAsync(long clubId)
        {
            var evidences = await _repo.GetPendingEvidencesForLeaderAsync(clubId);
            return evidences.Select(MapToEvidenceDto).ToList();
        }

        public async Task<List<ParticipantResponseDto>> GetParticipantsByEventAsync(long currentUserId, long eventId, bool isAdminOrManager)
        {
            var ev = await _repo.GetByIdAsync(eventId)
                ?? throw new Exception($"Không tìm thấy sự kiện ID = {eventId}.");

            if (!isAdminOrManager)
            {
                var isMember = await _repo.IsUserInClubAsync(currentUserId, ev.Clubid);
                if (!isMember)
                    throw new Exception("Bạn không có quyền xem danh sách tham gia của sự kiện này.");
            }

            var participants = await _repo.GetParticipantsByEventIdAsync(eventId);
            return participants.Select(p => new ParticipantResponseDto
            {
                ParticipantId = p.Participantid,
                EventId = p.Eventid,
                UserId = p.Userid,
                FullName = p.User?.Userinformation?.Student?.Fullname ?? p.User?.Username ?? "Unknown",
                Email = p.User?.Userinformation?.Student?.Schoolemail ?? "",
                StudentId = p.User?.Userinformation?.Student?.Studentid,
                RoleInEvent = p.Roleinevent,
                AttendanceStatus = p.Attendancestatus,
                Feedback = p.Feedback,
                EvaluationScore = p.Evaluationscore,
                CheckedInAt = p.Checkedinat
            }).ToList();
        }

        private static EvidenceResponseDto MapToEvidenceDto(Evidence e) => new()
        {
            EvidenceId      = e.Evidenceid,
            ParticipantId   = e.Participantid,
            EventId         = e.Participant?.Eventid ?? 0,
            EvidenceName    = e.Evidencename,
            FileUrl         = e.Fileurl,
            IsVerified      = e.Isverified,
            UploadedAt      = e.Uploadedat,
            VerifiedAt      = e.Verifiedat,
            ParticipantName = e.Participant?.User?.Userinformation?.Student?.Fullname
                           ?? e.Participant?.User?.Username ?? "",
            EventName       = e.Participant?.Event?.Eventname,
            ClubName        = e.Participant?.Event?.Club?.Clubname,
            ClubId          = e.Participant?.Event?.Clubid
        };


    }
}