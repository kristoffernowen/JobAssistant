namespace JobAssistant.Application.JobSearch;

public sealed record SearchAdsSessionEntry(
    string SessionId,
    IReadOnlyCollection<JobSearchAdDto> Ads,
    DateTime ExpiresAtUtc,
    bool WasHardCapped);