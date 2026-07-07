using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using JobAssistant.Api.Features.Ads.GetAds;
using JobAssistant.Api.Tests.Testing;
using JobAssistant.Application.Common.Entities;

namespace JobAssistant.Api.Tests;

public sealed class AdsEndpointsTests
{
    [Fact]
    public async Task GetAds_WhenNoAdsExist_ReturnsNotFoundProblemDetails()
    {
        using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/ads");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal("https://jobassistant/errors/not-found", payload.GetProperty("type").GetString());
        Assert.Equal("Resource not found", payload.GetProperty("title").GetString());
        Assert.Equal(404, payload.GetProperty("status").GetInt32());
        Assert.Equal("/ads", payload.GetProperty("instance").GetString());
    }

    [Fact]
    public async Task GetAds_ReturnsMaxTenLatestActiveAds()
    {
        using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();

        var now = DateTime.UtcNow;
        var ads = Enumerable.Range(1, 12)
            .Select(i => new JobAd
            {
                SourceType = "JobStream",
                SourceId = $"ad-{i}",
                Title = $"Title {i}",
                Description = $"Description {i}",
                Location = "Vasteras",
                OccupationGroup = "IT-utvecklare",
                OccupationField = "IT",
                PublicationDate = now.AddMinutes(i),
                Removed = false,
                Loaded = now.AddMinutes(i),
                Inactive = false
            })
            .ToList();

        ads.Add(new JobAd
        {
            SourceType = "JobStream",
            SourceId = "inactive-1",
            Title = "Inactive",
            Description = "Should not be returned",
            Location = "Vasteras",
            OccupationGroup = "IT-utvecklare",
            OccupationField = "IT",
            PublicationDate = now.AddDays(1),
            Removed = true,
            Loaded = now.AddDays(1),
            Inactive = true
        });

        await factory.SeedAdsAsync(ads.ToArray());

        var response = await client.GetAsync("/ads");

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<GetAdsResponse>();

        Assert.NotNull(payload);
        Assert.Equal(10, payload.Ads.Count);
        Assert.Equal("Title 12", payload.Ads[0].Title);
        Assert.DoesNotContain(payload.Ads, x => x.Title == "Inactive");
    }

    [Fact]
    public async Task GetAdsByFilter_WhenValidationFails_ReturnsValidationProblemDetails()
    {
        using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/ads/filter?location=&occupationField=&numberOfAds=0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal("https://jobassistant/errors/validation", payload.GetProperty("type").GetString());
        Assert.Equal("Validation failed", payload.GetProperty("title").GetString());
        Assert.Equal(400, payload.GetProperty("status").GetInt32());
        Assert.Equal("/ads/filter", payload.GetProperty("instance").GetString());
        var errors = payload.GetProperty("errors");
        Assert.True(errors.TryGetProperty("location", out _));
        Assert.True(errors.TryGetProperty("occupationField", out _));
        Assert.True(errors.TryGetProperty("numberOfAds", out _));
    }
}
