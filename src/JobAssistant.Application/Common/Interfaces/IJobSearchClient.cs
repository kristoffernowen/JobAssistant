using JobAssistant.Application.JobSearch;

namespace JobAssistant.Application.Common.Interfaces;

public interface IJobSearchClient
{
    Task<IReadOnlyCollection<JobSearchAdDto>> SearchAdsAsync(
        DateTime publishedAfter,
        DateTime? publishedBefore,
        IReadOnlyCollection<string> municipalityIds,
        IReadOnlyCollection<string> occupationGroupIds,
        int? maxLimit,
        CancellationToken cancellationToken);
}