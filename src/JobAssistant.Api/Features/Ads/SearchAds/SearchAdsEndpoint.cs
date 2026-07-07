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
                var municipalityIds = SearchAdsCommon.NormalizeValues(municipality);
                var occupationGroupIds = SearchAdsCommon.NormalizeValues(occupationGroup);

                SearchAdsCommon.ValidateSearchRequest(
                    publishedAfter,
                    publishedBefore,
                    municipalityIds,
                    occupationGroupIds,
                    maxLimit,
                    taxonomyConceptValidator,
                    maxLimitUpperBound: 100);

                var ads = await jobSearchClient.SearchAdsAsync(
                    publishedAfter!.Value,
                    publishedBefore,
                    municipalityIds,
                    occupationGroupIds,
                    maxLimit,
                    cancellationToken);

                var filteredAds = SearchAdsCommon.ApplyKeywordFilter(ads, keyword);
                var responseItems = SearchAdsCommon.MapToSearchItems(filteredAds);

                return Results.Ok(new SearchAdsResponse(responseItems));
            })
            .WithName("SearchAds")
            .WithSummary("Search ads from AF JobSearch and apply internal filters.");

        return group;
    }
}