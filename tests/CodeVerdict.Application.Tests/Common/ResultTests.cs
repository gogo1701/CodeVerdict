using CodeVerdict.Application.Common;
using Xunit;

namespace CodeVerdict.Application.Tests.Common;

public sealed class ResultTests
{
    [Fact]
    public void Success_contains_the_value()
    {
        var result = Result<int>.Success(42);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(42, result.Value);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Failure_contains_the_error_and_has_no_value()
    {
        var error = new Error("Problem.Invalid", "The problem is invalid.", ErrorType.Validation);
        var result = Result<int>.Failure(error);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Same(error, result.Error);
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }
}
