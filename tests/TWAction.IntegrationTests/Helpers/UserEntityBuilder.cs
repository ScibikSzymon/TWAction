using TWAction.Domain.Entities;

namespace TWAction.IntegrationTests.Helpers;

public sealed class UserEntityBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _email = "test@example.com";
    private string? _displayName = "Test User";
    private string _provider = "google";
    private DateTimeOffset _createdAt = DateTimeOffset.UtcNow;

    public UserEntityBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public UserEntityBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public UserEntityBuilder WithDisplayName(string? displayName)
    {
        _displayName = displayName;
        return this;
    }

    public UserEntityBuilder WithProvider(string provider)
    {
        _provider = provider;
        return this;
    }

    public UserEntityBuilder WithCreatedAt(DateTimeOffset createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public UserEntity Build()
    {
        return new UserEntity
        {
            Id = _id,
            Email = _email,
            DisplayName = _displayName,
            Provider = _provider,
            CreatedAt = _createdAt
        };
    }
}
