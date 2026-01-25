using System.IO.Compression;
using System.Text;
using TWAction.Application.Common;

namespace TWAction.Application.Schedules.Services;

public sealed class TroopsStateCompressionService
{
    /// <summary>
    /// Compresses raw troops data to base64 encoded gzip format
    /// </summary>
    public string Compress(string rawData)
    {
        var bytes = Encoding.UTF8.GetBytes(rawData);
        using var memoryStream = new MemoryStream();
        using (var gzip = new GZipStream(memoryStream, CompressionMode.Compress))
        {
            gzip.Write(bytes, 0, bytes.Length);
        }
        return Convert.ToBase64String(memoryStream.ToArray());
    }

    /// <summary>
    /// Decompresses base64 encoded gzip data to original string
    /// </summary>
    public Result<string> Decompress(string compressedData)
    {
        try
        {
            var buffer = Convert.FromBase64String(compressedData);
            using var memoryStream = new MemoryStream(buffer);
            using var gzip = new GZipStream(memoryStream, CompressionMode.Decompress);
            using var reader = new StreamReader(gzip, Encoding.UTF8);
            var decompressed = reader.ReadToEnd();
            return Result.Success(decompressed);
        }
        catch (Exception ex)
        {
            return Result.Failure<string>($"Failed to decompress troops data: {ex.Message}");
        }
    }
}
