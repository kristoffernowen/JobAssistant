using JobAssistant.Application.Common.Interfaces;
using JobAssistant.Application.JobSearch;
using JobAssistant.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JobAssistant.Api.Tests.Testing;

internal sealed class StubJobSearchClient : IJobSearchClient
{
    public IReadOnlyCollection<JobSearchAdDto> Response { get; set; } = [];

    public Task<IReadOnlyCollection<JobSearchAdDto>> SearchAdsAsync(
        DateTime publishedAfter,
        DateTime? publishedBefore,
        IReadOnlyCollection<string> municipalityIds,
        IReadOnlyCollection<string> occupationGroupIds,
        int? maxLimit,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(Response);
    }
}

internal sealed class StubTaxonomyConceptValidator : ITaxonomyConceptValidator
{
    public HashSet<string> MunicipalityIds { get; } = new(StringComparer.OrdinalIgnoreCase);

    public HashSet<string> OccupationGroupIds { get; } = new(StringComparer.OrdinalIgnoreCase);

    public bool IsValidMunicipalityId(string conceptId)
    {
        return MunicipalityIds.Contains(conceptId);
    }

    public bool IsValidOccupationGroupId(string conceptId)
    {
        return OccupationGroupIds.Contains(conceptId);
    }
}

internal sealed class SearchAdsTestFactory(
    StubJobSearchClient jobSearchClient,
    StubTaxonomyConceptValidator taxonomyConceptValidator) : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"search-ads-tests-{Guid.NewGuid()}";
    private readonly IServiceProvider _inMemoryProvider = new ServiceCollection()
        .AddEntityFrameworkInMemoryDatabase()
        .BuildServiceProvider();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));
            services.RemoveAll(typeof(ApplicationDbContext));
            services.RemoveAll<IJobSearchClient>();
            services.RemoveAll<ITaxonomyConceptValidator>();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName)
                    .UseInternalServiceProvider(_inMemoryProvider));

            services.AddSingleton<IJobSearchClient>(jobSearchClient);
            services.AddSingleton<ITaxonomyConceptValidator>(taxonomyConceptValidator);
        });
    }
}