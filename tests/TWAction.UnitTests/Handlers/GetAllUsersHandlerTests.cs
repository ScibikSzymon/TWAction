using AwesomeAssertions;
using NSubstitute;
using TWAction.Application.Handlers;
using TWAction.Application.Interfaces;
using TWAction.Domain.Entities;

namespace TWAction.UnitTests.Handlers;

public sealed class GetAllUsersHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly GetAllUsersHandler _handler;

    public GetAllUsersHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _handler = new GetAllUsersHandler(_userRepository);
    }

    [Fact]
    public async Task Handle_WithMultipleUsers_ReturnsAllUsers()
    {
        var users = new List<UserEntity>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Email = "user1@example.com",
                DisplayName = "User One",
                Provider = "google",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-10)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Email = "user2@example.com",
                DisplayName = "User Two",
                Provider = "google",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-5)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Email = "user3@example.com",
                DisplayName = null,
                Provider = "google",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-1)
            }
        };

        _userRepository.ListAllAsync(Arg.Any<CancellationToken>())
            .Returns(users);

        var query = new GetAllUsersQuery();
        var result = await _handler.Handle(query);

        result.Should().HaveCount(3);
        result[0].Email.Should().Be("user1@example.com");
        result[0].DisplayName.Should().Be("User One");
        result[1].Email.Should().Be("user2@example.com");
        result[1].DisplayName.Should().Be("User Two");
        result[2].Email.Should().Be("user3@example.com");
        result[2].DisplayName.Should().BeNull();

        await _userRepository.Received(1).ListAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNoUsers_ReturnsEmptyList()
    {
        var users = new List<UserEntity>();

        _userRepository.ListAllAsync(Arg.Any<CancellationToken>())
            .Returns(users);

        var query = new GetAllUsersQuery();
        var result = await _handler.Handle(query);

        result.Should().BeEmpty();
        await _userRepository.Received(1).ListAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithSingleUser_ReturnsSingleUserDto()
    {
        var userId = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow.AddDays(-30);
        var users = new List<UserEntity>
        {
            new()
            {
                Id = userId,
                Email = "single@example.com",
                DisplayName = "Single User",
                Provider = "google",
                CreatedAt = createdAt
            }
        };

        _userRepository.ListAllAsync(Arg.Any<CancellationToken>())
            .Returns(users);

        var query = new GetAllUsersQuery();
        var result = await _handler.Handle(query);

        result.Should().HaveCount(1);
        result[0].Id.Should().Be(userId);
        result[0].Email.Should().Be("single@example.com");
        result[0].DisplayName.Should().Be("Single User");
        result[0].Provider.Should().Be("google");
        result[0].CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_PropagatesCancellationToken()
    {
        var users = new List<UserEntity>();
        var cancellationToken = new CancellationToken();

        _userRepository.ListAllAsync(cancellationToken)
            .Returns(users);

        var query = new GetAllUsersQuery();
        await _handler.Handle(query, cancellationToken);

        await _userRepository.Received(1).ListAllAsync(cancellationToken);
    }

    [Fact]
    public async Task Handle_MapsEntityPropertiesToDtoProperties()
    {
        var userId = Guid.NewGuid();
        var email = "mapper@example.com";
        var displayName = "Mapper Test";
        var provider = "google";
        var createdAt = DateTimeOffset.UtcNow.AddDays(-15);

        var users = new List<UserEntity>
        {
            new()
            {
                Id = userId,
                Email = email,
                DisplayName = displayName,
                Provider = provider,
                CreatedAt = createdAt
            }
        };

        _userRepository.ListAllAsync(Arg.Any<CancellationToken>())
            .Returns(users);

        var query = new GetAllUsersQuery();
        var result = await _handler.Handle(query);

        result.Should().HaveCount(1);
        var userDto = result[0];
        userDto.Id.Should().Be(userId);
        userDto.Email.Should().Be(email);
        userDto.DisplayName.Should().Be(displayName);
        userDto.Provider.Should().Be(provider);
        userDto.CreatedAt.Should().Be(createdAt);
    }
}
