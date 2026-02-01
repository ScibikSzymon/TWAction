using ActionGenerator.Domain.Enums;

namespace ActionGenerator.Domain.Configuration;

public sealed record UnitConfiguration
{
    public required CommandType CommandType { get; init; }
    public required int SpeedMinutesPerField { get; init; }}

public static class UnitConfigurationProvider
{
    private static readonly Dictionary<CommandType, UnitConfiguration> _configurations = new()
    {
        [CommandType.Reconnaissance] = new UnitConfiguration
        {
            CommandType = CommandType.Reconnaissance,
            SpeedMinutesPerField = 9
        },
        [CommandType.Off] = new UnitConfiguration
        {
            CommandType = CommandType.Off,
            SpeedMinutesPerField = 30
        },
        [CommandType.FakeOffensive] = new UnitConfiguration
        {
            CommandType = CommandType.FakeOffensive,
            SpeedMinutesPerField = 30
        },
        [CommandType.FakeDefensive] = new UnitConfiguration
        {
            CommandType = CommandType.FakeDefensive,
            SpeedMinutesPerField = 30
        },
        [CommandType.Catapults] = new UnitConfiguration
        {
            CommandType = CommandType.Catapults,
            SpeedMinutesPerField = 30
        },
        [CommandType.NobleWithFullOffensive] = new UnitConfiguration
        {
            CommandType = CommandType.NobleWithFullOffensive,
            SpeedMinutesPerField = 35
        },
        [CommandType.NobleWithHalfOffensive] = new UnitConfiguration
        {
            CommandType = CommandType.NobleWithHalfOffensive,
            SpeedMinutesPerField = 35
        },
        [CommandType.NobleWithQuarterOffensive] = new UnitConfiguration
        {
            CommandType = CommandType.NobleWithQuarterOffensive,
            SpeedMinutesPerField = 35
        },
        [CommandType.NobleWith150Axes] = new UnitConfiguration
        {
            CommandType = CommandType.NobleWith150Axes,
            SpeedMinutesPerField = 35
        },
        [CommandType.NobleWith100HeavyCavalry] = new UnitConfiguration
        {
            CommandType = CommandType.NobleWith100HeavyCavalry,
            SpeedMinutesPerField = 35
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

    public static int GetSpeedMinutesPerField(CommandType commandType) => GetConfiguration(commandType).SpeedMinutesPerField;
}
