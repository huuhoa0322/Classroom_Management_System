using AutoMapper;
using CLS.BLL.Common;
using CLS.BLL.DTOs.Sessions;
using CLS.BLL.Interfaces;
using CLS.DAL.Entities;
using CLS.DAL.Repositories;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CLS.BLL.Services;

public class SessionService : ISessionService
{
    private readonly ISessionRepository               _sessionRepo;
    private readonly IClassRepository                 _classRepo;
    private readonly IRoomRepository                  _roomRepo;
    private readonly IUserRepository                  _userRepo;
    private readonly IMapper                          _mapper;
    private readonly ILogger<SessionService>          _logger;
    private readonly IValidator<CreateSessionRequest> _createValidator;
    private readonly IValidator<UpdateSessionRequest> _updateValidator;

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

    public async Task<ServiceResult<SessionResponse>> CreateSessionAsync(
        CreateSessionRequest request, CancellationToken ct = default)
    {
        var validation = await _createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return ServiceResult<SessionResponse>.Validation(validation.Errors);

        var classEntity = await _classRepo.GetByIdAsync(request.ClassId, ct);
        if (classEntity is null)
            return ServiceResult<SessionResponse>.Fail($"Lớp học #{request.ClassId} không tồn tại.", 404);

        var teacher = await _userRepo.GetByIdAsync(request.TeacherId, ct);
        if (teacher is null)
            return ServiceResult<SessionResponse>.Fail($"Giáo viên #{request.TeacherId} không tồn tại.", 404);

        if (teacher.Role != AppConstants.AppRoles.Teacher)
            return ServiceResult<SessionResponse>.Fail(
                $"User #{request.TeacherId} không phải là giáo viên.",
                400);

        var room = await _roomRepo.GetByIdAsync(request.RoomId, ct);
        if (room is null)
            return ServiceResult<SessionResponse>.Fail($"Phòng học #{request.RoomId} không tồn tại.", 404);

        var conflict = await FindConflictAsync(
            request.TeacherId,
            request.RoomId,
            request.StartTime,
            request.EndTime,
            excludeSessionId: null,
            ct);

        if (conflict is not null)
            return ServiceResult<SessionResponse>.Fail(
                conflict.Message,
                conflict.StatusCode,
                conflict.ErrorData);

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
            "Created Session {SessionId}: Class={ClassId}, Teacher={TeacherId}, Room={RoomId}, Time={Start}-{End}",
            session.Id, request.ClassId, request.TeacherId, request.RoomId,
            request.StartTime, request.EndTime);

        var created = await _sessionRepo.GetByIdWithDetailsAsync(session.Id, ct);
        if (created is null)
            return ServiceResult<SessionResponse>.Fail($"Không thể reload Session {session.Id} sau khi tạo.", 500);

