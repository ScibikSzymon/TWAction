using ActionGenerator.Domain.Entities;
using ActionGenerator.Domain.Settings;

namespace ActionGenerator.MainAction.Generators;

internal interface ICommandTypeGenerator
{
    void Generate(
        IReadOnlyList<SourceVillage> allyVillages,
        IReadOnlyList<Target> targets,
        ActionSettings settings);
}
