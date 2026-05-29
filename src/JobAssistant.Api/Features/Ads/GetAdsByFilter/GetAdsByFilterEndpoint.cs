using JobAssistant.Api.Features.Ads.GetAds;
using JobAssistant.Application.Common.Exceptions;
using JobAssistant.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobAssistant.Api.Features.Ads.GetAdsByFilter;

public static class GetAdsByFilterEndpoint
{
    public static RouteGroupBuilder MapGetAdsByFilter(this RouteGroupBuilder group)
    {
        group.MapGet(
            "/filter",
            async (
                [FromQuery] string? location,
                [FromQuery] string? category,
                [FromQuery] int numberOfAds,
                ApplicationDbContext dbContext,
                CancellationToken cancellationToken) =>
            {
                ValidateRequest(location, category, numberOfAds);

                var hasAnyAds = await dbContext.JobAds
                    .AsNoTracking()
                    .AnyAsync(cancellationToken);

                if (!hasAnyAds)
                {
                    throw new NotFoundException("No records exist.");
                }

                var normalizedLocation = location!.Trim();
                var normalizedCategory = category!.Trim();

                var ads = await dbContext.JobAds
                    .AsNoTracking()
                    .Where(x =>
                        !x.Inactive
                        && x.Location.ToLower() == normalizedLocation.ToLower()
                        && x.Category.ToLower() == normalizedCategory.ToLower())
                    .OrderByDescending(x => x.Loaded)
                    .Take(numberOfAds)
                    .Select(x => new AdItem(x.Title, x.Description))
                    .ToListAsync(cancellationToken);

                return Results.Ok(new GetAdsResponse(ads));
            })
            .WithName("GetAdsByFilter")
            .WithSummary("Get active ads filtered by location and category.");

        return group;
    }

    private static void ValidateRequest(string? location, string? category, int numberOfAds)
    {
        Dictionary<string, string[]>? errors = null;

        if (string.IsNullOrWhiteSpace(location))
        {
            errors ??= new Dictionary<string, string[]>();
            errors["location"] = ["Location is required."];
        }

        if (string.IsNullOrWhiteSpace(category))
        {
            errors ??= new Dictionary<string, string[]>();
            errors["category"] = ["Category is required."];
        }

        if (numberOfAds <= 0)
        {
            errors ??= new Dictionary<string, string[]>();
            errors["numberOfAds"] = ["NumberOfAds must be greater than zero."];
        }

        if (errors is not null)
        {
            throw new ValidationException("Validation failed.", errors);
        }
    }
}
