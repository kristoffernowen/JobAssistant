using JobAssistant.Application.Common.Exceptions;
using JobAssistant.Application.Common.Interfaces;

namespace JobAssistant.Api.Features.Ads.SearchAds;

public static class SearchAdsSessionsEndpoint
{
    public static RouteGroupBuilder MapSearchAdsSessions(this RouteGroupBuilder group)
    {
        group.MapPost(
            "/sessions/search",
            async (
                CreateSearchAdsSessionRequest request,
                IJobSearchClient jobSearchClient,
                ITaxonomyConceptValidator taxonomyConceptValidator,
                ISearchAdsSessionStore sessionStore,
                CancellationToken cancellationToken) =>
            {
                var municipalityIds = SearchAdsCommon.NormalizeValues(request.Municipality);
                var occupationGroupIds = SearchAdsCommon.NormalizeValues(request.OccupationGroup);

                SearchAdsCommon.ValidateSearchRequest(
                    request.PublishedAfter,
                    request.PublishedBefore,
                    municipalityIds,
                    occupationGroupIds,
                    request.MaxLimit,
                    taxonomyConceptValidator,
                    maxLimitUpperBound: 100);

                var requestedLimit = request.MaxLimit ?? SearchAdsCommon.SessionHardCap;
                var queryLimit = requestedLimit >= SearchAdsCommon.SessionHardCap
                    ? SearchAdsCommon.SessionHardCap + 1
                    : requestedLimit;

                var rawAds = await jobSearchClient.SearchAdsAsync(
                    request.PublishedAfter!.Value,
                    request.PublishedBefore,
                    municipalityIds,
                    occupationGroupIds,
                    queryLimit,
                    cancellationToken);

                var filteredAds = SearchAdsCommon.ApplyKeywordFilter(rawAds, request.Keyword).ToList();

                var cappedAds = filteredAds
                    .Take(SearchAdsCommon.SessionHardCap)
                    .ToList();

                var wasHardCapped = filteredAds.Count > SearchAdsCommon.SessionHardCap;
                var session = sessionStore.CreateSession(cappedAds, wasHardCapped);
                var ads = SearchAdsCommon.MapToSearchItems(session.Ads);
                var messages = BuildMessages(session.WasHardCapped, request.MaxLimit, ads.Count);

                return Results.Ok(new SearchAdsSessionResponse(
                    session.SessionId,
                    session.ExpiresAtUtc,
                    messages,
                    ads));
            })
            .WithName("CreateSearchAdsSession")
            .WithSummary("Create an in-memory search session from AF JobSearch results.");

        group.MapPost(
            "/sessions/{sessionId}/refine",
            (
                string sessionId,
                RefineSearchAdsSessionRequest request,
                ISearchAdsSessionStore sessionStore) =>
            {
                if (string.IsNullOrWhiteSpace(sessionId))
                {
                    throw new ValidationException(
                        "Validation failed.",
                        new Dictionary<string, string[]>
                        {
                            ["sessionId"] = ["SessionId is required."]
                        });
                }

                var session = sessionStore.GetSession(sessionId);
                var refinedAds = SearchAdsSessionRefineHandler.Apply(session.Ads, request)
                    .Take(SearchAdsCommon.SessionHardCap)
                    .ToList();

                var wasHardCappedInRefine = session.WasHardCapped
                    && refinedAds.Count == SearchAdsCommon.SessionHardCap;
                var ads = SearchAdsCommon.MapToSearchItems(refinedAds);
                var messages = BuildMessages(wasHardCappedInRefine, requestedMaxLimit: null, ads.Count);

                return Results.Ok(new SearchAdsSessionResponse(
                    session.SessionId,
                    session.ExpiresAtUtc,
                    messages,
                    ads));
            })
            .WithName("RefineSearchAdsSession")
            .WithSummary("Refine a previously created in-memory search session.");

        return group;
    }

    private static List<string> BuildMessages(bool wasHardCapped, int? requestedMaxLimit, int hitCount)
    {
        var messages = new List<string>
        {
            $"Träffar: {hitCount} annonser."
        };

        if (wasHardCapped)
        {
            messages.Add("Maxgräns 50 annonser är aktiv. Fler träffar fanns men klipptes bort i sessionen.");
        }

        if (requestedMaxLimit is > SearchAdsCommon.SessionHardCap)
        {
            messages.Add("MaxLimit i request var högre än 50. API:t använder max 50 för sessionsflödet.");
        }

        return messages;
    }
}