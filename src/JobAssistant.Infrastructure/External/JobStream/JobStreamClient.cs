using System.Globalization;
using System.Text.Json;
using JobAssistant.Application.Common.Interfaces;
using JobAssistant.Application.Common.Exceptions;
using JobAssistant.Application.JobStream;

namespace JobAssistant.Infrastructure.External.JobStream;

public sealed class JobStreamClient(HttpClient httpClient) : IJobStreamClient
{
    private const string LogDirectory = "Logs";

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
            throw new RateLimitExceededException("JobStream rate limit exceeded.");
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new ExternalServiceException($"JobStream returned status {(int)response.StatusCode}.");
        }

        var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);

        await LogRawJobStreamResponse(jsonContent);

        var ads = JsonSerializer.Deserialize<List<JobStreamAdDto>>(
            jsonContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return ads ?? [];
    }

    private static async Task LogRawJobStreamResponse(string jsonContent)
    {
        try
        {
            var baseDirectory = AppContext.BaseDirectory;
            var projectRoot = Directory.GetParent(baseDirectory)?.Parent?.Parent?.Parent?.FullName;

            if (projectRoot == null)
            {
                return;
            }

            var logDirectory = Path.Combine(projectRoot, LogDirectory);

            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            using var doc = JsonDocument.Parse(jsonContent);
            var root = doc.RootElement;

            var firstThree = root.EnumerateArray().Take(3).ToList();

            var logContent = JsonSerializer.Serialize(firstThree, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });

            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
            var logFilePath = Path.Combine(logDirectory, $"jobstream_raw_{timestamp}.json");

            await File.WriteAllTextAsync(logFilePath, logContent);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to log JobStream response: {ex.Message}");
        }
    }
}
