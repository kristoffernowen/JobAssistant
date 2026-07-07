using JobAssistant.Application.Common.Entities;
using JobAssistant.Application.Common.Interfaces;
using JobAssistant.Application.JobStream;
using JobAssistant.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JobAssistant.IntegrationTests.Testing;

internal sealed class StubLocationConceptMapper(string conceptId) : ILocationConceptMapper
{
    public bool TryMapToConceptIds(string locationInput, out IReadOnlyCollection<string> conceptIds)
    {
        conceptIds = [conceptId];
        return true;
    }
}

internal sealed class StubJobStreamClient : IJobStreamClient
{
    private readonly Queue<IReadOnlyCollection<JobStreamAdDto>> _responses = new();

    public void EnqueueResponse(params JobStreamAdDto[] ads)
    {
        _responses.Enqueue(ads);
    }

    public Task<IReadOnlyCollection<JobStreamAdDto>> GetStreamAdsAsync(
        DateTime fromUtc,
        DateTime toUtc,
        IReadOnlyCollection<string> locationConceptIds,
        CancellationToken cancellationToken)
    {
        if (_responses.Count == 0)
        {
            return Task.FromResult<IReadOnlyCollection<JobStreamAdDto>>([]);
        }

        return Task.FromResult(_responses.Dequeue());
    }
}

internal sealed class LoadAdsTestFactory(StubJobStreamClient jobStreamClient) : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"integration-tests-{Guid.NewGuid()}";
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
            services.RemoveAll<ILocationConceptMapper>();
            services.RemoveAll<IJobStreamClient>();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName)
                    .UseInternalServiceProvider(_inMemoryProvider));

            services.AddSingleton<ILocationConceptMapper>(new StubLocationConceptMapper("G6DV_fKE_Viz"));
            services.AddSingleton<IJobStreamClient>(jobStreamClient);
        });
    }

    public async Task SeedImportWindowAsync(ImportWindow importWindow)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.ImportWindows.Add(importWindow);
        await dbContext.SaveChangesAsync();
    }

    public async Task<JobAd?> GetJobAdAsync(string sourceId)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await dbContext.JobAds
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.SourceType == "JobStream" && x.SourceId == sourceId);
    }
}
