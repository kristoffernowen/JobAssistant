using JobAssistant.Application.Common.Entities;
using JobAssistant.Application.Common.Exceptions;
using JobAssistant.Application.Common.Interfaces;
using JobAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobAssistant.Api.Features.Ads.LoadJobStreamAds;

public static class LoadJobStreamAdsEndpoint
{
    public static RouteGroupBuilder MapLoadJobStreamAds(this RouteGroupBuilder group)
    {
        group.MapPost(
            "/",
            async (
                LoadJobStreamAdsRequest request,
                ApplicationDbContext dbContext,
                ILocationConceptMapper locationMapper,
                IJobStreamClient jobStreamClient,
                CancellationToken cancellationToken) =>
            {
                ValidateRequest(request);

                if (!locationMapper.TryMapToConceptIds(request.Location, out var conceptIds))
                {
                    throw new ValidationException(
                        "Validation failed.",
                        new Dictionary<string, string[]>
                        {
                            ["location"] = ["Location is not supported in current mapping."]
                        });
                }

                var overlapExists = await dbContext.ImportWindows
                    .AsNoTracking()
                    .AnyAsync(
                        x => request.FromDateTime < x.ToUtc && request.ToDateTime > x.FromUtc,
                        cancellationToken);

                if (overlapExists)
                {
                    throw new ConflictException("Ads already loaded for overlapping timespan.");
                }

                var streamAds = await jobStreamClient.GetStreamAdsAsync(
                    request.FromDateTime,
                    request.ToDateTime,
                    conceptIds,
                    cancellationToken);

                var sourceIds = streamAds
                    .Select(x => x.Id)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.Ordinal)
                    .ToList();

                var existingAds = await dbContext.JobAds
                    .Where(x => x.SourceType == "JobStream" && sourceIds.Contains(x.SourceId))
                    .ToDictionaryAsync(x => x.SourceId, StringComparer.Ordinal, cancellationToken);

                var loadedUtc = DateTime.UtcNow;

                foreach (var ad in streamAds)
                {
                    if (string.IsNullOrWhiteSpace(ad.Id))
                    {
                        continue;
                    }

                    if (!existingAds.TryGetValue(ad.Id, out var jobAd))
                    {
                        jobAd = new JobAd
                        {
                            SourceType = "JobStream",
                            SourceId = ad.Id
                        };
                        dbContext.JobAds.Add(jobAd);
                        existingAds[jobAd.SourceId] = jobAd;
                    }

                    jobAd.Title = string.IsNullOrWhiteSpace(ad.Headline) ? "Untitled" : ad.Headline;
                    jobAd.Description = ad.Description?.Text ?? string.Empty;
                    jobAd.Location = ad.WorkplaceAddress?.Municipality
                        ?? ad.WorkplaceAddress?.Region
                        ?? request.Location.Trim();
                    jobAd.OccupationGroup = ad.OccupationGroup?.Label ?? "Unknown";
                    jobAd.OccupationField = ad.OccupationField?.Label ?? "Unknown";
                    jobAd.PublicationDate = ad.PublicationDate;
                    jobAd.Removed = ad.Removed;
                    jobAd.Loaded = loadedUtc;
                    jobAd.Inactive = ad.Removed;
                    jobAd.FullData = ad;
                }

                dbContext.ImportWindows.Add(new ImportWindow
                {
                    Id = Guid.NewGuid(),
                    FromUtc = request.FromDateTime,
                    ToUtc = request.ToDateTime,
                    Location = request.Location.Trim(),
                    CreatedUtc = loadedUtc
                });

                await dbContext.SaveChangesAsync(cancellationToken);

                return Results.Ok(new LoadJobStreamAdsResponse(
                    AdsAlreadyLoaded: false,
                    OperationAttempted: true,
                    Success: true));
            })
            .WithName("LoadJobStreamAds")
            .WithSummary("Load ads from JobStream and persist in local database.");

        return group;
    }

    private static void ValidateRequest(LoadJobStreamAdsRequest request)
    {
        if (request.FromDateTime >= request.ToDateTime)
        {
            throw new ValidationException(
                "Validation failed.",
                new Dictionary<string, string[]>
                {
                    ["fromDateTime"] = ["FromDateTime must be earlier than ToDateTime."]
                });
        }

        if (string.IsNullOrWhiteSpace(request.Location))
        {
            throw new ValidationException(
                "Validation failed.",
                new Dictionary<string, string[]>
                {
                    ["location"] = ["Location is required."]
                });
        }
    }
}
