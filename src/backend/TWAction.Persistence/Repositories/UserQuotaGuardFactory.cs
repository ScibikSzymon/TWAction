using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TWAction.Application.Users.Interfaces;

namespace TWAction.Persistence.Repositories;

public sealed class UserQuotaGuardFactory(TWActionDbContext db) : IUserQuotaGuardFactory
{
    public async Task<IUserQuotaGuard> AcquireAsync(Guid userId, CancellationToken ct = default)
    {
        var transaction = await db.Database.BeginTransactionAsync(ct);
        try
        {
            await using var command = db.Database.GetDbConnection().CreateCommand();
            command.Transaction = transaction.GetDbTransaction();
            command.CommandText = "SELECT \"Id\" FROM \"Users\" WHERE \"Id\" = @userId FOR UPDATE";
            var parameter = command.CreateParameter();
            parameter.ParameterName = "userId";
            parameter.Value = userId;
            command.Parameters.Add(parameter);
            await command.ExecuteScalarAsync(ct);
            return new Guard(transaction);
        }
        catch
        {
            await transaction.DisposeAsync();
            throw;
        }
    }

    private sealed class Guard(IDbContextTransaction transaction) : IUserQuotaGuard
    {
        public Task CommitAsync(CancellationToken ct = default) => transaction.CommitAsync(ct);
        public ValueTask DisposeAsync() => transaction.DisposeAsync();
    }
}
