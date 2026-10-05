using TWAction.Application.Common;
using TWAction.Application.Templates.Interfaces;

namespace TWAction.Application.Templates.Commands;

/// <summary>Permanently removes a user-owned template.</summary>
public sealed record DeleteTargetTemplateCommand(Guid TemplateId, Guid UserId);

public class DeleteTargetTemplateHandler(ITargetTemplateRepository repository)
{
    public async Task<Result> Handle(
        DeleteTargetTemplateCommand command,
        CancellationToken ct = default)
    {
        var template = await repository.GetByIdAsync(command.TemplateId, ct);

        if (template is null || template.UserId != command.UserId)
        {
            return Result.Failure("Template not found.");
        }

        if (template.IsDefault)
        {
            return Result.Failure("Default templates cannot be deleted.");
        }

        await repository.DeleteAsync(template, ct);
        return Result.Success();
    }
}
