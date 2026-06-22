namespace JobAssistant.Api.Features.Ads.GetAds;

public sealed record AdItem(
    string Title, 
    string Description, 
    string Location, 
    string OccupationGroup, 
    string OccupationField);

public sealed record GetAdsResponse(List<AdItem> Ads);
