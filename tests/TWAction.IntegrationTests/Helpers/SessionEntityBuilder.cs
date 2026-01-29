using TWAction.Domain.Users;

namespace TWAction.IntegrationTests.Helpers;

public sealed class SessionEntityBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _userId = Guid.NewGuid();
    private DateTimeOffset _expiresAt = DateTimeOffset.UtcNow.AddHours(8);
    private string? _data;

    public SessionEntityBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public SessionEntityBuilder WithUserId(Guid userId)
    {
        _userId = userId;
        return this;
    }

    public SessionEntityBuilder WithExpiresAt(DateTimeOffset expiresAt)
    {
        _expiresAt = expiresAt;
        return this;
    }

    public SessionEntityBuilder WithData(string? data)
    {
        _data = data;
        return this;
    }

    public SessionEntityBuilder AsExpired()
    {
        _expiresAt = DateTimeOffset.UtcNow.AddHours(-1);
        return this;
    }

    public UserSessionEntity Build()
    {
        return new UserSessionEntity
        {
            Id = _id,
            UserId = _userId,
            ExpiresAt = _expiresAt,
            Data = _data
        };
    }
}
