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
        Assert.Contains(payload.Messages, x => x.Contains("Traffar: 1 annonser.", StringComparison.OrdinalIgnoreCase));
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
        Assert.Contains(payload.Messages, x => x.Contains("Traffar: 50 annonser.", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(payload.Messages, x => x.Contains("Maxgräns 50 annonser", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task RefineSearchSession_AppliesMustContainAndMustNotContainOnSessionData()
    {
        var occupation = JsonSerializer.Deserialize<JsonElement>("{\"label\":\"Backendutvecklare\",\"concept_id\":\"abc\"}");
        var mustHave = JsonSerializer.Deserialize<JsonElement>("[{\"label\":\"CSharp\"},{\"label\":\"SQL\"}]");
        var niceToHave = JsonSerializer.Deserialize<JsonElement>("[{\"label\":\"Azure\"}]");

        var stubClient = new StubJobSearchClient
        {
            Response =
            [
                new JobSearchAdDto
                {
                    Id = "ad-1",
                    Headline = "Backend Developer",
                    Description = new JobSearchDescriptionDto { Text = "CSharp och SQL i backend" },
                    WorkplaceAddress = new JobSearchWorkplaceAddressDto { Municipality = "Västerås" },
                    OccupationGroup = new JobSearchOccupationGroupDto { Label = "Utvecklare" },
                    Occupation = occupation,
                    MustHave = mustHave,
                    NiceToHave = niceToHave
                },
                new JobSearchAdDto
                {
                    Id = "ad-2",
                    Headline = "Säljare",
                    Description = new JobSearchDescriptionDto { Text = "butik med mycket kundkontakt" },
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
            MustContain: [" csharp ", "sql"],
            MustNotContain: ["kundkontakt"]);

        var refineResponse = await client.PostAsJsonAsync($"/ads/sessions/{createPayload.SessionId}/refine", refineRequest);

        refineResponse.EnsureSuccessStatusCode();
        var refinePayload = await refineResponse.Content.ReadFromJsonAsync<SearchAdsSessionResponse>();

        Assert.NotNull(refinePayload);
        Assert.Single(refinePayload.Ads);
        Assert.Contains(refinePayload.Messages, x => x.Contains("Traffar: 1 annonser.", StringComparison.OrdinalIgnoreCase));
        Assert.Equal("ad-1", refinePayload.Ads[0].Id);
    }

    [Fact]
    public async Task RefineSearchSession_EmptyTermsOrLists_DoNotAddFiltering()
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
            MustContain: ["   ", ""],
            MustNotContain: []);

        var refineResponse = await client.PostAsJsonAsync($"/ads/sessions/{createPayload.SessionId}/refine", refineRequest);
        refineResponse.EnsureSuccessStatusCode();

        var refinePayload = await refineResponse.Content.ReadFromJsonAsync<SearchAdsSessionResponse>();

        Assert.NotNull(refinePayload);
        Assert.Equal(2, refinePayload.Ads.Count);
        Assert.Contains(refinePayload.Messages, x => x.Contains("Traffar: 2 annonser.", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task RefineSearchSession_MustNotContain_MatchesMustHaveWithNormalizedWhitespace()
    {
        var mustHaveWithNbsp = JsonSerializer.Deserialize<JsonElement>("[{\"label\":\"Mycket god förståelse och erfarenhet av inbyggda system (minst 5\\u00A0år).\"}]");

        var stubClient = new StubJobSearchClient
        {
            Response =
            [
                new JobSearchAdDto
                {
                    Id = "ad-1",
                    Headline = "Embedded Developer",
                    Description = new JobSearchDescriptionDto { Text = "Beskrivning" },
                    WorkplaceAddress = new JobSearchWorkplaceAddressDto { Municipality = "Västerås" },
                    OccupationGroup = new JobSearchOccupationGroupDto { Label = "Utvecklare" },
                    MustHave = mustHaveWithNbsp
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
            MustContain: null,
            MustNotContain: ["5 år"]);

        var refineResponse = await client.PostAsJsonAsync($"/ads/sessions/{createPayload.SessionId}/refine", refineRequest);
        refineResponse.EnsureSuccessStatusCode();

        var refinePayload = await refineResponse.Content.ReadFromJsonAsync<SearchAdsSessionResponse>();

        Assert.NotNull(refinePayload);
        Assert.Empty(refinePayload.Ads);
        Assert.Contains(refinePayload.Messages, x => x.Contains("Traffar: 0 annonser.", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task RefineSearchSession_WhenRefinedResultUnderFifty_DoesNotReturnCapMessage()
    {
        var stubClient = new StubJobSearchClient
        {
            Response = Enumerable.Range(1, 51)
                .Select(i => new JobSearchAdDto
                {
                    Id = $"ad-{i}",
                    Headline = i <= 4 ? $"Backend {i}" : $"Other {i}",
                    Description = new JobSearchDescriptionDto { Text = i <= 4 ? "dotnet" : "other" },
                    WorkplaceAddress = new JobSearchWorkplaceAddressDto { Municipality = "Västerås" },
                    OccupationGroup = new JobSearchOccupationGroupDto { Label = "Utvecklare" }
                })
                .ToList()
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
            MaxLimit: 100);

        var createResponse = await client.PostAsJsonAsync("/ads/sessions/search", createRequest);
        createResponse.EnsureSuccessStatusCode();

        var createPayload = await createResponse.Content.ReadFromJsonAsync<SearchAdsSessionResponse>();
        Assert.NotNull(createPayload);
        Assert.Contains(createPayload.Messages, x => x.Contains("Maxgräns 50 annonser", StringComparison.OrdinalIgnoreCase));

        var refineRequest = new RefineSearchAdsSessionRequest(
            MustContain: ["dotnet"],
            MustNotContain: null);

        var refineResponse = await client.PostAsJsonAsync($"/ads/sessions/{createPayload.SessionId}/refine", refineRequest);
        refineResponse.EnsureSuccessStatusCode();

        var refinePayload = await refineResponse.Content.ReadFromJsonAsync<SearchAdsSessionResponse>();

        Assert.NotNull(refinePayload);
        Assert.Equal(4, refinePayload.Ads.Count);
        Assert.Contains(refinePayload.Messages, x => x.Contains("Traffar: 4 annonser.", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(refinePayload.Messages, x => x.Contains("Maxgräns 50 annonser", StringComparison.OrdinalIgnoreCase));
    }
}