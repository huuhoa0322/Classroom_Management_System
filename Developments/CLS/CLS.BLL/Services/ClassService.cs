using AutoMapper;
using CLS.BLL.Common;
using CLS.BLL.DTOs.Classes;
using CLS.BLL.Interfaces;
using CLS.DAL.Entities;
using CLS.DAL.Repositories;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CLS.BLL.Services;

/// <summary>
/// Class Management Service — CRUD lớp học + đăng ký học sinh hàng loạt.
/// </summary>
public class ClassService : IClassService
{
    private readonly IClassRepository                   _classRepo;
    private readonly IStudentService                    _studentService;
    private readonly IMapper                            _mapper;
    private readonly ILogger<ClassService>              _logger;
    private readonly IValidator<CreateClassRequest>     _createValidator;
    private readonly IValidator<UpdateClassRequest>     _updateValidator;

    public ClassService(
        IClassRepository classRepo,
        IStudentService studentService,
        IMapper mapper,
        ILogger<ClassService> logger,
        IValidator<CreateClassRequest> createValidator,
        IValidator<UpdateClassRequest> updateValidator)
    {
        _classRepo       = classRepo;
        _studentService  = studentService;
        _mapper          = mapper;
        _logger          = logger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    // ── GetAll (paged) ────────────────────────────────────────────────────────
    public async Task<PagedResult<ClassResponse>> GetAllAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        (page, pageSize) = AppConstants.Pagination.Clamp(page, pageSize);
        var (classes, total) = await _classRepo.GetPagedAsync(page, pageSize, ct);
        var items = _mapper.Map<List<ClassResponse>>(classes);
        return PagedResult<ClassResponse>.Create(items, total, page, pageSize);
    }

    // ── GetById ───────────────────────────────────────────────────────────────
    public async Task<ServiceResult<ClassResponse>> GetByIdAsync(
        int id, CancellationToken ct = default)
    {
        var classEntity = await _classRepo.GetByIdWithDetailsReadOnlyAsync(id, ct);
        if (classEntity is null)
            return ServiceResult<ClassResponse>.Fail($"Lớp học #{id} không tồn tại.", 404);

        return ServiceResult<ClassResponse>.Success(_mapper.Map<ClassResponse>(classEntity));
    }

    // ── Create ────────────────────────────────────────────────────────────────
    public async Task<ServiceResult<ClassResponse>> CreateAsync(
        CreateClassRequest request, int createdBy, CancellationToken ct = default)
    {
        var validation = await _createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return ServiceResult<ClassResponse>.Validation(validation.Errors);

        if (await _classRepo.ExistsByNameAsync(request.Name, null, ct))
            return ServiceResult<ClassResponse>.Fail(
                $"Lớp học '{request.Name}' đã tồn tại.", 409);

        var entity = new Class
        {
            Name      = request.Name.Trim(),
            Status    = AppConstants.ClassStatus.Active,
            CreatedBy = createdBy,
        };

        await _classRepo.AddAsync(entity, ct);
        await _classRepo.SaveChangesAsync(ct);

        _logger.LogInformation("Created Class {ClassId}: Name={Name}, CreatedBy={UserId}",
            entity.Id, entity.Name, createdBy);

        var created = await _classRepo.GetByIdWithDetailsReadOnlyAsync(entity.Id, ct);
        return ServiceResult<ClassResponse>.Success(_mapper.Map<ClassResponse>(created!));
    }

    // ── Update ────────────────────────────────────────────────────────────────
    public async Task<ServiceResult<ClassResponse>> UpdateAsync(
        int id, UpdateClassRequest request, CancellationToken ct = default)
    {
        var validation = await _updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return ServiceResult<ClassResponse>.Validation(validation.Errors);

        var entity = await _classRepo.GetByIdWithDetailsAsync(id, ct);
        if (entity is null)
            return ServiceResult<ClassResponse>.Fail($"Lớp học #{id} không tồn tại.", 404);

        if (await _classRepo.ExistsByNameAsync(request.Name, id, ct))
            return ServiceResult<ClassResponse>.Fail(
                $"Lớp học '{request.Name}' đã tồn tại.", 409);

        entity.Name = request.Name.Trim();
        _classRepo.Update(entity);
        await _classRepo.SaveChangesAsync(ct);

        _logger.LogInformation("Updated Class {ClassId}: Name={Name}", id, entity.Name);

        var updated = await _classRepo.GetByIdWithDetailsReadOnlyAsync(id, ct);
        return ServiceResult<ClassResponse>.Success(_mapper.Map<ClassResponse>(updated!));
    }

    // ── UpdateStatus ──────────────────────────────────────────────────────────
    public async Task<ServiceResult<ClassResponse>> UpdateStatusAsync(
        int id, UpdateClassStatusRequest request, CancellationToken ct = default)
    {
        var validStatuses = new[]
        {
            AppConstants.ClassStatus.Active,
            AppConstants.ClassStatus.Inactive,
        };

        if (!validStatuses.Contains(request.Status))
            return ServiceResult<ClassResponse>.Fail(
                $"Trạng thái '{request.Status}' không hợp lệ. Chỉ chấp nhận: active, inactive.", 400);

        var entity = await _classRepo.GetByIdWithDetailsAsync(id, ct);
        if (entity is null)
            return ServiceResult<ClassResponse>.Fail($"Lớp học #{id} không tồn tại.", 404);

        entity.Status = request.Status;
        _classRepo.Update(entity);
        await _classRepo.SaveChangesAsync(ct);

        _logger.LogInformation("Updated Class {ClassId} status to {Status}", id, request.Status);

        var updated = await _classRepo.GetByIdWithDetailsReadOnlyAsync(id, ct);
        return ServiceResult<ClassResponse>.Success(_mapper.Map<ClassResponse>(updated!));
    }

