namespace JobAssistant.Api.Features.Ads.LoadJobStreamAds;

public sealed record LoadJobStreamAdsRequest(DateTime FromDateTime, DateTime ToDateTime, string Location);

public sealed record LoadJobStreamAdsResponse(bool AdsAlreadyLoaded, bool OperationAttempted, bool Success);
