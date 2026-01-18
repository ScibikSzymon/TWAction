using Microsoft.EntityFrameworkCore;
using TWAction.Application.Interfaces;

namespace TWAction.Persistence.Repositories;

public class EfRepository<T> : IRepository<T> where T : class
{
    private readonly TWActionDbContext _db;
    private readonly DbSet<T> _set;

    public EfRepository(TWActionDbContext db)
    {
        _db = db;
        _set = db.Set<T>();
    }

    public async Task AddAsync(T entity, CancellationToken ct = default)
    {
        await _set.AddAsync(entity, ct).ConfigureAwait(false);
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _set.FindAsync(new object[] { id }, ct).AsTask().ConfigureAwait(false);
    }

    public async Task<IEnumerable<T>> ListAsync(CancellationToken ct = default)
    {
        return await _set.ToListAsync(ct).ConfigureAwait(false);
    }

    public void Remove(T entity)
    {
        _set.Remove(entity);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return _db.SaveChangesAsync(ct);
    }
}
