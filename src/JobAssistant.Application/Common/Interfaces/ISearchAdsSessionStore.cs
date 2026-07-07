using JobAssistant.Application.JobSearch;

namespace JobAssistant.Application.Common.Interfaces;

public interface ISearchAdsSessionStore
{
    SearchAdsSessionEntry CreateSession(IReadOnlyCollection<JobSearchAdDto> ads, bool wasHardCapped);

    SearchAdsSessionEntry GetSession(string sessionId);
}