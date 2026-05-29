namespace JobAssistant.Api.Features.Users.CreateUser;

public sealed record CreateUserRequest(string UserName);

public sealed record CreateUserResponse(string UserName, Guid Id);
