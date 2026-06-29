namespace JobAssistant.Api.Features.Ads.SearchAds;

public sealed record CreateSearchAdsSessionRequest(
    DateTime? PublishedAfter,
    DateTime? PublishedBefore,
    List<string>? Municipality,
    List<string>? OccupationGroup,
    string? Keyword,
    int? MaxLimit);

public sealed record RefineSearchAdsSessionRequest(
    string? Keyword,
    int? MaxLimit);

public sealed record SearchAdsSessionResponse(
    string SessionId,
    DateTime ExpiresAtUtc,
    List<SearchAdItem> Ads,
    List<string> Messages);