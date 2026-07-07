namespace JobAssistant.Application.Common.Entities;

public sealed class UserProfile
{
    public Guid Id { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string NormalizedUserName { get; set; } = string.Empty;

    public ICollection<UserSkill> Skills { get; set; } = new List<UserSkill>();
}
