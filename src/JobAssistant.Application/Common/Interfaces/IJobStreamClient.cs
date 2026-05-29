using JobAssistant.Application.JobStream;

namespace JobAssistant.Application.Common.Interfaces;

public interface IJobStreamClient
{
    Task<IReadOnlyCollection<JobStreamAdDto>> GetStreamAdsAsync(
        DateTime fromUtc,
        DateTime toUtc,
        IReadOnlyCollection<string> locationConceptIds,
        CancellationToken cancellationToken);
}
