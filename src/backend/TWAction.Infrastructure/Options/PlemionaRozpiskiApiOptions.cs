namespace TWAction.Infrastructure.Options;

public sealed class PlemionaRozpiskiApiOptions
{
    public const string SectionName = "PlemionaRozpiskiApi";

    public string BaseUrl { get; set; } = "https://plemionarozpiski.pl";

    public string ApiKey { get; set; } = string.Empty;
}
