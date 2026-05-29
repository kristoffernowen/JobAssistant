namespace JobAssistant.Api.Features.Users.AddSkillsOnUser;

public sealed record AddSkillsOnUserRequest(List<string> Skills, string UserName);

public sealed record AddSkillsOnUserResponse(List<string> Skills, string UserName);
