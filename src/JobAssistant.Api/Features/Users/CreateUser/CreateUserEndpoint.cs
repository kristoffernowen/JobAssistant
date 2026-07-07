using JobAssistant.Application.Common.Entities;
using JobAssistant.Application.Common.Exceptions;
using JobAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobAssistant.Api.Features.Users.CreateUser;

public static class CreateUserEndpoint
{
    public static RouteGroupBuilder MapCreateUser(this RouteGroupBuilder group)
    {
        group.MapPost(
            "/",
            async (CreateUserRequest request, ApplicationDbContext dbContext, CancellationToken cancellationToken) =>
            {
                var normalized = Features.Users.UserValidation.NormalizeUserName(request.UserName);
                var exists = await dbContext.Users
                    .AsNoTracking()
                    .AnyAsync(x => x.NormalizedUserName == normalized, cancellationToken);

                if (exists)
                {
                    throw new ConflictException("A user with the same username already exists.");
                }

                var user = new UserProfile
                {
                    Id = Guid.NewGuid(),
                    UserName = request.UserName.Trim(),
                    NormalizedUserName = normalized
                };

                dbContext.Users.Add(user);
                await dbContext.SaveChangesAsync(cancellationToken);

                var response = new CreateUserResponse(user.UserName, user.Id);
                return Results.Created($"/users/{response.Id}", response);
            })
            .WithName("CreateUser")
            .WithSummary("Create a new user profile.");

        return group;
    }
}
