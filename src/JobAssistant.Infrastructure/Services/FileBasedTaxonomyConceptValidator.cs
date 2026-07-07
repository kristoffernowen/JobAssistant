using System.Text.Json;
using JobAssistant.Application.Common.Interfaces;
using Microsoft.Extensions.Hosting;

namespace JobAssistant.Infrastructure.Services;

public sealed class FileBasedTaxonomyConceptValidator : ITaxonomyConceptValidator
{
    private readonly HashSet<string> _municipalityIds;
    private readonly HashSet<string> _occupationGroupIds;

    public FileBasedTaxonomyConceptValidator(IHostEnvironment hostEnvironment)
    {
        var taxonomyDirectory = Path.Combine(hostEnvironment.ContentRootPath, "Data", "Taxonomy");

        _municipalityIds = LoadIdsFromLatestFile(taxonomyDirectory, "municipality-list-*.json", "municipality");
        _occupationGroupIds = LoadIdsFromLatestFile(taxonomyDirectory, "ssyk-level-4-list-*.json", "ssyk-level-4");
    }

    public bool IsValidMunicipalityId(string conceptId)
    {
        return _municipalityIds.Contains(conceptId);
    }

    public bool IsValidOccupationGroupId(string conceptId)
    {
        return _occupationGroupIds.Contains(conceptId);
    }

    private static HashSet<string> LoadIdsFromLatestFile(string directoryPath, string filePattern, string expectedType)
    {
        if (!Directory.Exists(directoryPath))
        {
            throw new InvalidOperationException($"Taxonomy directory was not found: {directoryPath}");
        }

        var filePath = Directory
            .GetFiles(directoryPath, filePattern, SearchOption.TopDirectoryOnly)
            .OrderByDescending(Path.GetFileName, StringComparer.Ordinal)
            .FirstOrDefault();

        if (filePath is null)
        {
            throw new InvalidOperationException($"No taxonomy files matched pattern '{filePattern}' in '{directoryPath}'.");
        }

        using var stream = File.OpenRead(filePath);
        using var document = JsonDocument.Parse(stream);

        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException($"Taxonomy file '{filePath}' does not contain a JSON array.");
        }

        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var element in document.RootElement.EnumerateArray())
        {
            if (!element.TryGetProperty("taxonomy/type", out var typeNode)
                || !string.Equals(typeNode.GetString(), expectedType, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!element.TryGetProperty("taxonomy/id", out var idNode))
            {
                continue;
            }

            var conceptId = idNode.GetString();
            if (!string.IsNullOrWhiteSpace(conceptId))
            {
                ids.Add(conceptId);
            }
        }

        return ids;
    }
}