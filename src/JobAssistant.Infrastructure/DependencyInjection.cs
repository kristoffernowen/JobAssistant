using JobAssistant.Application.Common.Interfaces;
using JobAssistant.Infrastructure.External.JobStream;
using JobAssistant.Infrastructure.Persistence;
using JobAssistant.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JobAssistant.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddSingleton<ILocationConceptMapper, VastmanlandLocationConceptMapper>();
        services.AddHttpClient<IJobStreamClient, JobStreamClient>(client =>
        {
            client.BaseAddress = new Uri("https://jobstream.api.jobtechdev.se/");
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        });

        return services;
    }
}
