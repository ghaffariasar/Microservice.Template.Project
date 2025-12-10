using Shared.Common;
using Xunit;

namespace Shared.Tests;

public class ResultTests
{
    [Fact]
    public void Success_Should_Set_IsSuccess_True()
    {
        // Arrange
        // (نیاز به Arrange ندارد)

        // Act
        var r = Result.Success();

        // Assert
        Assert.True(r.IsSuccess);
        Assert.False(r.IsFailure);
        Assert.Equal(string.Empty, r.ErrorMessage);
    }

    [Fact]
    public void Failure_Should_Set_IsSuccess_False_And_ErrorMessage()
    {
        // Arrange
        const string message = "err";

        // Act
        var r = Result.Failure(message);

        // Assert
        Assert.False(r.IsSuccess);
        Assert.True(r.IsFailure);
        Assert.Equal(message, r.ErrorMessage);
    }

    [Fact]
    public void Generic_Success_Should_Carry_Value()
    {
        // Arrange
        const int value = 42;

        // Act
        var r = Result.Success(value);

        // Assert
        Assert.True(r.IsSuccess);
        Assert.Equal(value, r.Value);
    }
}