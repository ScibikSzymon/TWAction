using System.Net.Http.Json;
using TWAction.Application.AttackCommands.DTOs;
using TWAction.Application.AttackCommands.Interfaces;

namespace TWAction.Infrastructure.Services;

/// <summary>
/// HTTP client for plemionarozpiski.pl API.
/// </summary>
public sealed class PlemionaRozpiskiApiClient(HttpClient httpClient) : IPlemionaRozpiskiApiClient
{
    public async Task<PlemionaRozpiskiSendResult> SendCommandsAsync(
        IReadOnlyList<PlemionaRozpiskiCommandDto> commands,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(
                "/api/commands/admin",
                commands,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return new PlemionaRozpiskiSendResult
                {
                    Success = true,
                    CommandsSentCount = commands.Count
                };
            }

            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            return new PlemionaRozpiskiSendResult
            {
                Success = false,
                CommandsSentCount = 0,
                ErrorMessage = $"HTTP {(int)response.StatusCode}: {errorContent}"
            };
        }
        catch (Exception ex)
        {
            return new PlemionaRozpiskiSendResult
            {
                Success = false,
                CommandsSentCount = 0,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<bool> DeleteCommandsAsync(
        string operationName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var deleteRequest = new { operationName };
            var request = new HttpRequestMessage(HttpMethod.Delete, "/api/commands/admin")
            {
                Content = JsonContent.Create(deleteRequest)
            };

            var response = await httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
