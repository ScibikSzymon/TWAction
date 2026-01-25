using System.Collections.Concurrent;
using TWAction.Application.Common;
using TWAction.Application.Tribes.Interfaces;
using TWAction.Domain.Schedules;
using TWAction.Domain.Tribes;

namespace TWAction.Infrastructure.Services;

/// <summary>
/// Service for fetching TribalWars tribe data with in-memory caching
/// </summary>
public sealed class TribesHttpService(
    HttpClient httpClient,
    TribesCsvParser parser) : ITribesService
{
    private const int CacheDurationMinutes = 15;
    private readonly ConcurrentDictionary<string, (DateTime expiry, List<TribeInfo> data)> _cache = new();

    /// <summary>
    /// Fetches tribes from TribalWars API with 15-minute caching
    /// </summary>
    public async Task<Result<List<TribeInfo>>> GetTribesAsync(WorldType world, CancellationToken cancellationToken = default)
    {
        var worldString = world.ToString();
        var cacheKey = $"tribal_wars_tribes_{worldString}";

        // Try to get from cache
        if (_cache.TryGetValue(cacheKey, out var cached) && DateTime.UtcNow < cached.expiry)
        {
            return Result.Success(cached.data);
        }

        try
        {
            var url = $"https://{worldString}.plemiona.pl/map/ally.txt";
            var response = await httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return Result.Failure<List<TribeInfo>>(
                    $"Failed to fetch tribes from TribalWars. Status: {response.StatusCode}");
            }

            var csvContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var parseResult = parser.Parse(csvContent);

            if (parseResult.IsFailure)
            {
                return parseResult;
            }

            // Sort by VillagesCount descending
            var sortedTribes = parseResult.Value
                .OrderByDescending(t => t.VillagesCount)
                .ToList();

            // Cache for 15 minutes
            var expiry = DateTime.UtcNow.AddMinutes(CacheDurationMinutes);
            _cache[cacheKey] = (expiry, sortedTribes);

            return Result.Success(sortedTribes);
        }
        catch (HttpRequestException ex)
        {
            return Result.Failure<List<TribeInfo>>($"HTTP request error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result.Failure<List<TribeInfo>>($"Error fetching tribes: {ex.Message}");
        }
    }
}


