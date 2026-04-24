using AutoMapper;
using CLS.BLL.Common;
using CLS.BLL.Common.Exceptions;
using CLS.BLL.DTOs.Sessions;
using CLS.BLL.Interfaces;
using CLS.DAL.Entities;
using CLS.DAL.Repositories;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CLS.BLL.Services;

public class SessionService : ISessionService
{
    private readonly ISessionRepository                   _sessionRepo;
    private readonly IClassRepository                     _classRepo;
    private readonly IRoomRepository                      _roomRepo;
    private readonly IUserRepository                      _userRepo;
    private readonly IMapper                              _mapper;
    private readonly ILogger<SessionService>              _logger;
    private readonly IValidator<CreateSessionRequest>     _createValidator;
    private readonly IValidator<UpdateSessionRequest>     _updateValidator;

    public SessionService(
        ISessionRepository sessionRepo,
        IClassRepository classRepo,
        IRoomRepository roomRepo,
        IUserRepository userRepo,
        IMapper mapper,
        ILogger<SessionService> logger,
        IValidator<CreateSessionRequest> createValidator,
        IValidator<UpdateSessionRequest> updateValidator)
    {
        _sessionRepo     = sessionRepo;
        _classRepo       = classRepo;
        _roomRepo        = roomRepo;
        _userRepo        = userRepo;
        _mapper          = mapper;
        _logger          = logger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    // ── CREATE SESSION (CLS-004 AC1) ─────────────────────────────────────────
    public async Task<SessionResponse> CreateSessionAsync(
        CreateSessionRequest request, CancellationToken ct = default)
    {
        // BƯỚC 0: Validate input
        var validation = await _createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new Common.Exceptions.ValidationException(
                string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

        // BƯỚC 1: Check Class tồn tại
        _ = await _classRepo.GetByIdAsync(request.ClassId, ct)
            ?? throw new NotFoundException($"Lớp học #{request.ClassId} không tồn tại.");

        // BƯỚC 2: Check Teacher tồn tại
        var teacher = await _userRepo.GetByIdAsync(request.TeacherId, ct)
            ?? throw new NotFoundException($"Giáo viên #{request.TeacherId} không tồn tại.");
        if (teacher.Role != AppConstants.AppRoles.Teacher)
            throw new Common.Exceptions.ValidationException(
                $"User #{request.TeacherId} không phải là giáo viên.");

        // BƯỚC 3: Check Room tồn tại
        _ = await _roomRepo.GetByIdAsync(request.RoomId, ct)
            ?? throw new NotFoundException($"Phòng học #{request.RoomId} không tồn tại.");

        // BƯỚC 4: Conflict Detection (CLS-005)
        await CheckConflictsAsync(request.TeacherId, request.RoomId,
            request.StartTime, request.EndTime, excludeSessionId: null, ct);

        // BƯỚC 5: Tạo Session
        var session = new Session
        {
            ClassId   = request.ClassId,
            TeacherId = request.TeacherId,
            RoomId    = request.RoomId,
            StartTime = request.StartTime,
            EndTime   = request.EndTime,
            Status    = AppConstants.SessionStatus.Scheduled
        };

        await _sessionRepo.AddAsync(session, ct);
        await _sessionRepo.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Created Session {SessionId}: Class={ClassId}, Teacher={TeacherId}, Room={RoomId}, Time={Start}–{End}",
            session.Id, request.ClassId, request.TeacherId, request.RoomId,
            request.StartTime, request.EndTime);

        // Reload with details
        var created = await _sessionRepo.GetByIdWithDetailsAsync(session.Id, ct)
            ?? throw new InvalidOperationException($"Không thể reload Session {session.Id} sau khi tạo.");

        return _mapper.Map<SessionResponse>(created);
    }

    // ── UPDATE SESSION ───────────────────────────────────────────────────────
    public async Task<SessionResponse> UpdateSessionAsync(
        int id, UpdateSessionRequest request, CancellationToken ct = default)
    {
        // Validate
        var validation = await _updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new Common.Exceptions.ValidationException(
                string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

        var session = await _sessionRepo.GetByIdWithDetailsAsync(id, ct)
            ?? throw new NotFoundException($"Buổi học #{id} không tồn tại.");

        if (session.Status == AppConstants.SessionStatus.Cancelled)
            throw new ConflictException("Không thể chỉnh sửa buổi học đã bị hủy.");

        // Check entities
        _ = await _classRepo.GetByIdAsync(request.ClassId, ct)
            ?? throw new NotFoundException($"Lớp học #{request.ClassId} không tồn tại.");

        var teacher = await _userRepo.GetByIdAsync(request.TeacherId, ct)
            ?? throw new NotFoundException($"Giáo viên #{request.TeacherId} không tồn tại.");
        if (teacher.Role != AppConstants.AppRoles.Teacher)
            throw new Common.Exceptions.ValidationException(
                $"User #{request.TeacherId} không phải là giáo viên.");

        _ = await _roomRepo.GetByIdAsync(request.RoomId, ct)
            ?? throw new NotFoundException($"Phòng học #{request.RoomId} không tồn tại.");

        // Conflict Detection — exclude current session
        await CheckConflictsAsync(request.TeacherId, request.RoomId,
            request.StartTime, request.EndTime, excludeSessionId: id, ct);

        // Update
        session.ClassId   = request.ClassId;
        session.TeacherId = request.TeacherId;
        session.RoomId    = request.RoomId;
        session.StartTime = request.StartTime;
        session.EndTime   = request.EndTime;

        _sessionRepo.Update(session);
        await _sessionRepo.SaveChangesAsync(ct);

        _logger.LogInformation("Updated Session {SessionId}", id);

        var updated = await _sessionRepo.GetByIdWithDetailsAsync(id, ct);
        return _mapper.Map<SessionResponse>(updated!);
    }

    // ── DELETE SESSION (soft-delete) ─────────────────────────────────────────
    public async Task DeleteSessionAsync(int id, CancellationToken ct = default)
    {
        var session = await _sessionRepo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Buổi học #{id} không tồn tại.");

        _sessionRepo.Delete(session);
        await _sessionRepo.SaveChangesAsync(ct);

        _logger.LogInformation("Soft-deleted Session {SessionId}", id);
    }

    // ── GET ALL SESSIONS (paged) ─────────────────────────────────────────────
    public async Task<PagedResult<SessionResponse>> GetAllSessionsAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var (sessions, total) = await _sessionRepo.GetPagedAllAsync(page, pageSize, ct);
        var items = _mapper.Map<List<SessionResponse>>(sessions);
        return PagedResult<SessionResponse>.Create(items, total, page, pageSize);
    }

    // ── DROPDOWN DATA ────────────────────────────────────────────────────────
    public async Task<IEnumerable<ClassDto>> GetClassesAsync(CancellationToken ct = default)
    {
        var classes = await _classRepo.GetAllActiveAsync(ct);
        return _mapper.Map<IEnumerable<ClassDto>>(classes);
    }

    public async Task<IEnumerable<RoomDto>> GetRoomsAsync(CancellationToken ct = default)
    {
        var rooms = await _roomRepo.GetAllActiveAsync(ct);
        return _mapper.Map<IEnumerable<RoomDto>>(rooms);
    }

    public async Task<IEnumerable<TeacherDto>> GetTeachersAsync(CancellationToken ct = default)
    {
        var teachers = await _userRepo.GetTeachersAsync(ct);
        return _mapper.Map<IEnumerable<TeacherDto>>(teachers);
    }

    // ── HELPER: Conflict Detection (CLS-005) ─────────────────────────────────
    private async Task CheckConflictsAsync(
        int teacherId, int roomId, DateTime startTime, DateTime endTime,
        int? excludeSessionId, CancellationToken ct)
    {
        // AC1: Teacher double-booking
        if (await _sessionRepo.HasTeacherConflictAsync(teacherId, startTime, endTime, excludeSessionId, ct))
            throw new ConflictException(
                "Scheduling Conflict: This Teacher is already assigned to a session in the selected time range.");

        // AC2: Room overlap
        if (await _sessionRepo.HasRoomConflictAsync(roomId, startTime, endTime, excludeSessionId, ct))
            throw new ConflictException(
                "Scheduling Conflict: This Room is occupied during the selected time window.");
    }
}
