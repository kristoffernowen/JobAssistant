namespace JobAssistant.Api.Features.Ads.GetAds;

public sealed record AdItem(string Title, string Description);

public sealed record GetAdsResponse(List<AdItem> Ads);
