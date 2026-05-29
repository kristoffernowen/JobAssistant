using JobAssistant.Application.Common.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobAssistant.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<UserProfile> Users => Set<UserProfile>();

    public DbSet<UserSkill> UserSkills => Set<UserSkill>();

    public DbSet<JobAd> JobAds => Set<JobAd>();

    public DbSet<ImportWindow> ImportWindows => Set<ImportWindow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
