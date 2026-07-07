using JobAssistant.Application.Common.Exceptions;

namespace JobAssistant.Application.Tests;

public sealed class ValidationExceptionTests
{
    [Fact]
    public void Constructor_SetsMessageAndErrors()
    {
        var errors = new Dictionary<string, string[]>
        {
            ["field"] = ["is required"]
        };

        var exception = new ValidationException("Validation failed.", errors);

        Assert.Equal("Validation failed.", exception.Message);
        Assert.Equal(errors, exception.Errors);
    }
}
