using AutoMapper;
using CLS.BLL.Common;
using CLS.BLL.DTOs.Attendance;
using CLS.BLL.DTOs.Sessions;
using CLS.BLL.Interfaces;
using CLS.DAL.Entities;
using CLS.DAL.Repositories;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CLS.BLL.Services;

public class AttendanceService : IAttendanceService
{
    private readonly ISessionRepository                      _sessionRepo;
    private readonly IAttendanceRepository                   _attendanceRepo;
    private readonly IMapper                                 _mapper;
    private readonly ILogger<AttendanceService>               _logger;
    private readonly IValidator<SubmitAttendanceRequest>      _submitValidator;

    public AttendanceService(
        ISessionRepository sessionRepo,
        IAttendanceRepository attendanceRepo,
        IMapper mapper,
        ILogger<AttendanceService> logger,
        IValidator<SubmitAttendanceRequest> submitValidator)
    {
        _sessionRepo     = sessionRepo;
        _attendanceRepo  = attendanceRepo;
        _mapper          = mapper;
        _logger          = logger;
        _submitValidator = submitValidator;
    }

    // ── UC-07: Teacher Timetable ─────────────────────────────────────────────
    public async Task<IEnumerable<SessionResponse>> GetTimetableAsync(
        int teacherId, DateTime from, DateTime to, CancellationToken ct = default)
    {
        var sessions = await _sessionRepo.GetTeacherScheduleAsync(teacherId, from, to, ct);

        _logger.LogInformation(
            "Timetable loaded for Teacher {TeacherId}: {Count} sessions from {From} to {To}",
            teacherId, sessions.Count, from, to);

        return _mapper.Map<IEnumerable<SessionResponse>>(sessions);
    }

    // ── UC-08: Get Attendance Sheet ──────────────────────────────────────────
    public async Task<ServiceResult<AttendanceSheetDto>> GetAttendanceSheetAsync(
        int sessionId, int teacherId, CancellationToken ct = default)
    {
        var session = await _sessionRepo.GetByIdWithClassStudentsAsync(sessionId, ct);
        if (session is null)
            return ServiceResult<AttendanceSheetDto>.Fail($"Buổi học #{sessionId} không tồn tại.", 404);

        // Validate ownership
        if (session.TeacherId != teacherId)
            return ServiceResult<AttendanceSheetDto>.Fail(
                "Bạn không có quyền xem buổi học này.", 403);

        // Build student list from class_students junction
        var students = session.Class.ClassStudents
            .Select(cs => new StudentAttendanceItem
            {
                StudentId   = cs.StudentId,
                StudentName = cs.Student.FullName
            })
            .OrderBy(s => s.StudentName)
            .ToList();

        // Check existing attendance records
        var existingRecords = await _attendanceRepo.GetBySessionIdAsync(sessionId, ct);
        var existingDtos = existingRecords.Count > 0
            ? _mapper.Map<List<AttendanceDto>>(existingRecords)
            : null;

        var sheet = new AttendanceSheetDto
        {
            SessionId     = session.Id,
            ClassName     = session.Class.Name,
            TeacherName   = session.Teacher.FullName,
            RoomName      = session.Room.Name,
            StartTime     = session.StartTime,
            EndTime       = session.EndTime,
            SessionStatus = session.Status,
            Students      = students,
            ExistingRecords = existingDtos
        };

        return ServiceResult<AttendanceSheetDto>.Success(sheet);
    }

    // ── UC-08: Submit Attendance ─────────────────────────────────────────────
    public async Task<ServiceResult<object?>> SubmitAttendanceAsync(
        int sessionId, int teacherId, SubmitAttendanceRequest request, CancellationToken ct = default)
    {
        // Validate request
        var validation = await _submitValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return ServiceResult<object?>.Validation(validation.Errors);

        // Load session (tracking mode for status update)
        var session = await _sessionRepo.GetByIdWithDetailsAsync(sessionId, ct);
        if (session is null)
            return ServiceResult<object?>.Fail($"Buổi học #{sessionId} không tồn tại.", 404);

        // Validate ownership
        if (session.TeacherId != teacherId)
            return ServiceResult<object?>.Fail(
                "Bạn không có quyền điểm danh buổi học này.", 403);

        // Validate session status — cho phép scheduled, in_progress, hoặc completed (nếu sửa lại)
        if (session.Status == AppConstants.SessionStatus.Cancelled)
            return ServiceResult<object?>.Fail(
                "Buổi học đã bị hủy, không thể điểm danh.", 409);

        // TODO: Extract to DateTimeHelper if multi-tenant — Ngày session theo giờ Việt Nam (UTC+7)
        var vnOffset = TimeSpan.FromHours(7);
        var localNow = DateTime.UtcNow.Add(vnOffset).Date;
        var localSessionDate = session.StartTime.ToUniversalTime().Add(vnOffset).Date;

        // Chưa đến ngày → không cho điểm danh
        if (localSessionDate > localNow)
            return ServiceResult<object?>.Fail(
                "Chưa đến ngày diễn ra buổi học, không thể điểm danh.", 400);

        // Kiểm tra đã có điểm danh chưa
        var alreadyRecorded = await _attendanceRepo.HasAttendanceAsync(sessionId, ct);
        if (alreadyRecorded)
        {
            // Chỉ cho phép sửa nếu session diễn ra trong ngày hôm nay
            if (localSessionDate != localNow)
                return ServiceResult<object?>.Fail(
                    "Chỉ có thể chỉnh sửa điểm danh trong ngày diễn ra buổi học.", 409);

            // Xóa bản ghi cũ → ghi lại mới
            await _attendanceRepo.DeleteBySessionIdAsync(sessionId, ct);

            _logger.LogInformation(
                "Attendance re-submitted (edit) for Session {SessionId} by Teacher {TeacherId}",
                sessionId, teacherId);
        }

        // Create attendance records
        var now = DateTime.UtcNow;
        var records = request.Records.Select(r => new Attendance
        {
            SessionId  = sessionId,
            StudentId  = r.StudentId,
            Status     = r.Status,
            Note       = r.Note,
            RecordedAt = now
        });

        await _attendanceRepo.AddRangeAsync(records, ct);

        // Update session status → completed
        session.Status = AppConstants.SessionStatus.Completed;
        _sessionRepo.Update(session);

        await _attendanceRepo.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Attendance submitted for Session {SessionId} by Teacher {TeacherId}: {Count} records",
            sessionId, teacherId, request.Records.Count);

        return ServiceResult<object?>.Success(null);
    }
}
