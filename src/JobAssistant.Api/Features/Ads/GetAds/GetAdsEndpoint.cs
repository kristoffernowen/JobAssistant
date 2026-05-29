using JobAssistant.Application.Common.Exceptions;
using JobAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobAssistant.Api.Features.Ads.GetAds;

public static class GetAdsEndpoint
{
    public static RouteGroupBuilder MapGetAds(this RouteGroupBuilder group)
    {
        group.MapGet(
            "/",
            async (ApplicationDbContext dbContext, CancellationToken cancellationToken) =>
            {
                var hasAnyAds = await dbContext.JobAds
                    .AsNoTracking()
                    .AnyAsync(cancellationToken);

                if (!hasAnyAds)
                {
                    throw new NotFoundException("No records exist.");
                }

                var ads = await dbContext.JobAds
                    .AsNoTracking()
                    .Where(x => !x.Inactive)
                    .OrderByDescending(x => x.Loaded)
                    .Take(10)
                    .Select(x => new AdItem(x.Title, x.Description))
                    .ToListAsync(cancellationToken);

                return Results.Ok(new GetAdsResponse(ads));
            })
            .WithName("GetAds")
            .WithSummary("Get up to 10 latest active ads.");

        return group;
    }
}