        return ServiceResult<SessionResponse>.Success(_mapper.Map<SessionResponse>(created));
    }

    public async Task<ServiceResult<SessionResponse>> UpdateSessionAsync(
        int id, UpdateSessionRequest request, CancellationToken ct = default)
    {
        var validation = await _updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return ServiceResult<SessionResponse>.Validation(validation.Errors);

        var session = await _sessionRepo.GetByIdWithDetailsAsync(id, ct);
        if (session is null)
            return ServiceResult<SessionResponse>.Fail($"Buổi học #{id} không tồn tại.", 404);

        if (session.Status == AppConstants.SessionStatus.Cancelled)
            return ServiceResult<SessionResponse>.Fail("Không thể chỉnh sửa buổi học đã bị hủy.", 409);

        var classEntity = await _classRepo.GetByIdAsync(request.ClassId, ct);
        if (classEntity is null)
            return ServiceResult<SessionResponse>.Fail($"Lớp học #{request.ClassId} không tồn tại.", 404);

        var teacher = await _userRepo.GetByIdAsync(request.TeacherId, ct);
        if (teacher is null)
            return ServiceResult<SessionResponse>.Fail($"Giáo viên #{request.TeacherId} không tồn tại.", 404);

        if (teacher.Role != AppConstants.AppRoles.Teacher)
            return ServiceResult<SessionResponse>.Fail(
                $"User #{request.TeacherId} không phải là giáo viên.",
                400);

        var room = await _roomRepo.GetByIdAsync(request.RoomId, ct);
        if (room is null)
            return ServiceResult<SessionResponse>.Fail($"Phòng học #{request.RoomId} không tồn tại.", 404);

        var conflict = await FindConflictAsync(
            request.TeacherId,
            request.RoomId,
            request.StartTime,
            request.EndTime,
            excludeSessionId: id,
            ct);

        if (conflict is not null)
            return ServiceResult<SessionResponse>.Fail(
                conflict.Message,
                conflict.StatusCode,
                conflict.ErrorData);

        session.ClassId   = request.ClassId;
        session.TeacherId = request.TeacherId;
        session.RoomId    = request.RoomId;
        session.StartTime = request.StartTime;
        session.EndTime   = request.EndTime;

        _sessionRepo.Update(session);
        await _sessionRepo.SaveChangesAsync(ct);

        _logger.LogInformation("Updated Session {SessionId}", id);

        var updated = await _sessionRepo.GetByIdWithDetailsAsync(id, ct);
        if (updated is null)
            return ServiceResult<SessionResponse>.Fail($"Không thể reload Session {id} sau khi cập nhật.", 500);

        return ServiceResult<SessionResponse>.Success(_mapper.Map<SessionResponse>(updated));
    }

    public async Task<ServiceResult<object?>> DeleteSessionAsync(int id, CancellationToken ct = default)
    {
        var session = await _sessionRepo.GetByIdAsync(id, ct);
        if (session is null)
            return ServiceResult<object?>.Fail($"Buổi học #{id} không tồn tại.", 404);

        _sessionRepo.Delete(session);
        await _sessionRepo.SaveChangesAsync(ct);

        _logger.LogInformation("Soft-deleted Session {SessionId}", id);
        return ServiceResult<object?>.Success(null);
    }

    public async Task<PagedResult<SessionResponse>> GetAllSessionsAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var (sessions, total) = await _sessionRepo.GetPagedAllAsync(page, pageSize, ct);
        var items = _mapper.Map<List<SessionResponse>>(sessions);
        return PagedResult<SessionResponse>.Create(items, total, page, pageSize);
    }

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

    private async Task<ServiceResult<object?>?> FindConflictAsync(
        int teacherId, int roomId, DateTime startTime, DateTime endTime,
        int? excludeSessionId, CancellationToken ct)
    {
        var teacherConflict = await _sessionRepo.GetTeacherConflictAsync(
            teacherId, startTime, endTime, excludeSessionId, ct);

        if (teacherConflict is not null)
            return ServiceResult<object?>.Fail(
                "Giáo viên đã được phân công cho một buổi học khác trong khoảng thời gian đã chọn.",
                409,
                BuildConflictData("teacher", teacherConflict));

        var roomConflict = await _sessionRepo.GetRoomConflictAsync(
            roomId, startTime, endTime, excludeSessionId, ct);

        if (roomConflict is not null)
            return ServiceResult<object?>.Fail(
                "Phòng học đã được sử dụng trong khoảng thời gian đã chọn.",
                409,
                BuildConflictData("room", roomConflict));

        return null;
    }

    private static object BuildConflictData(string resourceType, Session conflict)
        => new
        {
            errorCode = "SCHEDULE_CONFLICT",
            resourceType,
            conflictingSession = new
            {
                id = conflict.Id,
                classId = conflict.ClassId,
                className = conflict.Class?.Name ?? string.Empty,
                teacherId = conflict.TeacherId,
                teacherName = conflict.Teacher?.FullName ?? string.Empty,
                roomId = conflict.RoomId,
                roomName = conflict.Room?.Name ?? string.Empty,
                startTime = conflict.StartTime,
                endTime = conflict.EndTime
            }
        };
}
