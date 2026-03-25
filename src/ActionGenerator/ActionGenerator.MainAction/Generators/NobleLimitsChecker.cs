using ActionGenerator.Domain.Entities;
using ActionGenerator.Domain.Enums;
using ActionGenerator.Domain.Settings;

namespace ActionGenerator.MainAction.Generators;

/// <summary>
/// Evaluates noble production eligibility by querying <see cref="ICommandsStorage"/> directly.
/// All production rules live here:
///   - village garrison (Army.Noble)
///   - per-type cap per village (game rule derived from army composition)
///   - per-village-per-player cap (prevents flooding one player from one village)
///   - player noble budget (filled in by the user on the frontend)
/// </summary>
internal sealed class NobleLimitsChecker(ICommandsStorage storage)
{
    internal static readonly IReadOnlyList<CommandType> NobleCommandTypes =
    [
        CommandType.NobleWithFullOff,
        CommandType.NobleWithHalfOff,
        CommandType.NobleWithQuarterOffensive,
        CommandType.NobleWith150Axes,
        CommandType.NobleWith100HeavyCavalry,
        CommandType.RandomNoble,
    ];

    public bool IsEligible(
        AttackCommand command,
        CommandType commandType,
        IReadOnlyDictionary<int, uint> playerBudgets)
    {
        var sourceId = command.Source.Id;
        var source = (SourceVillage)command.Source;

        return !HasExhaustedGarrisonNobles(sourceId, source.Army.Noble) &&
               !HasExhaustedTypeLimit(sourceId, commandType) &&
               !HasExhaustedPerPlayerLimit(sourceId, command.Target.PlayerId, commandType) &&
               !HasExhaustedPlayerBudget(command.Source.PlayerId, playerBudgets);
    }

    public bool HasNoblesInGarrison(SourceVillage source) =>
        !HasExhaustedGarrisonNobles(source.Id, source.Army.Noble);

    public int CountEligible(
        List<AttackCommand> commands,
        CommandType commandType,
        IReadOnlyDictionary<int, uint> playerBudgets)
        => commands.Count(cmd => IsEligible(cmd, commandType, playerBudgets));

    public List<AttackCommand> GetEligible(
        List<AttackCommand> commands,
        CommandType commandType,
        IReadOnlyDictionary<int, uint> playerBudgets)
        => commands.Where(cmd => IsEligible(cmd, commandType, playerBudgets)).ToList();

    // -------------------------------------------------------------------------
    // Individual limit checks
    // -------------------------------------------------------------------------

    private bool HasExhaustedGarrisonNobles(int sourceId, uint nobleCapacity) =>
        (uint)NobleCommandsFromSource(sourceId) >= nobleCapacity;

    private bool HasExhaustedTypeLimit(int sourceId, CommandType commandType) =>
        (uint)storage.GetCommandsFromSource(sourceId)
            .Count(cmd => cmd.Target.CommandType == commandType)
        >= MaxPerVillageForType(commandType);

    // Counts all noble commands from this village toward one enemy player (across all noble types).
    // A village that already sent a FullOff noble to player P counts against HalfOff's per-player cap too.
    private bool HasExhaustedPerPlayerLimit(int sourceId, int targetPlayerId, CommandType commandType) =>
        (uint)storage.GetCommandsFromSource(sourceId)
            .Count(cmd => cmd.Target.PlayerId == targetPlayerId && IsNobleCommandType(cmd.Target.CommandType))
        >= MaxPerPlayerForType(commandType);

    private bool HasExhaustedPlayerBudget(int sourcePlayerId, IReadOnlyDictionary<int, uint> playerBudgets)
    {
        if (!playerBudgets.TryGetValue(sourcePlayerId, out var budget))
            return false;

        var used = (uint)storage.Commands
            .Count(cmd => cmd.Source.PlayerId == sourcePlayerId && IsNobleCommandType(cmd.Target.CommandType));

        return used >= budget;
    }

    private int NobleCommandsFromSource(int sourceId) =>
        storage.GetCommandsFromSource(sourceId)
            .Count(cmd => IsNobleCommandType(cmd.Target.CommandType));

    // -------------------------------------------------------------------------
    // Game-rule constants
    // -------------------------------------------------------------------------

    /// <summary>
    /// Maximum noble commands per village for a given type.
    /// FullOff sends the entire army ? 1 slot; QuarterOff sends ? ? up to 4 slots, etc.
    /// </summary>
    internal static uint MaxPerVillageForType(CommandType commandType) => commandType switch
    {
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
    /// Mirrors maxAttackOnPlayerFromVillage from the old generator.
    /// </summary>
    internal static uint MaxPerPlayerForType(CommandType commandType) => commandType switch
    {
        CommandType.NobleWithFullOff            => 1,
        CommandType.NobleWithHalfOff            => 2,
        CommandType.NobleWithQuarterOffensive   => 4,
        CommandType.NobleWith150Axes            => 2,
        CommandType.NobleWith100HeavyCavalry    => 6,
        CommandType.RandomNoble                 => 2,
        _ => throw new ArgumentOutOfRangeException(nameof(commandType))
    };

    internal static bool IsNobleCommandType(CommandType commandType) =>
        NobleCommandTypes.Contains(commandType);
}
