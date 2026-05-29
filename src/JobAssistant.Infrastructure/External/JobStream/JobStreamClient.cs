using System.Globalization;
using System.Text.Json;
using JobAssistant.Application.Common.Interfaces;
using JobAssistant.Application.Common.Exceptions;
using JobAssistant.Application.JobStream;

namespace JobAssistant.Infrastructure.External.JobStream;

public sealed class JobStreamClient(HttpClient httpClient) : IJobStreamClient
{
    public async Task<IReadOnlyCollection<JobStreamAdDto>> GetStreamAdsAsync(
        DateTime fromUtc,
        DateTime toUtc,
        IReadOnlyCollection<string> locationConceptIds,
        CancellationToken cancellationToken)
    {
        var updatedAfter = Uri.EscapeDataString(fromUtc.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture));
        var updatedBefore = Uri.EscapeDataString(toUtc.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture));

        var query = new List<string>
        {
            $"updated-after={updatedAfter}",
            $"updated-before={updatedBefore}"
        };

        query.AddRange(locationConceptIds.Select(id => $"location-concept-id={Uri.EscapeDataString(id)}"));

        var path = $"v2/stream?{string.Join("&", query)}";
        using var response = await httpClient.GetAsync(path, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            throw new ExternalServiceException("JobStream rate limit exceeded.");
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new ExternalServiceException($"JobStream returned status {(int)response.StatusCode}.");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var ads = await JsonSerializer.DeserializeAsync<List<JobStreamAdDto>>(
            stream,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
            cancellationToken);

        return ads ?? [];
    }
}