    // ── GetStudents ───────────────────────────────────────────────────────────
    public async Task<IEnumerable<ClassStudentResponse>> GetStudentsAsync(
        int classId, CancellationToken ct = default)
    {
        var entity = await _classRepo.GetByIdWithDetailsReadOnlyAsync(classId, ct);
        if (entity is null) return [];

        var activeStudents = entity.ClassStudents
            .Where(cs => cs.Status == AppConstants.ClassStudentStatus.Active)
            .OrderBy(cs => cs.Student.FullName);

        return _mapper.Map<IEnumerable<ClassStudentResponse>>(activeStudents);
    }

    // ── EnrollStudents (batch) ─────────────────────────────────────────────────
    public async Task<ServiceResult<IEnumerable<ClassStudentResponse>>> EnrollStudentsAsync(
        int classId, EnrollStudentsRequest request, CancellationToken ct = default)
    {
        if (request.StudentIds is null || request.StudentIds.Count == 0)
            return ServiceResult<IEnumerable<ClassStudentResponse>>.Fail(
                "Danh sách học sinh không được trống.", 400);

        var classEntity = await _classRepo.GetByIdWithDetailsAsync(classId, ct);
        if (classEntity is null)
            return ServiceResult<IEnumerable<ClassStudentResponse>>.Fail(
                $"Lớp học #{classId} không tồn tại.", 404);

        if (classEntity.Status != AppConstants.ClassStatus.Active)
            return ServiceResult<IEnumerable<ClassStudentResponse>>.Fail(
                "Không thể đăng ký học sinh vào lớp đã ngừng hoạt động.", 409);

        var distinctIds = request.StudentIds.Distinct().ToList();
        var (enrolled, errors) = await ValidateAndCreateEnrollmentsAsync(
            classEntity, distinctIds, ct);

        if (errors.Count > 0 && enrolled.Count == 0)
            return ServiceResult<IEnumerable<ClassStudentResponse>>.Fail(
                string.Join(" ", errors), 400);

        if (enrolled.Count > 0)
        {
            await _classRepo.SaveChangesAsync(ct);
            _logger.LogInformation(
                "Enrolled {Count} students into Class {ClassId}: [{StudentIds}]",
                enrolled.Count, classId,
                string.Join(", ", enrolled.Select(e => e.StudentId)));
        }

        // Reload to get Student navigation
        var reloaded = await _classRepo.GetByIdWithDetailsReadOnlyAsync(classId, ct);
        var enrolledIds = enrolled.Select(e => e.StudentId).ToHashSet();
        var result = reloaded!.ClassStudents
            .Where(cs => enrolledIds.Contains(cs.StudentId))
            .OrderBy(cs => cs.Student.FullName);

        return ServiceResult<IEnumerable<ClassStudentResponse>>.Success(
            _mapper.Map<IEnumerable<ClassStudentResponse>>(result));
    }

    // ── UnenrollStudent ───────────────────────────────────────────────────────
    public async Task<ServiceResult<object?>> UnenrollStudentAsync(
        int classId, int studentId, CancellationToken ct = default)
    {
        var classEntity = await _classRepo.GetByIdWithDetailsAsync(classId, ct);
        if (classEntity is null)
            return ServiceResult<object?>.Fail($"Lớp học #{classId} không tồn tại.", 404);

        var enrollment = classEntity.ClassStudents
            .FirstOrDefault(cs => cs.StudentId == studentId
                                  && cs.Status == AppConstants.ClassStudentStatus.Active);

        if (enrollment is null)
            return ServiceResult<object?>.Fail(
                $"Học sinh #{studentId} không có trong lớp #{classId}.", 404);

        enrollment.Status = AppConstants.ClassStudentStatus.Inactive;
        await _classRepo.SaveChangesAsync(ct);

        _logger.LogInformation("Unenrolled Student {StudentId} from Class {ClassId}",
            studentId, classId);

        return ServiceResult<object?>.Success(null);
    }

    // ── Private Helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Validate từng studentId và tạo ClassStudent entity.
    /// Trả về danh sách enrolled thành công + errors.
    /// </summary>
    private async Task<(List<ClassStudent> Enrolled, List<string> Errors)>
        ValidateAndCreateEnrollmentsAsync(
            Class classEntity, List<int> studentIds, CancellationToken ct)
    {
        var existingStudentIds = classEntity.ClassStudents
            .Where(cs => cs.Status == AppConstants.ClassStudentStatus.Active)
            .Select(cs => cs.StudentId)
            .ToHashSet();

        var enrolled = new List<ClassStudent>();
        var errors = new List<string>();

        foreach (var studentId in studentIds)
        {
            if (existingStudentIds.Contains(studentId))
            {
                errors.Add($"Học sinh #{studentId} đã có trong lớp.");
                continue;
            }

            var studentResult = await _studentService.GetByIdAsync(studentId, ct);
            if (!studentResult.IsSuccess)
            {
                errors.Add($"Học sinh #{studentId} không tồn tại.");
                continue;
            }

            if (studentResult.Value!.Status != AppConstants.StudentStatus.Active)
            {
                errors.Add($"Học sinh '{studentResult.Value!.FullName}' không ở trạng thái hoạt động.");
                continue;
            }

            var cs = new ClassStudent
            {
                ClassId        = classEntity.Id,
                StudentId      = studentId,
                EnrollmentDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Status         = AppConstants.ClassStudentStatus.Active,
            };

            classEntity.ClassStudents.Add(cs);
            enrolled.Add(cs);
        }

        return (enrolled, errors);
    }
}
