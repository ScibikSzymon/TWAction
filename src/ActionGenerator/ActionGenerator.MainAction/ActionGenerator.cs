using ActionGenerator.Domain.Entities;
using ActionGenerator.Domain.Settings;
using ActionGenerator.MainAction.Generators;

namespace ActionGenerator.MainAction;

internal sealed class ActionGenerator : IActionGenerator
{
    private readonly IEnumerable<ICommandTypeGenerator> _generators;

    public ActionGenerator(IEnumerable<ICommandTypeGenerator> generators)
    {
        _generators = generators;
    }

    public IReadOnlyList<AttackCommand> Generate(
        IReadOnlyList<SourceVillage> allyVillages,
        IReadOnlyList<Target> targets,
        ActionSettings settings)
    {
        var result = new List<AttackCommand>();

        foreach (var generator in _generators)
        {
            var commands = generator.Generate(allyVillages, targets, settings, result);
            result.AddRange(commands);
        }

        return result;
    }
}
