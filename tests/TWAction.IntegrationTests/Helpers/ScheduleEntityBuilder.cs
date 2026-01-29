using TWAction.Domain.Schedules;

namespace TWAction.IntegrationTests.Helpers;

public sealed class ScheduleEntityBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _userGuid = Guid.NewGuid();
    private string _name = "Test Schedule";
    private DateTime _creationDate = DateTime.UtcNow;
    private WorldType _world = WorldType.pl218;
    private ScheduleType _scheduleType = ScheduleType.Main;

    public ScheduleEntityBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public ScheduleEntityBuilder WithUserGuid(Guid userGuid)
    {
        _userGuid = userGuid;
        return this;
    }

    public ScheduleEntityBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public ScheduleEntityBuilder WithCreationDate(DateTime creationDate)
    {
        _creationDate = creationDate;
        return this;
    }

    public ScheduleEntityBuilder WithWorld(WorldType world)
    {
        _world = world;
        return this;
    }

    public ScheduleEntityBuilder WithScheduleType(ScheduleType scheduleType)
    {
        _scheduleType = scheduleType;
        return this;
    }

    public ScheduleEntity Build()
    {
        return new ScheduleEntity
        {
            Id = _id,
            UserGuid = _userGuid,
            Name = _name,
            CreationDate = _creationDate,
            World = _world,
            ScheduleType = _scheduleType
        };
    }
}
