using JobAssistant.Application.Common.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobAssistant.Infrastructure.Persistence.Configurations;

public sealed class JobAdConfiguration : IEntityTypeConfiguration<JobAd>
{
    public void Configure(EntityTypeBuilder<JobAd> builder)
    {
        builder.ToTable("JobAds");

        builder.HasKey(x => new { x.SourceType, x.SourceId });

        builder.Property(x => x.SourceType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.SourceId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(x => x.Location)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Category)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(x => x.Loaded);
        builder.HasIndex(x => new { x.Location, x.Category });
    }
}
