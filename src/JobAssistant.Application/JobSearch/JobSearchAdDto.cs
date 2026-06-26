using System.Text.Json.Serialization;

namespace JobAssistant.Application.JobSearch;

public sealed class JobSearchResponseDto
{
    [JsonPropertyName("hits")]
    public List<JobSearchAdDto>? Hits { get; set; }
}

public sealed class JobSearchAdDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("headline")]
    public string? Headline { get; set; }

    [JsonPropertyName("webpage_url")]
    public string? WebpageUrl { get; set; }

    [JsonPropertyName("description")]
    public JobSearchDescriptionDto? Description { get; set; }

    [JsonPropertyName("workplace_address")]
    public JobSearchWorkplaceAddressDto? WorkplaceAddress { get; set; }

    [JsonPropertyName("occupation_group")]
    public JobSearchOccupationGroupDto? OccupationGroup { get; set; }
}

public sealed class JobSearchDescriptionDto
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

public sealed class JobSearchWorkplaceAddressDto
{
    [JsonPropertyName("municipality")]
    public string? Municipality { get; set; }
}

public sealed class JobSearchOccupationGroupDto
{
    [JsonPropertyName("label")]
    public string? Label { get; set; }
}