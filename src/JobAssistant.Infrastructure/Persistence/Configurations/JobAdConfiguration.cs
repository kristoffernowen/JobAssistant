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

        builder.Property(x => x.OccupationGroup)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.OccupationField)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.FullData)
            .HasColumnType("nvarchar(max)")
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<JobAssistant.Application.JobStream.JobStreamAdDto>(v, (System.Text.Json.JsonSerializerOptions?)null));

        builder.HasIndex(x => x.Loaded);
        builder.HasIndex(x => x.PublicationDate);
        builder.HasIndex(x => new { x.Location, x.OccupationGroup });
        builder.HasIndex(x => new { x.Location, x.OccupationField });
    }
}
