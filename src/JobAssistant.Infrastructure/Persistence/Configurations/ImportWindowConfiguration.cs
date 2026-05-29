using JobAssistant.Application.Common.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobAssistant.Infrastructure.Persistence.Configurations;

public sealed class ImportWindowConfiguration : IEntityTypeConfiguration<ImportWindow>
{
    public void Configure(EntityTypeBuilder<ImportWindow> builder)
    {
        builder.ToTable("ImportWindows");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Location)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(x => new { x.FromUtc, x.ToUtc });
    }
}
