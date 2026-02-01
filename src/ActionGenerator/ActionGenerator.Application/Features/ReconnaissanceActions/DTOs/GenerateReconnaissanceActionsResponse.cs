namespace ActionGenerator.Application.Features.ReconnaissanceActions.DTOs;

public sealed record GenerateReconnaissanceActionsResponse
{
    public required IReadOnlyList<AttackCommandDto> Commands { get; init; }
}
