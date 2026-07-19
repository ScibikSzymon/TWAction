using ActionGenerator.Domain.Entities;
using ActionGenerator.Domain.Settings;
using ActionGenerator.MainAction.Generators;

namespace ActionGenerator.MainAction;

internal sealed class ActionGenerator(
    IEnumerable<ICommandTypeGenerator> generators,
    ICommandsStorage storage) : IActionGenerator
{
    public IReadOnlyList<AttackCommand> Generate(
        IReadOnlyList<SourceVillage> allyVillages,
        IReadOnlyList<Target> targets,
        ActionSettings settings)
    {
        foreach (var generator in generators)
            generator.Generate(allyVillages, targets, settings);

        return storage.Commands;
    }
}
