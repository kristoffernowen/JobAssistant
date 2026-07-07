using System.Text.Json;
using System.Text;
using JobAssistant.Application.JobSearch;

namespace JobAssistant.Api.Features.Ads.SearchAds;

internal static class SearchAdsSessionRefineHandler
{
    public static IReadOnlyCollection<JobSearchAdDto> Apply(
        IReadOnlyCollection<JobSearchAdDto> ads,
        RefineSearchAdsSessionRequest request)
    {
        var mustContainTerms = NormalizeTerms(request.MustContain);
        var mustNotContainTerms = NormalizeTerms(request.MustNotContain);

        if (mustContainTerms.Count == 0 && mustNotContainTerms.Count == 0)
        {
            return ads;
        }

        return ads
            .Where(ad => Matches(ad, mustContainTerms, mustNotContainTerms))
            .ToList();
    }

    private static bool Matches(
        JobSearchAdDto ad,
        IReadOnlyCollection<string> mustContainTerms,
        IReadOnlyCollection<string> mustNotContainTerms)
    {
        var searchableValues = BuildSearchableValues(ad);

        var containsAllRequired = mustContainTerms.All(term =>
            searchableValues.Any(value => ContainsTerm(value, term)));

        if (!containsAllRequired)
        {
            return false;
        }

        var containsBlockedTerm = mustNotContainTerms.Any(term =>
            searchableValues.Any(value => ContainsTerm(value, term)));

        return !containsBlockedTerm;
    }

    private static List<string> BuildSearchableValues(JobSearchAdDto ad)
    {
        var values = new List<string>
        {
            NormalizeText(ad.Headline),
            NormalizeText(ad.Description?.Text)
        };

        AppendJsonTextValues(values, ad.Occupation);
        AppendJsonTextValues(values, ad.MustHave);
        AppendJsonTextValues(values, ad.NiceToHave);

        return values;
    }

    private static List<string> NormalizeTerms(IEnumerable<string>? terms)
    {
        return terms?
            .Select(NormalizeText)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList()
            ?? [];
    }

    private static bool ContainsTerm(string value, string term)
    {
        return !string.IsNullOrEmpty(value)
            && value.Contains(term, StringComparison.OrdinalIgnoreCase);
    }

    private static void AppendJsonTextValues(List<string> values, JsonElement element)
    {
        if (element.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return;
        }

        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    AppendJsonTextValues(values, property.Value);
                }
                break;

            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    AppendJsonTextValues(values, item);
                }
                break;

            case JsonValueKind.String:
                values.Add(NormalizeText(element.GetString()));
                break;

            default:
                values.Add(NormalizeText(element.ToString()));
                break;
        }
    }

    private static string NormalizeText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var trimmed = value.Trim();
        var sb = new StringBuilder(trimmed.Length);
        var previousWasWhitespace = false;

        foreach (var ch in trimmed)
        {
            var isWhitespace = char.IsWhiteSpace(ch) || ch == '\u00A0';

            if (isWhitespace)
            {
                if (previousWasWhitespace)
                {
                    continue;
                }

                sb.Append(' ');
                previousWasWhitespace = true;
                continue;
            }

            sb.Append(ch);
            previousWasWhitespace = false;
        }

        return sb.ToString();
    }
}