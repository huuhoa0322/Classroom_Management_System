using CLS.DAL.Data;
using CLS.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace CLS.DAL.Repositories;

public class FeedbackRepository : IFeedbackRepository
{
    private readonly AppDbContext _ctx;

    public FeedbackRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task<List<Feedback>> GetBySessionIdAsync(int sessionId, CancellationToken ct = default)
        => await _ctx.Feedbacks
            .AsNoTracking()
            .Include(f => f.Student)
            .Where(f => f.SessionId == sessionId)
            .OrderBy(f => f.Student.FullName)
            .ToListAsync(ct);

    public async Task<Feedback?> GetBySessionAndStudentAsync(int sessionId, int studentId, CancellationToken ct = default)
        => await _ctx.Feedbacks
            .Include(f => f.Student)
            .FirstOrDefaultAsync(f => f.SessionId == sessionId && f.StudentId == studentId, ct);

    public async Task<bool> HasFeedbackAsync(int sessionId, int studentId, CancellationToken ct = default)
        => await _ctx.Feedbacks.AnyAsync(f => f.SessionId == sessionId && f.StudentId == studentId, ct);

    public async Task AddAsync(Feedback entity, CancellationToken ct = default)
        => await _ctx.Feedbacks.AddAsync(entity, ct);

    public void Delete(Feedback entity)
        => _ctx.Feedbacks.Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _ctx.SaveChangesAsync(ct);
}
