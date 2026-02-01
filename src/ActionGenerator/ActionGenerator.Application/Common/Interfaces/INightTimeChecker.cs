namespace ActionGenerator.Application.Common.Interfaces;

public interface INightTimeChecker
{
    bool IsNightTime(DateTimeOffset time);
}
