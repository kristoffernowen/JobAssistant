using JobAssistant.Application.Common.Entities;
using JobAssistant.Application.Common.Exceptions;
using JobAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobAssistant.Api.Features.Users.AddSkillsOnUser;

public static class AddSkillsOnUserEndpoint
{
    public static RouteGroupBuilder MapAddSkillsOnUser(this RouteGroupBuilder group)
    {
        group.MapPut(
            "/skills",
            async (AddSkillsOnUserRequest request, ApplicationDbContext dbContext, CancellationToken cancellationToken) =>
            {
                var normalized = Features.Users.UserValidation.NormalizeUserName(request.UserName);
                Features.Users.UserValidation.EnsureValidSkills(request.Skills);

                var user = await dbContext.Users
                    .Include(x => x.Skills)
                    .SingleOrDefaultAsync(x => x.NormalizedUserName == normalized, cancellationToken);

                if (user is null)
                {
                    throw new NotFoundException("The requested user was not found.");
                }

                var cleanedSkills = request.Skills
                    .Select(x => x.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                foreach (var skill in cleanedSkills)
                {
                    var alreadyExists = user.Skills.Any(x => string.Equals(x.Value, skill, StringComparison.OrdinalIgnoreCase));
                    if (alreadyExists)
                    {
                        continue;
                    }

                    user.Skills.Add(new UserSkill
                    {
                        Id = Guid.NewGuid(),
                        UserProfileId = user.Id,
                        Value = skill
                    });
                }

                await dbContext.SaveChangesAsync(cancellationToken);

                var response = new AddSkillsOnUserResponse(
                    user.Skills.Select(x => x.Value).OrderBy(x => x).ToList(),
                    user.UserName);

                return Results.Ok(response);
            })
            .WithName("AddSkillsOnUser")
            .WithSummary("Add skills to an existing user profile.");

        return group;
    }
}
