using ActionGenerator.Domain.Entities;

namespace ActionGenerator.MainAction.Generators;

internal sealed partial class NobleGenerator
{
    private readonly record struct SourcePlayerKey(int SourceId, int PlayerId);

    /// <summary>
    /// Tracks noble assignments for one command-type pass.
    ///
    /// Shared references (<c>totalUsed</c>, <c>perPlayerUsed</c>, <c>playerBudgetUsed</c>)
    /// accumulate across all command-type passes, so every limit is enforced globally.
    /// <c>_typeUsed</c> is fresh per instance, resetting the per-type cap each iteration.
    /// </summary>
    private sealed class NobleTracker(
        Dictionary<int, uint> totalUsed,
        Dictionary<SourcePlayerKey, uint> perPlayerUsed,
        Dictionary<int, uint> playerBudgetUsed,
        IReadOnlyDictionary<int, uint> playerBudgets,
        uint maxPerVillageForType,
        uint maxPerPlayerForType)
    {
        private readonly Dictionary<int, uint> _typeUsed = new();

        public bool IsEligible(AttackCommand command)
        {
            var sourceId = command.Source.Id;
            var nobleCapacity = ((SourceVillage)command.Source).Army.Noble;

            return !HasReachedTotalLimit(sourceId, nobleCapacity) &&
                   !HasReachedTypeLimit(sourceId) &&
                   !HasReachedPlayerLimit(KeyFor(command)) &&
                   !HasReachedPlayerBudget(command.Source.PlayerId);
        }

        public void Record(AttackCommand command)
        {
            Increment(totalUsed, command.Source.Id);
            Increment(_typeUsed, command.Source.Id);
            Increment(perPlayerUsed, KeyFor(command));
            Increment(playerBudgetUsed, command.Source.PlayerId);
        }

        public int CountEligible(List<AttackCommand> commands) => commands.Count(IsEligible);
        public List<AttackCommand> GetEligible(List<AttackCommand> commands) => commands.Where(IsEligible).ToList();

        private bool HasReachedTotalLimit(int sourceId, uint nobleCapacity) =>
            HasReached(totalUsed, sourceId, nobleCapacity);

        private bool HasReachedTypeLimit(int sourceId) =>
            HasReached(_typeUsed, sourceId, maxPerVillageForType);

        private bool HasReachedPlayerLimit(SourcePlayerKey key) =>
            HasReached(perPlayerUsed, key, maxPerPlayerForType);

        private bool HasReachedPlayerBudget(int sourcePlayerId) =>
            HasReached(playerBudgetUsed, sourcePlayerId, PlayerBudget(sourcePlayerId));

        private uint PlayerBudget(int playerId) =>
            playerBudgets.TryGetValue(playerId, out var budget) ? budget : uint.MaxValue;

        private static SourcePlayerKey KeyFor(AttackCommand command) =>
            new(command.Source.Id, command.Target.PlayerId);

        private static bool HasReached<TKey>(Dictionary<TKey, uint> dict, TKey key, uint limit)
            where TKey : notnull
            => dict.GetValueOrDefault(key) >= limit;

        private static void Increment<TKey>(Dictionary<TKey, uint> dict, TKey key)
            where TKey : notnull
            => dict[key] = dict.GetValueOrDefault(key) + 1;
    }
}

