using ActionGenerator.Domain.Entities;
using ActionGenerator.Domain.Enums;

namespace ActionGenerator.MainAction.Generators;

internal sealed class NobleLimitsChecker(ICommandsStorage storage)
{
    internal static readonly IReadOnlyList<CommandType> NobleCommandTypes =
    [
        CommandType.NobleWithDeff,
        CommandType.NobleWithQuarterOffensive,
        CommandType.NobleWithHalfOff,
        CommandType.NobleWithFullOff,
        CommandType.NobleWith150Axes,
        CommandType.NobleWith100HeavyCavalry,
        CommandType.RandomNoble,
    ];

    public bool IsAllowed(
        AttackCommand command,
        IReadOnlyDictionary<int, uint> playerBudgets)
    {
        var sourceId = command.Source.Id;

        return !HasFinishedTypeLimitFromSource(sourceId, command.Target.CommandType) &&
               !HasFinishedPerTargetPlayerLimit(sourceId, command.Target.PlayerId, command.Target.CommandType) &&
               !HasFinishedPlayerBudget(command.Source.PlayerId, playerBudgets);
    }

    public int CountAllowed(
        List<AttackCommand> commands,
        IReadOnlyDictionary<int, uint> playerBudgets)
        => commands.Count(cmd => IsAllowed(cmd, playerBudgets));

    public List<AttackCommand> GetAllowed(
        List<AttackCommand> commands,
        IReadOnlyDictionary<int, uint> playerBudgets)
        => commands.Where(cmd => IsAllowed(cmd, playerBudgets)).ToList();

    private bool HasFinishedTypeLimitFromSource(int sourceId, CommandType commandType) =>
        (uint)storage.GetCommandsFromSource(sourceId)
            .Count(cmd => cmd.Target.CommandType == commandType)
        >= MaxPerVillageForType(commandType);

    private bool HasFinishedPerTargetPlayerLimit(int sourceId, int targetPlayerId, CommandType commandType) =>
        (uint)storage.GetCommandsFromSource(sourceId)
            .Count(cmd => cmd.Target.PlayerId == targetPlayerId && IsNobleCommandType(cmd.Target.CommandType))
        >= MaxPerPlayerForType(commandType);

    private bool HasFinishedPlayerBudget(int sourcePlayerId, IReadOnlyDictionary<int, uint> playerBudgets)
    {
        if (!playerBudgets.TryGetValue(sourcePlayerId, out var budget))
            return false;

        var used = (uint)storage.Commands
            .Count(cmd => cmd.Source.PlayerId == sourcePlayerId && IsNobleCommandType(cmd.Target.CommandType));

        return used >= budget;
    }

    /// <summary>
    /// Maximum noble commands from village for a given type.
    /// </summary>
    internal static uint MaxPerVillageForType(CommandType commandType) => commandType switch
    {
        CommandType.NobleWithDeff               => 1,
        CommandType.NobleWithFullOff            => 1,
        CommandType.NobleWithHalfOff            => 2,
        CommandType.NobleWithQuarterOffensive   => 4,
        CommandType.NobleWith150Axes            => 6,
        CommandType.NobleWith100HeavyCavalry    => 6,
        CommandType.RandomNoble                 => uint.MaxValue,
        _ => throw new ArgumentOutOfRangeException(nameof(commandType))
    };

    /// <summary>
    /// Maximum noble commands one village may send toward the same destination player.
    /// </summary>
    internal static uint MaxPerPlayerForType(CommandType commandType) => commandType switch
    {
        CommandType.NobleWithDeff               => 1,
        CommandType.NobleWithFullOff            => 1,
        CommandType.NobleWithHalfOff            => 2,
        CommandType.NobleWithQuarterOffensive   => 4,
        CommandType.NobleWith150Axes            => 2,
        CommandType.NobleWith100HeavyCavalry    => 2,
        CommandType.RandomNoble                 => 4,
        _ => throw new ArgumentOutOfRangeException(nameof(commandType))
    };

    internal static bool IsNobleCommandType(CommandType commandType) =>
        NobleCommandTypes.Contains(commandType);
}
