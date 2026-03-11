namespace TWAction.Infrastructure.Options;

public sealed class GeneratorApiOptions
{
    public const string SectionName = "GeneratorApi";

    public string BaseUrl { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;
}
