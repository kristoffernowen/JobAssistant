using JobAssistant.Application.Common.Exceptions;
using JobAssistant.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JobAssistant.Api.Features.Ads.SearchAds;

public static class SearchAdsEndpoint
{
    public static RouteGroupBuilder MapSearchAds(this RouteGroupBuilder group)
    {
        group.MapGet(
            "/search",
            async (
                [FromQuery] DateTime? publishedAfter,
                [FromQuery] DateTime? publishedBefore,
                [FromQuery] string[]? municipality,
                [FromQuery] string[]? occupationGroup,
                [FromQuery] string? keyword,
                [FromQuery] int? maxLimit,
                IJobSearchClient jobSearchClient,
                ITaxonomyConceptValidator taxonomyConceptValidator,
                CancellationToken cancellationToken) =>
            {
                var municipalityIds = NormalizeValues(municipality);
                var occupationGroupIds = NormalizeValues(occupationGroup);

                ValidateRequest(
                    publishedAfter,
                    publishedBefore,
                    municipalityIds,
                    occupationGroupIds,
                    maxLimit,
                    taxonomyConceptValidator);

                var ads = await jobSearchClient.SearchAdsAsync(
                    publishedAfter!.Value,
                    publishedBefore,
                    municipalityIds,
                    occupationGroupIds,
                    maxLimit,
                    cancellationToken);

                var filteredAds = ApplyKeywordFilter(ads, keyword);

                var responseItems = filteredAds
                    .Select(x => new SearchAdItem(
                        x.Headline ?? "Untitled",
                        x.WorkplaceAddress?.Municipality ?? "Unknown",
                        x.OccupationGroup?.Label ?? "Unknown",
                        x.Id,
                        x.WebpageUrl))
                    .ToList();

                return Results.Ok(new SearchAdsResponse(responseItems));
            })
            .WithName("SearchAds")
            .WithSummary("Search ads from AF JobSearch and apply internal filters.");

        return group;
    }

    private static List<string> NormalizeValues(string[]? values)
    {
        return values?
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList()
            ?? [];
    }

    private static IReadOnlyCollection<JobAssistant.Application.JobSearch.JobSearchAdDto> ApplyKeywordFilter(
        IReadOnlyCollection<JobAssistant.Application.JobSearch.JobSearchAdDto> ads,
        string? keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return ads;
        }

        var searchTerm = keyword.Trim();

        return ads
            .Where(x => x.Description?.Text?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true)
            .ToList();
    }

    private static void ValidateRequest(
        DateTime? publishedAfter,
        DateTime? publishedBefore,
        IReadOnlyCollection<string> municipality,
        IReadOnlyCollection<string> occupationGroup,
        int? maxLimit,
        ITaxonomyConceptValidator taxonomyConceptValidator)
    {
        Dictionary<string, string[]>? errors = null;

        if (publishedAfter is null)
        {
            errors ??= new Dictionary<string, string[]>();
            errors["publishedAfter"] = ["PublishedAfter is required."];
        }

        if (publishedAfter is not null && publishedBefore is not null && publishedAfter >= publishedBefore)
        {
            errors ??= new Dictionary<string, string[]>();
            errors["publishedBefore"] = ["PublishedBefore must be later than PublishedAfter."];
        }

        if (municipality.Count == 0 && occupationGroup.Count == 0)
        {
            errors ??= new Dictionary<string, string[]>();
            errors["filters"] = ["At least one of municipality or occupationGroup is required."];
        }

        if (maxLimit is <= 0 or > 100)
        {
            errors ??= new Dictionary<string, string[]>();
            errors["maxLimit"] = ["MaxLimit must be between 1 and 100."];
        }

        var invalidMunicipalityIds = municipality
            .Where(x => !taxonomyConceptValidator.IsValidMunicipalityId(x))
            .ToList();

        if (invalidMunicipalityIds.Count > 0)
        {
            errors ??= new Dictionary<string, string[]>();
            errors["municipality"] = [$"Unknown municipality concept ids: {string.Join(", ", invalidMunicipalityIds)}"];
        }

        var invalidOccupationGroupIds = occupationGroup
            .Where(x => !taxonomyConceptValidator.IsValidOccupationGroupId(x))
            .ToList();

        if (invalidOccupationGroupIds.Count > 0)
        {
            errors ??= new Dictionary<string, string[]>();
            errors["occupationGroup"] = [$"Unknown occupationGroup concept ids: {string.Join(", ", invalidOccupationGroupIds)}"];
        }

        if (errors is not null)
        {
            throw new ValidationException("Validation failed.", errors);
        }
    }
}