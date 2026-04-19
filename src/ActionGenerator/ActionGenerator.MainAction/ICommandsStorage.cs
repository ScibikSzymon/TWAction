using ActionGenerator.Domain.Entities;

namespace ActionGenerator.MainAction;

public interface ICommandsStorage
{
    IReadOnlyList<AttackCommand> Commands { get; }
    IReadOnlyDictionary<int, IReadOnlyList<AttackCommand>> CommandsBySource { get; }
    IReadOnlyList<AttackCommand> GetCommandsFromSource(int sourceId);
    void Add(IEnumerable<AttackCommand> commands);
}
