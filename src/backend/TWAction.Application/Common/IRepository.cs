using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TWAction.Application.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task AddAsync(T entity, CancellationToken ct = default);
        Task<IEnumerable<T>> ListAsync(CancellationToken ct = default);
        void Remove(T entity);
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
