using System.Net;
using System.Net.Http.Json;
using JobAssistant.Api.Features.Users.AddSkillsOnUser;

namespace JobAssistant.Api.Tests;

public sealed class UsersEndpointsTests
{
    [Fact]
    public async Task AddSkillsOnUser_AddsSkillsWithoutConcurrencyErrors()
    {
        using var factory = new Testing.ApiTestFactory();
        using var client = factory.CreateClient();

        var createUserResponse = await client.PostAsJsonAsync("/users", new
        {
            userName = "anna-andersson"
        });

        Assert.Equal(HttpStatusCode.Created, createUserResponse.StatusCode);

        var addSkillsResponse = await client.PutAsJsonAsync("/users/skills", new
        {
            userName = "anna-andersson",
            skills = new[] { "c#", ".net" }
        });

        addSkillsResponse.EnsureSuccessStatusCode();
        var payload = await addSkillsResponse.Content.ReadFromJsonAsync<AddSkillsOnUserResponse>();

        Assert.NotNull(payload);
        Assert.Equal("anna-andersson", payload.UserName);
        Assert.Contains("c#", payload.Skills, StringComparer.OrdinalIgnoreCase);
        Assert.Contains(".net", payload.Skills, StringComparer.OrdinalIgnoreCase);
    }
}
