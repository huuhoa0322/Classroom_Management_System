using AutoMapper;
using CLS.BLL.Common;
using CLS.BLL.DTOs.Feedback;
using CLS.BLL.Interfaces;
using CLS.DAL.Entities;
using CLS.DAL.Repositories;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CLS.BLL.Services;

public class FeedbackService : IFeedbackService
{
    private readonly ISessionRepository                    _sessionRepo;
    private readonly IFeedbackRepository                   _feedbackRepo;
    private readonly IMapper                               _mapper;
    private readonly ILogger<FeedbackService>               _logger;
    private readonly IValidator<SubmitFeedbackRequest>      _submitValidator;

    /// <summary>SLA window: 12 hours after session end.</summary>
    private static readonly TimeSpan SlaWindow = TimeSpan.FromHours(12);

    public FeedbackService(
        ISessionRepository sessionRepo,
        IFeedbackRepository feedbackRepo,
        IMapper mapper,
        ILogger<FeedbackService> logger,
        IValidator<SubmitFeedbackRequest> submitValidator)
    {
        _sessionRepo     = sessionRepo;
        _feedbackRepo    = feedbackRepo;
        _mapper          = mapper;
        _logger          = logger;
        _submitValidator = submitValidator;
    }

    // ── UC-09: Get Feedback List ─────────────────────────────────────────────
    public async Task<ServiceResult<FeedbackListDto>> GetFeedbackListAsync(
        int sessionId, int teacherId, CancellationToken ct = default)
    {
        var session = await _sessionRepo.GetByIdWithClassStudentsAsync(sessionId, ct);
        if (session is null)
            return ServiceResult<FeedbackListDto>.Fail($"Buổi học #{sessionId} không tồn tại.", 404);

        if (session.TeacherId != teacherId)
            return ServiceResult<FeedbackListDto>.Fail("Bạn không có quyền xem buổi học này.", 403);

        if (session.Status != AppConstants.SessionStatus.Completed)
            return ServiceResult<FeedbackListDto>.Fail(
                "Chỉ có thể đánh giá sau khi buổi học hoàn thành (đã điểm danh).", 400);

        // Load existing feedbacks
        var existingFeedbacks = await _feedbackRepo.GetBySessionIdAsync(sessionId, ct);
        var feedbackMap = existingFeedbacks.ToDictionary(f => f.StudentId);

        // Build student list with feedback status
        var students = session.Class.ClassStudents
            .Select(cs =>
            {
                feedbackMap.TryGetValue(cs.StudentId, out var fb);
                return new StudentFeedbackSummary
                {
                    StudentId   = cs.StudentId,
                    StudentName = cs.Student.FullName,
                    HasFeedback = fb is not null,
                    Score       = fb?.Score
                };
            })
            .OrderBy(s => s.StudentName)
            .ToList();

        var slaDeadline = session.EndTime.Add(SlaWindow);
        var dto = new FeedbackListDto
        {
            SessionId         = session.Id,
            ClassName         = session.Class.Name,
            TeacherName       = session.Teacher.FullName,
            StartTime         = session.StartTime,
            EndTime           = session.EndTime,
            SlaDeadline       = slaDeadline,
            IsSlaExpired      = DateTime.UtcNow > slaDeadline,
            Students          = students,
            ExistingFeedbacks = existingFeedbacks.Count > 0
                ? _mapper.Map<List<FeedbackDto>>(existingFeedbacks)
                : null
        };

        return ServiceResult<FeedbackListDto>.Success(dto);
    }

