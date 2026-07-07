using JobAssistant.Application.Common.Exceptions;
using JobAssistant.Application.Common.Interfaces;
using JobAssistant.Application.JobSearch;

namespace JobAssistant.Api.Features.Ads.SearchAds;

internal static class SearchAdsCommon
{
    public const int SessionHardCap = 50;

    public static List<string> NormalizeValues(IEnumerable<string>? values)
    {
        return values?
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList()
            ?? [];
    }

    public static IReadOnlyCollection<JobSearchAdDto> ApplyKeywordFilter(
        IReadOnlyCollection<JobSearchAdDto> ads,
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

    public static List<SearchAdItem> MapToSearchItems(IReadOnlyCollection<JobSearchAdDto> ads)
    {
        return ads
            .Select(x => new SearchAdItem(
                x.Headline ?? "Untitled",
                x.WorkplaceAddress?.Municipality ?? "Unknown",
                x.OccupationGroup?.Label ?? "Unknown",
                x.Id,
                x.WebpageUrl))
            .ToList();
    }

    public static void ValidateSearchRequest(
        DateTime? publishedAfter,
        DateTime? publishedBefore,
        IReadOnlyCollection<string> municipality,
        IReadOnlyCollection<string> occupationGroup,
        int? maxLimit,
        ITaxonomyConceptValidator taxonomyConceptValidator,
        int maxLimitUpperBound)
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

        if (maxLimit is <= 0 || maxLimit > maxLimitUpperBound)
        {
            errors ??= new Dictionary<string, string[]>();
            errors["maxLimit"] = [$"MaxLimit must be between 1 and {maxLimitUpperBound}."];
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