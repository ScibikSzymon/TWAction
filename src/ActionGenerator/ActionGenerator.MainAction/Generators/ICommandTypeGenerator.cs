using ActionGenerator.Domain.Entities;
using ActionGenerator.Domain.Settings;

namespace ActionGenerator.MainAction.Generators;

internal interface ICommandTypeGenerator
{
    IReadOnlyList<AttackCommand> Generate(
        IReadOnlyList<SourceVillage> allyVillages,
        IReadOnlyList<Target> targets,
        ActionSettings settings,
        IReadOnlyList<AttackCommand> alreadyGenerated);
}
