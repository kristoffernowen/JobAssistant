using System.Text.Json.Serialization;

namespace JobAssistant.Application.JobStream;

public sealed class JobStreamAdDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("headline")]
    public string? Headline { get; set; }

    [JsonPropertyName("description")]
    public JobStreamDescriptionDto? Description { get; set; }

    [JsonPropertyName("workplace_address")]
    public JobStreamWorkplaceAddressDto? WorkplaceAddress { get; set; }

    [JsonPropertyName("occupation_group")]
    public JobStreamOccupationGroupDto? OccupationGroup { get; set; }

    [JsonPropertyName("occupation_field")]
    public JobStreamOccupationFieldDto? OccupationField { get; set; }

    [JsonPropertyName("publication_date")]
    public DateTime PublicationDate { get; set; }

    [JsonPropertyName("removed")]
    public bool Removed { get; set; }
}

public sealed class JobStreamDescriptionDto
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("text_formatted")]
    public string? TextFormatted { get; set; }
}

public sealed class JobStreamWorkplaceAddressDto
{
    [JsonPropertyName("municipality")]
    public string? Municipality { get; set; }

    [JsonPropertyName("region")]
    public string? Region { get; set; }
}

public sealed class JobStreamOccupationFieldDto
{
    [JsonPropertyName("label")]
    public string? Label { get; set; }
}

public sealed class JobStreamOccupationGroupDto
{
    [JsonPropertyName("label")]
    public string? Label { get; set; }
}
