namespace JobAssistant.Application.Common.Entities;

public sealed class JobAd
{
    public string SourceType { get; set; } = "JobStream";

    public string SourceId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public DateTime Loaded { get; set; }

    public bool Inactive { get; set; }
}
