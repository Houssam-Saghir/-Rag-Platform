using Microsoft.EntityFrameworkCore;
using Rag.Domain.Common;
using Rag.Domain.Entities;
using Rag.Infrastructure.Interfaces;
using Rag.Infrastructure.Persistence;

namespace Rag.Infrastructure.Repositories;

public class Repository<T>(ApplicationDbContext dbContext) : IRepository<T> where T : BaseEntity
{
    private readonly DbSet<T> _set = dbContext.Set<T>();

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default) => await _set.FindAsync([id], ct);

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default) => await _set.ToListAsync(ct);

    public async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        await _set.AddAsync(entity, ct);
        return entity;
    }

    public Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        _set.Update(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await GetByIdAsync(id, ct);
        if (entity is null) return;

        if (entity is ISoftDelete softDelete)
        {
            softDelete.IsDeleted = true;
            softDelete.DeletedAt = DateTime.UtcNow;
            _set.Update(entity);
            return;
        }

        _set.Remove(entity);
    }

    public IQueryable<T> Query() => _set.AsQueryable();
}
