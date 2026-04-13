using TWAction.Application.MainActions.DTOs;
using TWAction.Application.ReconnaissanceActions.DTOs;

namespace TWAction.Application.ReconnaissanceActions.Interfaces;

/// <summary>
/// Client for communicating with Generator.Api.
/// </summary>
public interface IGeneratorApiClient
{
    /// <summary>
    /// Calls Generator.Api to generate reconnaissance attack commands.
    /// </summary>
    Task<IReadOnlyList<AttackCommandDto>> GenerateReconnaissanceActionsAsync(
        GenerateReconnaissanceActionsRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calls Generator.Api to generate main action attack commands.
    /// </summary>
    Task<IReadOnlyList<AttackCommandDto>> GenerateMainActionsAsync(
        GenerateMainActionsRequest request,
        CancellationToken cancellationToken = default);
}

