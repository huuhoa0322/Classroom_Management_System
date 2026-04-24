using CLS.DAL.Data;
using CLS.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace CLS.DAL.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _ctx;

    public UserRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task<User?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _ctx.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken ct = default)
        => await _ctx.Users.AsNoTracking().ToListAsync(ct);

    public async Task<List<User>> GetTeachersAsync(CancellationToken ct = default)
        => await _ctx.Users
            .AsNoTracking()
            .Where(u => u.Role == "Teacher" && u.Status == "active")
            .OrderBy(u => u.FullName)
            .ToListAsync(ct);

    public async Task AddAsync(User entity, CancellationToken ct = default)
        => await _ctx.Users.AddAsync(entity, ct);

    public void Update(User entity)
        => _ctx.Users.Update(entity);

    public void Delete(User entity)
    {
        entity.IsDeleted = true;
        _ctx.Users.Update(entity);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _ctx.SaveChangesAsync(ct);
}
