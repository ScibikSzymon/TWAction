using ActionGenerator.Domain.Entities;

namespace ActionGenerator.Application.Common.Interfaces;

public interface ICommandGenerator
{
    AttackCommand Generate(Village source, Target target);
}
