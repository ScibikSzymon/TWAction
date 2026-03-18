using ActionGenerator.Domain.Entities;
using ActionGenerator.Domain.Settings;

namespace ActionGenerator.MainAction.Generators;

internal sealed partial class NobleGenerator
{
    private readonly record struct SourcePlayerKey(int SourceId, int PlayerId);

    /// <summary>
    /// Tracks noble assignments for one command-type pass.
    /// <c>_typeUsed</c> resets per instance (per type).
    /// <c>_perPlayerUsed</c> is a shared reference so cross-type limits are enforced.
    /// </summary>
    private sealed class NobleTracker(
        Dictionary<SourcePlayerKey, uint> perPlayerUsed,
        NobleSettings settings,
        uint maxPerVillageForType)
    {
        private readonly Dictionary<int, uint> _typeUsed = new();

        public bool IsEligible(AttackCommand command) =>
            !HasReachedTypeLimit(command.Source.Id) &&
            !HasReachedPlayerLimit(KeyFor(command));

        public void Record(AttackCommand command)
        {
            Increment(_typeUsed, command.Source.Id);
            Increment(perPlayerUsed, KeyFor(command));
        }

        public int CountEligible(List<AttackCommand> commands) => commands.Count(IsEligible);
        public List<AttackCommand> GetEligible(List<AttackCommand> commands) => commands.Where(IsEligible).ToList();

        private bool HasReachedTypeLimit(int sourceId) =>
            HasReached(_typeUsed, sourceId, maxPerVillageForType);

        private bool HasReachedPlayerLimit(SourcePlayerKey key) =>
            HasReached(perPlayerUsed, key, settings.MaxNoblesPerVillagePerPlayer);

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

