using JobAssistant.Application.JobStream;

namespace JobAssistant.Application.Common.Entities;

public sealed class JobAd
{
    public string SourceType { get; set; } = "JobStream";

    public string SourceId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string OccupationGroup { get; set; } = string.Empty;

    public string OccupationField { get; set; } = string.Empty;

    public DateTime PublicationDate { get; set; }

    public bool Removed { get; set; }

    public DateTime Loaded { get; set; }

    public bool Inactive { get; set; }

    public JobStreamAdDto? FullData { get; set; }
}
