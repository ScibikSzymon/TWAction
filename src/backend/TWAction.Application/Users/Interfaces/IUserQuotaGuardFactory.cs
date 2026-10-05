namespace TWAction.Application.Users.Interfaces;

public interface IUserQuotaGuard : IAsyncDisposable
{
    Task CommitAsync(CancellationToken ct = default);
}

public interface IUserQuotaGuardFactory
{
    Task<IUserQuotaGuard> AcquireAsync(Guid userId, CancellationToken ct = default);
}
