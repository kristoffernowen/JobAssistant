using System.Globalization;
using System.Text.Json;
using JobAssistant.Application.Common.Exceptions;
using JobAssistant.Application.Common.Interfaces;
using JobAssistant.Application.JobSearch;

namespace JobAssistant.Infrastructure.External.JobSearch;

public sealed class JobSearchClient(HttpClient httpClient) : IJobSearchClient
{
    public async Task<IReadOnlyCollection<JobSearchAdDto>> SearchAdsAsync(
        DateTime publishedAfter,
        DateTime? publishedBefore,
        IReadOnlyCollection<string> municipalityIds,
        IReadOnlyCollection<string> occupationGroupIds,
        int? maxLimit,
        CancellationToken cancellationToken)
    {
        var query = new List<string>
        {
            $"published-after={Uri.EscapeDataString(FormatDateTime(publishedAfter))}",
            "sort=pubdate-desc"
        };

        if (publishedBefore is not null)
        {
            query.Add($"published-before={Uri.EscapeDataString(FormatDateTime(publishedBefore.Value))}");
        }

        query.AddRange(municipalityIds.Select(x => $"municipality={Uri.EscapeDataString(x)}"));
        query.AddRange(occupationGroupIds.Select(x => $"occupation-group={Uri.EscapeDataString(x)}"));

        if (maxLimit is not null)
        {
            query.Add($"limit={maxLimit.Value}");
        }

        var path = $"search?{string.Join("&", query)}";

        using var response = await httpClient.GetAsync(path, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            throw new RateLimitExceededException("JobSearch rate limit exceeded.");
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new ExternalServiceException($"JobSearch returned status {(int)response.StatusCode}.");
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        var payload = JsonSerializer.Deserialize<JobSearchResponseDto>(
            content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return payload?.Hits ?? [];
    }

    private static string FormatDateTime(DateTime value)
    {
        return value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
    }
}