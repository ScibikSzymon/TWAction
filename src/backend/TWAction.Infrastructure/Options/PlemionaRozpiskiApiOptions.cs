using System.ComponentModel.DataAnnotations;

namespace TWAction.Infrastructure.Options;

public sealed class PlemionaRozpiskiApiOptions
{
    public const string SectionName = "PlemionaRozpiskiApi";

    [Required]
    public string BaseUrl { get; set; } = "https://plemionarozpiski.pl";

    [Required]
    public string ApiKey { get; set; } = string.Empty;
}
