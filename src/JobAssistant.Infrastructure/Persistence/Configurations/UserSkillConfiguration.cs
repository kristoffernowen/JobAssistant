using JobAssistant.Application.Common.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobAssistant.Infrastructure.Persistence.Configurations;

public sealed class UserSkillConfiguration : IEntityTypeConfiguration<UserSkill>
{
    public void Configure(EntityTypeBuilder<UserSkill> builder)
    {
        builder.ToTable("UserSkills");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Value)
            .HasMaxLength(300)
            .IsRequired();

        builder.HasIndex(x => new { x.UserProfileId, x.Value });
    }
}
