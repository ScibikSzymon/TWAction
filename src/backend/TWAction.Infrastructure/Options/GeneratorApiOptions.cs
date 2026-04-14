using System.ComponentModel.DataAnnotations;

namespace TWAction.Infrastructure.Options;

public sealed class GeneratorApiOptions
{
    public const string SectionName = "GeneratorApi";

    [Required]
    public string BaseUrl { get; set; } = string.Empty;

    [Required]
    public string ApiKey { get; set; } = string.Empty;
}
