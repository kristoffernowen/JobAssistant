using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using JobAssistant.Api.Features.Ads.LoadJobStreamAds;
using JobAssistant.Application.Common.Entities;
using JobAssistant.Application.JobStream;
using JobAssistant.IntegrationTests.Testing;

namespace JobAssistant.IntegrationTests;

public sealed class LoadJobStreamAdsIntegrationTests
{
    [Fact]
    public async Task LoadJobStreamAds_WhenTimeSpanOverlaps_ReturnsConflictProblemDetails()
    {
        var stubClient = new StubJobStreamClient();
        using var factory = new LoadAdsTestFactory(stubClient);
        using var client = factory.CreateClient();

        await factory.SeedImportWindowAsync(new ImportWindow
        {
            Id = Guid.NewGuid(),
            FromUtc = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc),
            ToUtc = new DateTime(2026, 5, 2, 0, 0, 0, DateTimeKind.Utc),
            Location = "Vasteras",
            CreatedUtc = DateTime.UtcNow
        });

        var request = new LoadJobStreamAdsRequest(
            new DateTime(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 5, 3, 0, 0, 0, DateTimeKind.Utc),
            "Vasteras");

        var response = await client.PostAsJsonAsync("/jobstream-ads", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("https://jobassistant/errors/conflict", payload.GetProperty("type").GetString());
        Assert.Equal("/jobstream-ads", payload.GetProperty("instance").GetString());
    }

    [Fact]
    public async Task LoadJobStreamAds_UpsertsAdsAndSetsInactiveFromRemovedFlag()
    {
        var stubClient = new StubJobStreamClient();
        stubClient.EnqueueResponse(new JobStreamAdDto
        {
            Id = "ad-1",
            Headline = "Old title",
            Description = new JobStreamDescriptionDto { Text = "Old description" },
            WorkplaceAddress = new JobStreamWorkplaceAddressDto
            {
                Municipality = "Vasteras",
                Region = "Vastmanland"
            },
            OccupationField = new JobStreamOccupationFieldDto { Label = "IT" },
            Removed = false
        });
        stubClient.EnqueueResponse(new JobStreamAdDto
        {
            Id = "ad-1",
            Headline = "Updated title",
            Description = new JobStreamDescriptionDto { Text = "Updated description" },
            WorkplaceAddress = new JobStreamWorkplaceAddressDto
            {
                Municipality = "Vasteras",
                Region = "Vastmanland"
            },
            OccupationField = new JobStreamOccupationFieldDto { Label = "IT" },
            Removed = true
        });

        using var factory = new LoadAdsTestFactory(stubClient);
        using var client = factory.CreateClient();

        var firstRequest = new LoadJobStreamAdsRequest(
            new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 5, 10, 1, 0, 0, DateTimeKind.Utc),
            "Vasteras");

        var firstResponse = await client.PostAsJsonAsync("/jobstream-ads", firstRequest);
        firstResponse.EnsureSuccessStatusCode();

        var secondRequest = new LoadJobStreamAdsRequest(
            new DateTime(2026, 5, 10, 2, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 5, 10, 3, 0, 0, DateTimeKind.Utc),
            "Vasteras");

        var secondResponse = await client.PostAsJsonAsync("/jobstream-ads", secondRequest);
        secondResponse.EnsureSuccessStatusCode();

        var ad = await factory.GetJobAdAsync("ad-1");

        Assert.NotNull(ad);
        Assert.Equal("Updated title", ad.Title);
        Assert.Equal("Updated description", ad.Description);
        Assert.True(ad.Inactive);
    }
}
