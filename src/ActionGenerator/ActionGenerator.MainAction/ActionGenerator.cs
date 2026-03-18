using ActionGenerator.Domain.Entities;
using ActionGenerator.Domain.Settings;

namespace ActionGenerator.MainAction;

internal class ActionGenerator
{
    public IEnumerable<AttackCommand> Generate(IEnumerable<Village> allyVillages, IEnumerable<Target> targets, ActionSettings settings)
    {
        // Placeholder for the actual command generation logic
        // This should be replaced with the real implementation that generates commands based on the ally and enemy villages
        return Enumerable.Empty<AttackCommand>();
    }
}
