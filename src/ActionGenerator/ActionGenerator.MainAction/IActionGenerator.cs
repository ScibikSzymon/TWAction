using ActionGenerator.Domain.Entities;
using ActionGenerator.Domain.Settings;

namespace ActionGenerator.MainAction;

public interface IActionGenerator
{
    IReadOnlyList<AttackCommand> Generate(
        IReadOnlyList<SourceVillage> allyVillages,
        IReadOnlyList<Target> targets,
        ActionSettings settings);
}
