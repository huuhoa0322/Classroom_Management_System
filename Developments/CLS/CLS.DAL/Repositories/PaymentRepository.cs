using CLS.DAL.Data;
using CLS.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace CLS.DAL.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _ctx;

    public PaymentRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task<Payment?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _ctx.Payments.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IEnumerable<Payment>> GetAllAsync(CancellationToken ct = default)
        => await _ctx.Payments.AsNoTracking().ToListAsync(ct);

    public async Task<(List<Payment> Items, int TotalCount)> GetPagedByStudentIdAsync(
        int studentId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _ctx.Payments
            .AsNoTracking()
            .Include(p => p.StudentPackage)
                .ThenInclude(sp => sp.Package)
            .Include(p => p.StudentPackage)
                .ThenInclude(sp => sp.Student)
            .Include(p => p.RecordedBy)
            .Where(p => p.StudentPackage.StudentId == studentId);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(p => p.PaymentDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<Payment?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default)
        => await _ctx.Payments
            .Include(p => p.StudentPackage)
                .ThenInclude(sp => sp.Package)
            .Include(p => p.StudentPackage)
                .ThenInclude(sp => sp.Student)
            .Include(p => p.RecordedBy)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<(List<Payment> Items, int TotalCount)> GetPagedAllAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = _ctx.Payments
            .AsNoTracking()
            .Include(p => p.StudentPackage)
                .ThenInclude(sp => sp.Package)
            .Include(p => p.StudentPackage)
                .ThenInclude(sp => sp.Student)
            .Include(p => p.RecordedBy);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(p => p.PaymentDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task AddAsync(Payment entity, CancellationToken ct = default)
        => await _ctx.Payments.AddAsync(entity, ct);

    public void Update(Payment entity)
        => _ctx.Payments.Update(entity);

    public void Delete(Payment entity)
    {
        entity.IsDeleted = true;
        _ctx.Payments.Update(entity);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _ctx.SaveChangesAsync(ct);
}
