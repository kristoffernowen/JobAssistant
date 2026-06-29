using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using JobAssistant.Api.Features.Ads.SearchAds;
using JobAssistant.Api.Tests.Testing;
using JobAssistant.Application.JobSearch;

namespace JobAssistant.Api.Tests;

public sealed class SearchAdsSessionsEndpointTests
{
    [Fact]
    public async Task CreateSearchSession_WhenPublishedAfterMissing_ReturnsValidationProblemDetails()
    {
        var stubClient = new StubJobSearchClient();
        var taxonomyValidator = new StubTaxonomyConceptValidator();
        taxonomyValidator.MunicipalityIds.Add("8deT_FRF_2SP");

        using var factory = new SearchAdsTestFactory(stubClient, taxonomyValidator);
        using var client = factory.CreateClient();

        var request = new CreateSearchAdsSessionRequest(
            PublishedAfter: null,
            PublishedBefore: null,
            Municipality: ["8deT_FRF_2SP"],
            OccupationGroup: null,
            Keyword: null,
            MaxLimit: null);

        var response = await client.PostAsJsonAsync("/ads/sessions/search", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(payload.GetProperty("errors").TryGetProperty("publishedAfter", out _));
    }

    [Fact]
    public async Task CreateSearchSession_ReturnsSessionIdAndResults()
    {
        var stubClient = new StubJobSearchClient
        {
            Response =
            [
                new JobSearchAdDto
                {
                    Id = "ad-1",
                    Headline = "Backend Developer",
                    WebpageUrl = "https://example.com/ad-1",
                    Description = new JobSearchDescriptionDto { Text = "dotnet" },
                    WorkplaceAddress = new JobSearchWorkplaceAddressDto { Municipality = "Västerås" },
                    OccupationGroup = new JobSearchOccupationGroupDto { Label = "Utvecklare" }
                }
            ]
        };

        var taxonomyValidator = new StubTaxonomyConceptValidator();
        taxonomyValidator.MunicipalityIds.Add("8deT_FRF_2SP");

        using var factory = new SearchAdsTestFactory(stubClient, taxonomyValidator);
        using var client = factory.CreateClient();

        var request = new CreateSearchAdsSessionRequest(
            PublishedAfter: new DateTime(2026, 6, 29, 10, 0, 0, DateTimeKind.Utc),
            PublishedBefore: null,
            Municipality: ["8deT_FRF_2SP"],
            OccupationGroup: null,
            Keyword: null,
            MaxLimit: null);

        var response = await client.PostAsJsonAsync("/ads/sessions/search", request);

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<SearchAdsSessionResponse>();

        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload.SessionId));
        Assert.Single(payload.Ads);
        Assert.Empty(payload.Messages);
    }

    [Fact]
    public async Task CreateSearchSession_WhenMoreThanFiftyResults_ReturnsCapMessage()
    {
        var stubClient = new StubJobSearchClient
        {
            Response = Enumerable.Range(1, 51)
                .Select(i => new JobSearchAdDto
                {
                    Id = $"ad-{i}",
                    Headline = $"Ad {i}",
                    Description = new JobSearchDescriptionDto { Text = "text" },
                    WorkplaceAddress = new JobSearchWorkplaceAddressDto { Municipality = "Västerås" },
                    OccupationGroup = new JobSearchOccupationGroupDto { Label = "Utvecklare" }
                })
                .ToList()
        };

        var taxonomyValidator = new StubTaxonomyConceptValidator();
        taxonomyValidator.MunicipalityIds.Add("8deT_FRF_2SP");

        using var factory = new SearchAdsTestFactory(stubClient, taxonomyValidator);
        using var client = factory.CreateClient();

        var request = new CreateSearchAdsSessionRequest(
            PublishedAfter: new DateTime(2026, 6, 29, 10, 0, 0, DateTimeKind.Utc),
            PublishedBefore: null,
            Municipality: ["8deT_FRF_2SP"],
            OccupationGroup: null,
            Keyword: null,
            MaxLimit: 100);

        var response = await client.PostAsJsonAsync("/ads/sessions/search", request);

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<SearchAdsSessionResponse>();

        Assert.NotNull(payload);
        Assert.Equal(50, payload.Ads.Count);
        Assert.Contains(payload.Messages, x => x.Contains("Maxgräns 50 annonser", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task RefineSearchSession_AppliesKeywordOnSessionData()
    {
        var stubClient = new StubJobSearchClient
        {
            Response =
            [
                new JobSearchAdDto
                {
                    Id = "ad-1",
                    Headline = "Backend Developer",
                    Description = new JobSearchDescriptionDto { Text = "dotnet" },
                    WorkplaceAddress = new JobSearchWorkplaceAddressDto { Municipality = "Västerås" },
                    OccupationGroup = new JobSearchOccupationGroupDto { Label = "Utvecklare" }
                },
                new JobSearchAdDto
                {
                    Id = "ad-2",
                    Headline = "Säljare",
                    Description = new JobSearchDescriptionDto { Text = "butik" },
                    WorkplaceAddress = new JobSearchWorkplaceAddressDto { Municipality = "Västerås" },
                    OccupationGroup = new JobSearchOccupationGroupDto { Label = "Sälj" }
                }
            ]
        };

        var taxonomyValidator = new StubTaxonomyConceptValidator();
        taxonomyValidator.MunicipalityIds.Add("8deT_FRF_2SP");

        using var factory = new SearchAdsTestFactory(stubClient, taxonomyValidator);
        using var client = factory.CreateClient();

        var createRequest = new CreateSearchAdsSessionRequest(
            PublishedAfter: new DateTime(2026, 6, 29, 10, 0, 0, DateTimeKind.Utc),
            PublishedBefore: null,
            Municipality: ["8deT_FRF_2SP"],
            OccupationGroup: null,
            Keyword: null,
            MaxLimit: null);

        var createResponse = await client.PostAsJsonAsync("/ads/sessions/search", createRequest);
        createResponse.EnsureSuccessStatusCode();

        var createPayload = await createResponse.Content.ReadFromJsonAsync<SearchAdsSessionResponse>();
        Assert.NotNull(createPayload);

        var refineRequest = new RefineSearchAdsSessionRequest(
            Keyword: "dotnet",
            MaxLimit: null);

        var refineResponse = await client.PostAsJsonAsync($"/ads/sessions/{createPayload.SessionId}/refine", refineRequest);

        refineResponse.EnsureSuccessStatusCode();
        var refinePayload = await refineResponse.Content.ReadFromJsonAsync<SearchAdsSessionResponse>();

        Assert.NotNull(refinePayload);
        Assert.Single(refinePayload.Ads);
        Assert.Equal("ad-1", refinePayload.Ads[0].Id);
    }
}