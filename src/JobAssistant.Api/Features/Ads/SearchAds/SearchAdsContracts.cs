namespace JobAssistant.Api.Features.Ads.SearchAds;

public sealed record SearchAdItem(
    string Title,
    string Location,
    string OccupationGroup,
    string Id,
    string? WebpageUrl);

public sealed record SearchAdsResponse(List<SearchAdItem> Ads);