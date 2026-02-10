using System.IO.Compression;
using Microsoft.Extensions.Caching.Memory;
using TWAction.Application.Tribes.Interfaces;
using TWAction.Domain.Schedules;
using TWAction.Domain.Tribes;

namespace TWAction.Infrastructure.Services;

/// <summary>
/// Service for fetching TribalWars data with in-memory caching
/// </summary>
public sealed class TribesHttpService(
    HttpClient httpClient,
    TribesCsvParser tribesParser,
    PlayersCsvParser playersParser,
    VillagesCsvParser villagesParser,
    IMemoryCache cache) : ITribesService
{
    private const int CacheDurationMinutes = 15;

    /// <summary>
    /// Fetches tribes from TribalWars Api with 15-minute caching (gzip compressed)
    /// </summary>
    public async Task<IReadOnlyList<TribeInfo>> GetTribesAsync(WorldType world, CancellationToken cancellationToken = default)
    {
        var worldString = world.ToString();
        var cacheKey = $"tw_tribes_{worldString}";

        // Try to get from cache
        if (cache.TryGetValue(cacheKey, out IReadOnlyList<TribeInfo>? cachedTribes))
        {
            return cachedTribes;
        }

        // Download gzip compressed file
        var url = $"https://{worldString}.plemiona.pl/map/ally.txt.gz";
        var response = await httpClient.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Failed to fetch tribes from TribalWars. Status: {response.StatusCode}");
        }

        // Decompress gzip and read content
        await using var compressedStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
        using var reader = new StreamReader(gzipStream);
        var csvContent = await reader.ReadToEndAsync(cancellationToken);

        var tribes = tribesParser.Parse(csvContent);

        // Sort by VillagesCount descending
        var sortedTribes = tribes
            .Where(e => e.VillagesCount > 0)
            .OrderByDescending(t => t.VillagesCount)
            .ToList();

        // Cache for 15 minutes
        cache.Set(cacheKey, sortedTribes, TimeSpan.FromMinutes(CacheDurationMinutes));

        return sortedTribes;
    }

    /// <summary>
    /// Fetches players from TribalWars API with 15-minute caching (gzip compressed)
    /// </summary>
    public async Task<Dictionary<int, PlayerInfo>> GetPlayersAsync(WorldType world, CancellationToken cancellationToken = default)
    {
        var worldString = world.ToString();
        var cacheKey = $"tw_players_{worldString}";

        // Try to get from cache
        if (cache.TryGetValue(cacheKey, out Dictionary<int, PlayerInfo>? cachedPlayers))
        {
            return cachedPlayers;
        }

        // Download gzip compressed file
        var url = $"https://{worldString}.plemiona.pl/map/player.txt.gz";
        var response = await httpClient.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Failed to fetch players from TribalWars. Status: {response.StatusCode}");
        }

        // Decompress gzip and read content
        await using var compressedStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
        using var reader = new StreamReader(gzipStream);
        var csvContent = await reader.ReadToEndAsync(cancellationToken);

        var players = playersParser.Parse(csvContent);

        // Cache for 15 minutes
        cache.Set(cacheKey, players, TimeSpan.FromMinutes(CacheDurationMinutes));

        return players;
    }

    /// <summary>
    /// Fetches villages from TribalWars API with 15-minute caching (gzip compressed)
    /// Links villages with player data automatically.
    /// </summary>
    public async Task<(Dictionary<int, VillageInfo> ById, Dictionary<int, VillageInfo> ByCoordinates)> GetVillagesAsync(
        WorldType world, 
        CancellationToken cancellationToken = default)
    {
        var worldString = world.ToString();
        var cacheKey = $"tw_villages_{worldString}";

        // Try to get from cache
        if (cache.TryGetValue(cacheKey, out (Dictionary<int, VillageInfo>, Dictionary<int, VillageInfo>)? cachedVillages))
        {
            return cachedVillages.Value;
        }

        // First, fetch players to link with villages
        var players = await GetPlayersAsync(world, cancellationToken);

        // Download gzip compressed village file
        var url = $"https://{worldString}.plemiona.pl/map/village.txt.gz";
        var response = await httpClient.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Failed to fetch villages from TribalWars. Status: {response.StatusCode}");
        }

        // Decompress gzip and read content
        await using var compressedStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
        using var reader = new StreamReader(gzipStream);
        var csvContent = await reader.ReadToEndAsync(cancellationToken);

        var villages = villagesParser.Parse(csvContent, players);

        // Cache for 15 minutes
        cache.Set(cacheKey, villages, TimeSpan.FromMinutes(CacheDurationMinutes));

        return villages;
    }
}