    // ── UC-09: Get Student Feedback Detail ───────────────────────────────────
    public async Task<ServiceResult<StudentFeedbackDto>> GetStudentFeedbackAsync(
        int sessionId, int studentId, int teacherId, CancellationToken ct = default)
    {
        var session = await _sessionRepo.GetByIdWithClassStudentsAsync(sessionId, ct);
        if (session is null)
            return ServiceResult<StudentFeedbackDto>.Fail($"Buổi học #{sessionId} không tồn tại.", 404);

        if (session.TeacherId != teacherId)
            return ServiceResult<StudentFeedbackDto>.Fail("Bạn không có quyền xem buổi học này.", 403);

        // Verify student belongs to this class
        var classStudent = session.Class.ClassStudents
            .FirstOrDefault(cs => cs.StudentId == studentId);
        if (classStudent is null)
            return ServiceResult<StudentFeedbackDto>.Fail(
                $"Học sinh #{studentId} không thuộc lớp này.", 404);

        var existingFeedback = await _feedbackRepo.GetBySessionAndStudentAsync(sessionId, studentId, ct);
        var slaDeadline = session.EndTime.Add(SlaWindow);

        var dto = new StudentFeedbackDto
        {
            SessionId        = session.Id,
            StudentId        = studentId,
            StudentName      = classStudent.Student.FullName,
            ClassName        = session.Class.Name,
            StartTime        = session.StartTime,
            EndTime          = session.EndTime,
            SlaDeadline      = slaDeadline,
            IsSlaExpired     = DateTime.UtcNow > slaDeadline,
            ExistingFeedback = existingFeedback is not null
                ? _mapper.Map<FeedbackDto>(existingFeedback)
                : null
        };

        return ServiceResult<StudentFeedbackDto>.Success(dto);
    }

    // ── UC-09: Submit Feedback ───────────────────────────────────────────────
    public async Task<ServiceResult<object?>> SubmitFeedbackAsync(
        int sessionId, int teacherId, SubmitFeedbackRequest request, CancellationToken ct = default)
    {
        // Validate request
        var validation = await _submitValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return ServiceResult<object?>.Validation(validation.Errors);

        // Load session
        var session = await _sessionRepo.GetByIdWithDetailsAsync(sessionId, ct);
        if (session is null)
            return ServiceResult<object?>.Fail($"Buổi học #{sessionId} không tồn tại.", 404);

        if (session.TeacherId != teacherId)
            return ServiceResult<object?>.Fail("Bạn không có quyền đánh giá buổi học này.", 403);

        if (session.Status != AppConstants.SessionStatus.Completed)
            return ServiceResult<object?>.Fail(
                "Chỉ có thể đánh giá sau khi buổi học hoàn thành (đã điểm danh).", 400);

        // Check SLA
        var slaDeadline = session.EndTime.Add(SlaWindow);
        var isSlaOverdue = DateTime.UtcNow > slaDeadline;

        if (isSlaOverdue)
            _logger.LogWarning(
                "SLA breached for Session {SessionId}, Student {StudentId} by Teacher {TeacherId}. " +
                "Deadline was {SlaDeadline}, submitted at {Now}",
                sessionId, request.StudentId, teacherId, slaDeadline, DateTime.UtcNow);

        // Check if already exists → same-day edit
        var existing = await _feedbackRepo.GetBySessionAndStudentAsync(sessionId, request.StudentId, ct);
        if (existing is not null)
        {
            // TODO: Extract to DateTimeHelper if multi-tenant — Ngày theo giờ Việt Nam (UTC+7)
            var vnOffset = TimeSpan.FromHours(7);
            var localNow = DateTime.UtcNow.Add(vnOffset).Date;
            var localSubmitDate = existing.SubmittedAt.Add(vnOffset).Date;

            if (localSubmitDate != localNow)
                return ServiceResult<object?>.Fail(
                    "Chỉ có thể chỉnh sửa đánh giá trong ngày.", 409);

            // Delete old → insert new
            _feedbackRepo.Delete(existing);

            _logger.LogInformation(
                "Feedback re-submitted (edit) for Session {SessionId}, Student {StudentId} by Teacher {TeacherId}",
                sessionId, request.StudentId, teacherId);
        }

        // Create new feedback record
        var feedback = new Feedback
        {
            SessionId    = sessionId,
            StudentId    = request.StudentId,
            TeacherId    = teacherId,
            Score        = request.Score,
            Content      = request.Content,
            SubmittedAt  = DateTime.UtcNow,
            IsSlaOverdue = isSlaOverdue
        };

        await _feedbackRepo.AddAsync(feedback, ct);
        await _feedbackRepo.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Feedback submitted for Session {SessionId}, Student {StudentId} by Teacher {TeacherId}: Score={Score}, SlaOverdue={SlaOverdue}",
            sessionId, request.StudentId, teacherId, request.Score, isSlaOverdue);

        return ServiceResult<object?>.Success(null);
    }
}
