using ActionGenerator.Application.Common.DTOs;
using ActionGenerator.Application.Common.Mappers;
using ActionGenerator.Application.Common.Services;
using ActionGenerator.Application.Features.MainActions.Dtos;
using ActionGenerator.Domain.Settings;
using ActionGenerator.MainAction;

namespace ActionGenerator.Application.Features.MainActions.Services;

public interface IMainActionsService
{
    IReadOnlyList<AttackCommandDto> Generate(
        GenerateMainActionRequest request,
        CancellationToken cancellationToken = default);
}

internal sealed class MainActionsService(
    IActionGenerator actionGenerator,
    IFrontDistanceCalculator frontDistanceCalculator) : IMainActionsService
{
    public IReadOnlyList<AttackCommandDto> Generate(
        GenerateMainActionRequest request,
        CancellationToken cancellationToken = default)
    {
        var allyVillages = request.AllyVillages.ToEntities();
        var targets = request.Targets.ToEntities();

        frontDistanceCalculator.CalculateFrontDistances(allyVillages, targets);

        var settings = BuildSettings(request);

        var commands = actionGenerator.Generate(allyVillages, targets, settings);

        return commands.ToDtos();
    }

    private static ActionSettings BuildSettings(GenerateMainActionRequest request) =>
        new()
        {
            MinDepartureTime = request.MinDepartureTime,
            SkipNightSendings = request.SkipNightSendings,
            MaxNobleDistance = request.MaxNobleDistance,
            OffSettings = request.OffSettings,
            CatasSettings = request.CatasSettings,
            FakeOffSettings = request.FakeOffSettings,
            FakeDeffSettings = request.FakeDeffSettings,
            NobleSettings = request.NobleSettings,
            PlayerNobleBudgets = request.PlayerNobleBudgets
        };
}
