using ActionGenerator.Domain.Entities;
using ActionGenerator.MainAction;

namespace ActionGenerator.Application.Common.Services;

public interface ICommandFactory
{
    AttackCommand Generate(Village source, Target target);
}

internal sealed class CommandFactory : ICommandFactory
{
    public AttackCommand Generate(Village source, Target target) =>
        MainAction.CommandFactory.Create(source, target);
}

