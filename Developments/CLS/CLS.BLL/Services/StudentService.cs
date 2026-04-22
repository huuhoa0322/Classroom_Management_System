using AutoMapper;
using CLS.BLL.Common;
using CLS.BLL.Common.Exceptions;
using CLS.BLL.DTOs.Students;
using CLS.BLL.Interfaces;
using CLS.DAL.Data;
using CLS.DAL.Entities;
using CLS.DAL.Repositories;
using Microsoft.Extensions.Logging;

namespace CLS.BLL.Services;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _studentRepo;
    private readonly IParentRepository _parentRepo;
    private readonly AppDbContext _ctx;
    private readonly IMapper _mapper;
    private readonly ILogger<StudentService> _logger;

    public StudentService(
        IStudentRepository studentRepo,
        IParentRepository parentRepo,
        AppDbContext ctx,
        IMapper mapper,
        ILogger<StudentService> logger)
    {
        _studentRepo = studentRepo;
        _parentRepo  = parentRepo;
        _ctx         = ctx;
        _mapper      = mapper;
        _logger      = logger;
    }

    // ── GET ALL (Phân trang) ──────────────────────────────────────────────────
    public async Task<PagedResult<StudentResponse>> GetAllAsync(
        int page, int pageSize, string? status, CancellationToken ct = default)
    {
        var (students, total) = await _studentRepo.GetPagedAsync(page, pageSize, status, ct);
        var items = _mapper.Map<List<StudentResponse>>(students);
        return PagedResult<StudentResponse>.Create(items, total, page, pageSize);
    }

    // ── GET BY ID ─────────────────────────────────────────────────────────────
    public async Task<StudentResponse> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var student = await _studentRepo.GetWithParentAsync(id, ct)
            ?? throw new NotFoundException($"Học sinh #{id} không tồn tại.");

        return _mapper.Map<StudentResponse>(student);
    }

    // ── CREATE (CLS-001: Onboard New Student Profiles) ────────────────────────
    public async Task<StudentResponse> CreateAsync(CreateStudentRequest request, CancellationToken ct = default)
    {
        // 1. Upsert Parent: tìm theo email → tạo mới nếu chưa tồn tại
        var parent = await _parentRepo.GetByEmailAsync(request.ParentEmail, ct);
        if (parent is null)
        {
            parent = _mapper.Map<Parent>(request);
            await _parentRepo.AddAsync(parent, ct);
            _logger.LogInformation("Created new Parent with Email {Email}", request.ParentEmail);
        }
        else
        {
            _logger.LogInformation("Linked existing Parent {ParentId} with Email {Email}", parent.Id, request.ParentEmail);
        }

        // 2. Tạo Student và liên kết với Parent
        var student = _mapper.Map<Student>(request);
        student.EnrolledAt = DateTime.UtcNow;

        // Nếu Parent chưa được lưu (Id = 0), phải save trước để lấy Id
        if (parent.Id == 0)
        {
            await _ctx.SaveChangesAsync(ct);
        }

        student.ParentId = parent.Id;
        await _studentRepo.AddAsync(student, ct);
        await _ctx.SaveChangesAsync(ct);

        _logger.LogInformation("Created Student {StudentId} ({Name}) linked to Parent {ParentId}",
            student.Id, student.FullName, parent.Id);

        // 3. Reload với Parent để map response
        var created = await _studentRepo.GetWithParentAsync(student.Id, ct)!;
        return _mapper.Map<StudentResponse>(created!);
    }

    // ── UPDATE (CLS-002: Sửa thông tin) ──────────────────────────────────────
    public async Task<StudentResponse> UpdateAsync(int id, UpdateStudentRequest request, CancellationToken ct = default)
    {
        var student = await _studentRepo.GetWithParentAsync(id, ct)
            ?? throw new NotFoundException($"Học sinh #{id} không tồn tại.");

        _mapper.Map(request, student);
        _studentRepo.Update(student);
        await _ctx.SaveChangesAsync(ct);

        _logger.LogInformation("Updated Student {StudentId}", id);
        return _mapper.Map<StudentResponse>(student);
    }

    // ── UPDATE STATUS (CLS-002 AC1: Lifecycle change) ─────────────────────────
    public async Task<StudentResponse> UpdateStatusAsync(
        int id, UpdateStudentStatusRequest request, CancellationToken ct = default)
    {
        var validStatuses = new[] { AppConstants.StudentStatus.Active, AppConstants.StudentStatus.Inactive };
        if (!validStatuses.Contains(request.Status))
            throw new ValidationException($"Trạng thái không hợp lệ: '{request.Status}'. Chỉ chấp nhận: active, inactive.");

        var student = await _studentRepo.GetWithParentAsync(id, ct)
            ?? throw new NotFoundException($"Học sinh #{id} không tồn tại.");

        var oldStatus = student.Status;
        student.Status = request.Status;

        if (request.Status == AppConstants.StudentStatus.Inactive)
        {
            // CLS-002 AC1: Archive gói học sẽ được thực hiện khi StudentPackage slice được implement
            _logger.LogWarning(
                "Student {StudentId} ({Name}) deactivated (Dropped Out). Archive packages: pending StudentPackage slice.",
                id, student.FullName);
        }

        _studentRepo.Update(student);
        await _ctx.SaveChangesAsync(ct);

        _logger.LogInformation("Student {StudentId} status changed: {Old} → {New}", id, oldStatus, request.Status);
        return _mapper.Map<StudentResponse>(student);
    }
}
