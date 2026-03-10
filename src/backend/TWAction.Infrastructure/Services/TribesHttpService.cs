using System.IO.Compression;
using Microsoft.Extensions.Caching.Memory;
using TWAction.Application.Tribes.Interfaces;
using TWAction.Domain.Schedules;
using TWAction.Domain.Tribes;

namespace TWAction.Infrastructure.Services;

/// <summary>
/// Service for fetching TribalWars tribe data with in-memory caching
/// </summary>
public sealed class TribesHttpService(
    HttpClient httpClient,
    TribesCsvParser parser,
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
            return cachedTribes!;
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

        var tribes = parser.Parse(csvContent);

        // Sort by VillagesCount descending
        var sortedTribes = tribes
            .Where(e => e.VillagesCount > 0)
            .OrderByDescending(t => t.VillagesCount)
            .ToList();

        // Cache for 15 minutes
        cache.Set(cacheKey, sortedTribes, TimeSpan.FromMinutes(CacheDurationMinutes));

        return sortedTribes;
    }
}
