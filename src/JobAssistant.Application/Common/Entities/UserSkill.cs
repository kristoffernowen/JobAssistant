namespace JobAssistant.Application.Common.Entities;

public sealed class UserSkill
{
    public Guid Id { get; set; }

    public Guid UserProfileId { get; set; }

    public string Value { get; set; } = string.Empty;

    public UserProfile UserProfile { get; set; } = null!;
}
