using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using JobAssistant.Api.Features.Ads.SearchAds;
using JobAssistant.Api.Tests.Testing;
using JobAssistant.Application.JobSearch;

namespace JobAssistant.Api.Tests;

public sealed class SearchAdsEndpointTests
{
    [Fact]
    public async Task SearchAds_WhenPublishedAfterMissing_ReturnsValidationProblemDetails()
    {
        var stubClient = new StubJobSearchClient();
        var taxonomyValidator = new StubTaxonomyConceptValidator();
        taxonomyValidator.MunicipalityIds.Add("Z5Cq_SgB_dsB");

        using var factory = new SearchAdsTestFactory(stubClient, taxonomyValidator);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/ads/search?municipality=Z5Cq_SgB_dsB");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal("https://jobassistant/errors/validation", payload.GetProperty("type").GetString());
        Assert.Equal("/ads/search", payload.GetProperty("instance").GetString());
        Assert.True(payload.GetProperty("errors").TryGetProperty("publishedAfter", out _));
    }

    [Fact]
    public async Task SearchAds_WhenMunicipalityAndOccupationGroupMissing_ReturnsValidationProblemDetails()
    {
        var stubClient = new StubJobSearchClient();
        var taxonomyValidator = new StubTaxonomyConceptValidator();

        using var factory = new SearchAdsTestFactory(stubClient, taxonomyValidator);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/ads/search?publishedAfter=2026-06-27T10:00:00");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal("https://jobassistant/errors/validation", payload.GetProperty("type").GetString());
        Assert.True(payload.GetProperty("errors").TryGetProperty("filters", out _));
    }

    [Fact]
    public async Task SearchAds_WhenIdsAreInvalid_ReturnsValidationProblemDetails()
    {
        var stubClient = new StubJobSearchClient();
        var taxonomyValidator = new StubTaxonomyConceptValidator();

        using var factory = new SearchAdsTestFactory(stubClient, taxonomyValidator);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/ads/search?publishedAfter=2026-06-27T10:00:00&municipality=invalid-muni&occupationGroup=invalid-occ");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        var errors = payload.GetProperty("errors");
        Assert.True(errors.TryGetProperty("municipality", out _));
        Assert.True(errors.TryGetProperty("occupationGroup", out _));
    }

    [Fact]
    public async Task SearchAds_WhenNoMatches_ReturnsOkWithEmptyList()
    {
        var stubClient = new StubJobSearchClient();
        var taxonomyValidator = new StubTaxonomyConceptValidator();
        taxonomyValidator.MunicipalityIds.Add("Z5Cq_SgB_dsB");

        using var factory = new SearchAdsTestFactory(stubClient, taxonomyValidator);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/ads/search?publishedAfter=2026-06-27T10:00:00&municipality=Z5Cq_SgB_dsB");

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<SearchAdsResponse>();

        Assert.NotNull(payload);
        Assert.Empty(payload.Ads);
    }

    [Fact]
    public async Task SearchAds_AppliesKeywordFilterOnDescriptionAndMapsFields()
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
                    Description = new JobSearchDescriptionDto { Text = "Vi söker en utvecklare med .NET erfarenhet." },
                    WorkplaceAddress = new JobSearchWorkplaceAddressDto { Municipality = "Västerås" },
                    OccupationGroup = new JobSearchOccupationGroupDto { Label = "Mjukvaru- och systemutvecklare m.fl." }
                },
                new JobSearchAdDto
                {
                    Id = "ad-2",
                    Headline = "Säljare",
                    WebpageUrl = "https://example.com/ad-2",
                    Description = new JobSearchDescriptionDto { Text = "Butikserfarenhet krävs." },
                    WorkplaceAddress = new JobSearchWorkplaceAddressDto { Municipality = "Västerås" },
                    OccupationGroup = new JobSearchOccupationGroupDto { Label = "Butikssäljare" }
                }
            ]
        };

        var taxonomyValidator = new StubTaxonomyConceptValidator();
        taxonomyValidator.MunicipalityIds.Add("8deT_FRF_2SP");

        using var factory = new SearchAdsTestFactory(stubClient, taxonomyValidator);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/ads/search?publishedAfter=2026-06-27T10:00:00&municipality=8deT_FRF_2SP&keyword=.net");

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<SearchAdsResponse>();

        Assert.NotNull(payload);
        Assert.Single(payload.Ads);

        var ad = payload.Ads[0];
        Assert.Equal("ad-1", ad.Id);
        Assert.Equal("Backend Developer", ad.Title);
        Assert.Equal("Västerås", ad.Location);
        Assert.Equal("Mjukvaru- och systemutvecklare m.fl.", ad.OccupationGroup);
        Assert.Equal("https://example.com/ad-1", ad.WebpageUrl);
    }
}