using NSubstitute;
using TWAction.Application.Users.Commands;
using TWAction.Application.Users.DTOs;
using TWAction.Application.Users.Interfaces;
using TWAction.Application.Users.Queries;
using TWAction.Domain.Users;

namespace TWAction.UnitTests.Handlers;

public sealed class UsersManagementHandlerTests
{
    [Fact]
    public async Task UpdateUser_UpdatesProfileAndRole()
    {
        var userId = Guid.NewGuid();
        var user = new UserEntity
        {
            Id = userId,
            Email = "old@example.com",
            Provider = "google",
            Role = UserRole.User
        };
        var repository = Substitute.For<IUserRepository>();
        repository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);
        repository.FindByEmailAsync("new@example.com", "google", Arg.Any<CancellationToken>()).Returns((UserEntity?)null);
        repository.UpdateAsync(user, Arg.Any<CancellationToken>()).Returns(user);

        var result = await new UpdateUserHandler(repository).Handle(new UpdateUserCommand(
            userId,
            new UpdateUserRequest
            {
                Email = " new@example.com ",
                DisplayName = " New Name ",
                Role = UserRole.Admin
            }));

        Assert.True(result.IsSuccess);
        Assert.Equal("new@example.com", user.Email);
        Assert.Equal("New Name", user.DisplayName);
        Assert.Equal(UserRole.Admin, user.Role);
        await repository.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateUser_RejectsDuplicateEmail()
    {
        var userId = Guid.NewGuid();
        var user = new UserEntity { Id = userId, Email = "old@example.com", Provider = "google" };
        var otherUser = new UserEntity { Id = Guid.NewGuid(), Email = "taken@example.com", Provider = "google" };
        var repository = Substitute.For<IUserRepository>();
        repository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);
        repository.FindByEmailAsync("taken@example.com", "google", Arg.Any<CancellationToken>()).Returns(otherUser);

        var result = await new UpdateUserHandler(repository).Handle(new UpdateUserCommand(
            userId,
            new UpdateUserRequest { Email = "taken@example.com", Role = UserRole.User }));

        Assert.True(result.IsFailure);
        await repository.DidNotReceive().UpdateAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetUserSessions_ReturnsOnlySafeSessionFieldsAndStatus()
    {
        var userId = Guid.NewGuid();
        var userRepository = Substitute.For<IUserRepository>();
        var sessionRepository = Substitute.For<IUserSessionRepository>();
        userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(new UserEntity { Id = userId });
        sessionRepository.ListByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns([
            new UserSessionEntity { Id = Guid.NewGuid(), UserId = userId, ExpiresAt = DateTimeOffset.UtcNow.AddHours(1), Data = "secret" },
            new UserSessionEntity { Id = Guid.NewGuid(), UserId = userId, ExpiresAt = DateTimeOffset.UtcNow.AddHours(-1) }
        ]);

        var result = await new GetUserSessionsHandler(userRepository, sessionRepository)
            .Handle(new GetUserSessionsQuery(userId));

        Assert.True(result.IsSuccess);
        var sessions = result.Value.ToList();
        Assert.Equal(2, sessions.Count);
        Assert.True(sessions[0].IsActive);
        Assert.False(sessions[1].IsActive);
        Assert.DoesNotContain("secret", sessions[0].ToString());
    }

    [Fact]
    public async Task DeleteUserSession_RejectsSessionBelongingToAnotherUser()
    {
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var repository = Substitute.For<IUserSessionRepository>();
        repository.GetByIdAsync(sessionId, Arg.Any<CancellationToken>()).Returns(new UserSessionEntity
        {
            Id = sessionId,
            UserId = Guid.NewGuid()
        });

        var result = await new DeleteUserSessionHandler(repository)
            .Handle(new DeleteUserSessionCommand(userId, sessionId));

        Assert.True(result.IsFailure);
        await repository.DidNotReceive().DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
