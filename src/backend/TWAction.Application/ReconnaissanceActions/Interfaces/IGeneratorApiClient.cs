using TWAction.Application.ReconnaissanceActions.DTOs;

namespace TWAction.Application.ReconnaissanceActions.Interfaces;

/// <summary>
/// Client for communicating with Generator.Api to generate reconnaissance actions.
/// </summary>
public interface IGeneratorApiClient
{
    /// <summary>
    /// Calls Generator.Api to generate reconnaissance attack commands.
    /// </summary>
    Task<IReadOnlyList<AttackCommandDto>> GenerateReconnaissanceActionsAsync(
        GenerateReconnaissanceActionsRequest request,
        CancellationToken cancellationToken = default);
}
