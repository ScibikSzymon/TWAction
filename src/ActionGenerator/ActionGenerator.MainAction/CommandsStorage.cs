using ActionGenerator.Domain.Entities;

namespace ActionGenerator.MainAction;

internal sealed class CommandsStorage : ICommandsStorage
{
    private readonly List<AttackCommand> _commands = [];
    private readonly Dictionary<int, List<AttackCommand>> _bySource = new();

    public IReadOnlyList<AttackCommand> Commands => _commands;

    public IReadOnlyDictionary<int, IReadOnlyList<AttackCommand>> CommandsBySource
        => _bySource.ToDictionary(kvp => kvp.Key, kvp => (IReadOnlyList<AttackCommand>)kvp.Value);

    public void Add(IEnumerable<AttackCommand> commands)
    {
        foreach (var command in commands)
        {
            _commands.Add(command);

            if (!_bySource.TryGetValue(command.Source.Id, out var sourceList))
            {
                sourceList = [];
                _bySource[command.Source.Id] = sourceList;
            }

            sourceList.Add(command);
        }
    }
}
