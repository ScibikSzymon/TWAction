using System.Net.Http.Json;
using TWAction.Application.ReconnaissanceActions.DTOs;
using TWAction.Application.ReconnaissanceActions.Interfaces;

namespace TWAction.Infrastructure.Services;

public sealed class GeneratorApiClient(HttpClient httpClient) : IGeneratorApiClient
{
    public async Task<IReadOnlyList<AttackCommandDto>> GenerateReconnaissanceActionsAsync(
        GenerateReconnaissanceActionsRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync(
            "/Api/reconnaissance-actions",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var commands = await response.Content.ReadFromJsonAsync<List<AttackCommandDto>>(cancellationToken) 
            ?? throw new InvalidOperationException("Generator.Api returned null response.");
        return commands;
    }
}
