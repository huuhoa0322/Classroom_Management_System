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
            .Where(u => u.Role == Common.DalConstants.Roles.Teacher && u.Status == Common.DalConstants.Status.Active)
            .OrderBy(u => u.FullName)
            .ToListAsync(ct);

    public async Task<(List<User> Items, int Total)> GetPagedAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = _ctx.Users.AsNoTracking().OrderBy(u => u.FullName);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<bool> ExistsByEmailAsync(string email, int? excludeId = null, CancellationToken ct = default)
    {
        var query = _ctx.Users.Where(u => u.Email == email);
        if (excludeId.HasValue) query = query.Where(u => u.Id != excludeId.Value);
        return await query.AnyAsync(ct);
    }

    public async Task AddAsync(User entity, CancellationToken ct = default)
        => await _ctx.Users.AddAsync(entity, ct);

    public void Update(User entity) => _ctx.Users.Update(entity);

    public void Delete(User entity)
    {
        entity.IsDeleted = true;
        _ctx.Users.Update(entity);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _ctx.SaveChangesAsync(ct);
}
