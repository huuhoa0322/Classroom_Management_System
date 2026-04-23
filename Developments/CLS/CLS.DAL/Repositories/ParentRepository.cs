using CLS.DAL.Data;
using CLS.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace CLS.DAL.Repositories;

public class ParentRepository : IParentRepository
{
    private readonly AppDbContext _ctx;

    public ParentRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task<Parent?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _ctx.Parents.FindAsync([id], ct);

    public async Task<IEnumerable<Parent>> GetAllAsync(CancellationToken ct = default)
        => await _ctx.Parents.AsNoTracking().ToListAsync(ct);

    public async Task<Parent?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await _ctx.Parents
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Email.ToLower() == email.ToLower(), ct);

    public async Task AddAsync(Parent entity, CancellationToken ct = default)
        => await _ctx.Parents.AddAsync(entity, ct);

    public void Update(Parent entity)
        => _ctx.Parents.Update(entity);

    public void Delete(Parent entity)
    {
        entity.IsDeleted = true;
        _ctx.Parents.Update(entity);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _ctx.SaveChangesAsync(ct);
}
