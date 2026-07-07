using JobAssistant.Application.Common.Exceptions;
using JobAssistant.Application.Common.Interfaces;
using JobAssistant.Application.JobSearch;
using Microsoft.Extensions.Caching.Memory;

namespace JobAssistant.Infrastructure.Services;

public sealed class InMemorySearchAdsSessionStore(IMemoryCache cache) : ISearchAdsSessionStore
{
    private static readonly TimeSpan SlidingExpiration = TimeSpan.FromMinutes(20);

    public SearchAdsSessionEntry CreateSession(IReadOnlyCollection<JobSearchAdDto> ads, bool wasHardCapped)
    {
        var sessionId = Guid.NewGuid().ToString("N");

        cache.Set(
            sessionId,
            new SessionPayload(ads, wasHardCapped),
            new MemoryCacheEntryOptions
            {
                SlidingExpiration = SlidingExpiration
            });

        return new SearchAdsSessionEntry(
            sessionId,
            ads,
            DateTime.UtcNow.Add(SlidingExpiration),
            wasHardCapped);
    }

    public SearchAdsSessionEntry GetSession(string sessionId)
    {
        if (!cache.TryGetValue<SessionPayload>(sessionId, out var payload) || payload is null)
        {
            throw new NotFoundException("Search session not found or expired.");
        }

        return new SearchAdsSessionEntry(
            sessionId,
            payload.Ads,
            DateTime.UtcNow.Add(SlidingExpiration),
            payload.WasHardCapped);
    }

    private sealed record SessionPayload(
        IReadOnlyCollection<JobSearchAdDto> Ads,
        bool WasHardCapped);
}