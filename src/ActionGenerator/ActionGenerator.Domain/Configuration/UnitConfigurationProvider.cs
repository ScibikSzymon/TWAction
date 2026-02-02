using ActionGenerator.Domain.Enums;

namespace ActionGenerator.Domain.Configuration;

public sealed record UnitConfiguration
{
    public required CommandType CommandType { get; init; }
    public required int TimePerFieldInMinutes { get; init; }}

public static class UnitConfigurationProvider
{
    private static readonly Dictionary<CommandType, UnitConfiguration> _configurations = new()
    {
        [CommandType.Reconnaissance] = new UnitConfiguration
        {
            CommandType = CommandType.Reconnaissance,
            TimePerFieldInMinutes = 9
        },
        [CommandType.Off] = new UnitConfiguration
        {
            CommandType = CommandType.Off,
            TimePerFieldInMinutes = 30
        },
        [CommandType.FakeOffensive] = new UnitConfiguration
        {
            CommandType = CommandType.FakeOffensive,
            TimePerFieldInMinutes = 30
        },
        [CommandType.FakeDefensive] = new UnitConfiguration
        {
            CommandType = CommandType.FakeDefensive,
            TimePerFieldInMinutes = 30
        },
        [CommandType.Catapults] = new UnitConfiguration
        {
            CommandType = CommandType.Catapults,
            TimePerFieldInMinutes = 30
        },
        [CommandType.NobleWithFullOff] = new UnitConfiguration
        {
            CommandType = CommandType.NobleWithFullOff,
            TimePerFieldInMinutes = 35
        },
        [CommandType.NobleWithHalfOff] = new UnitConfiguration
        {
            CommandType = CommandType.NobleWithHalfOff,
            TimePerFieldInMinutes = 35
        },
        [CommandType.NobleWithQuarterOffensive] = new UnitConfiguration
        {
            CommandType = CommandType.NobleWithQuarterOffensive,
            TimePerFieldInMinutes = 35
        },
        [CommandType.NobleWith150Axes] = new UnitConfiguration
        {
            CommandType = CommandType.NobleWith150Axes,
            TimePerFieldInMinutes = 35
        },
        [CommandType.NobleWith100HeavyCavalry] = new UnitConfiguration
        {
            CommandType = CommandType.NobleWith100HeavyCavalry,
            TimePerFieldInMinutes = 35
        }
    };

    private static UnitConfiguration GetConfiguration(CommandType commandType)
    {
        if (_configurations.TryGetValue(commandType, out var config))
        {
            return config;
        }

        throw new ArgumentException($"No configuration found for command type: {commandType}", nameof(commandType));
    }

    public static int GetTimePerFieldInMinutes(CommandType commandType) => GetConfiguration(commandType).TimePerFieldInMinutes;
}
