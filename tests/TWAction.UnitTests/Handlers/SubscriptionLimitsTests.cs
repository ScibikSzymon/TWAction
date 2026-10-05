using AwesomeAssertions;
using NSubstitute;
using TWAction.Application.Schedules.Commands;
using TWAction.Application.Schedules.Interfaces;
using TWAction.Application.Tribes.Interfaces;
using TWAction.Application.Users.Interfaces;
using TWAction.Application.Templates.Commands;
using TWAction.Application.Templates.DTOs;
using TWAction.Application.Templates.Interfaces;
using TWAction.Application.Interfaces;
using TWAction.Application.MainActions.Services;
using TWAction.Application.Schedules.Services;
using TWAction.Domain.Schedules;
using TWAction.Domain.Users;
using TWAction.Domain.Templates;

namespace TWAction.UnitTests.Handlers;

public sealed class SubscriptionLimitsTests
{
    [Fact]
    public void EffectiveLimits_UsePlanConfigurationOverridesAndUnlimitedValues()
    {
        var user = new UserEntity { Email = "x@example.com" };
        var configuredPlan = CreatePlanLimits(
            scheduleLimit: 7,
            templateLimit: 5,
            troopsUploadLimit: 9,
            uploadWindowHours: 6);
        UserLimits.Schedules(user, configuredPlan).Should().Be(7);
        UserLimits.Templates(user, configuredPlan).Should().Be(5);
        UserLimits.TroopsUploads(user, configuredPlan).Should().Be(9);
        UserLimits.UploadWindow(configuredPlan).Should().Be(TimeSpan.FromHours(6));

        user.ScheduleLimitOverride = 0;
        user.TemplateLimitOverride = 4;
        user.SubscriptionTier = SubscriptionTier.Premium;
        var premiumPlan = CreatePlanLimits(SubscriptionTier.Premium, null, null, null);
        UserLimits.Schedules(user, premiumPlan).Should().Be(0);
        UserLimits.Templates(user, premiumPlan).Should().Be(4);
        UserLimits.TroopsUploads(user, premiumPlan).Should().BeNull();
    }

    [Fact]
    public async Task FreeUserCannotCreateFourthSchedule()
    {
        var userId = Guid.NewGuid();
        var users = Substitute.For<IUserRepository>();
        var schedules = Substitute.For<IScheduleRepository>();
        var tribes = Substitute.For<ITribesService>();
        var quotaGuards = FakeQuotaGuards();
        var planLimits = FakePlanLimits();
        users.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new UserEntity { Id = userId, Email = "x@example.com" });
        schedules.CountByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(3);

        var handler = new CreateScheduleHandler(schedules, users, tribes, quotaGuards, planLimits);
        var result = await handler.Handle(new CreateScheduleCommand(userId, "Fourth", WorldType.pl218, ScheduleType.Main, []));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("limit reached");
        await schedules.DidNotReceive().AddAsync(Arg.Any<ScheduleEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FreeUserCannotCreateSecondTemplate()
    {
        var userId = Guid.NewGuid();
        var users = Substitute.For<IUserRepository>();
        var templates = Substitute.For<ITargetTemplateRepository>();
        var quotaGuards = FakeQuotaGuards();
        var planLimits = FakePlanLimits();
        users.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new UserEntity { Id = userId, Email = "x@example.com" });
        templates.CountOwnedAsync(userId, Arg.Any<CancellationToken>()).Returns(1);

        var handler = new CreateTargetTemplateHandler(templates, users, quotaGuards, planLimits);
        var result = await handler.Handle(new CreateTargetTemplateCommand(userId, "Second",
            [new TemplateWaveDto(new TimeOnly(10, 0), new TimeOnly(11, 0), 1, CommandTypeConstants.Off)]));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("limit reached");
        await templates.DidNotReceive().CreateAsync(Arg.Any<TargetTemplate>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TroopsUploadIsLimitedPerScheduleInRollingWindow()
    {
        var userId = Guid.NewGuid();
        var scheduleId = Guid.NewGuid();
        var schedules = Substitute.For<IScheduleRepository>();
        var troops = Substitute.For<ITroopsStateRepository>();
        var uploads = Substitute.For<ITroopsUploadRepository>();
        var users = Substitute.For<IUserRepository>();
        var currentUser = Substitute.For<ICurrentUserAccessor>();
        var quotaGuards = FakeQuotaGuards();
        var planLimits = FakePlanLimits();
        currentUser.TryGetUserId(out Arg.Any<Guid>()).Returns(x => { x[0] = userId; return true; });
        schedules.GetByIdAsync(scheduleId, Arg.Any<CancellationToken>())
            .Returns(new ScheduleEntity { Id = scheduleId, UserGuid = userId, Name = "A" });
        users.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new UserEntity { Id = userId, Email = "x@example.com" });
        uploads.ListRecentAsync(scheduleId, Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns([new TroopsUploadEntity(), new TroopsUploadEntity()]);

        var handler = new UploadTroopsStateHandler(schedules, troops, uploads, users, quotaGuards, planLimits, currentUser,
            new TroopsStateValidator(), new TroopsStateCompressionService(), new TroopsStateStatsExtractor());
        var result = await handler.Handle(new UploadTroopsStateCommand(scheduleId, "unused"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("limit reached");
        await uploads.DidNotReceive().RecordAsync(Arg.Any<TroopsUploadEntity>(), Arg.Any<CancellationToken>());
    }

    private static IUserQuotaGuardFactory FakeQuotaGuards()
    {
        var factory = Substitute.For<IUserQuotaGuardFactory>();
        factory.AcquireAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(_ => Substitute.For<IUserQuotaGuard>());
        return factory;
    }

    private static ISubscriptionPlanLimitsRepository FakePlanLimits()
    {
        var repository = Substitute.For<ISubscriptionPlanLimitsRepository>();
        repository.GetAsync(Arg.Any<SubscriptionTier>(), Arg.Any<CancellationToken>())
            .Returns(CreatePlanLimits());
        return repository;
    }

    private static SubscriptionPlanLimitsEntity CreatePlanLimits(
        SubscriptionTier tier = SubscriptionTier.Free,
        int? scheduleLimit = 3,
        int? templateLimit = 1,
        int? troopsUploadLimit = 2,
        int uploadWindowHours = 12) => new()
        {
            SubscriptionTier = tier,
            ScheduleLimit = scheduleLimit,
            TemplateLimit = templateLimit,
            TroopsUploadLimit = troopsUploadLimit,
            TroopsUploadWindowHours = uploadWindowHours
        };
}
