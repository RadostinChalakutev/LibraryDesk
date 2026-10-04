using LibraryDesk.Application.Primitives;

namespace LibraryDesk.Application.Tests.Primitives;

public sealed class OperationResultTests
{
    [Fact]
    public void SuccessHasNoError()
    {
        var result = OperationResult.Success();

        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
    }

    [Fact]
    public void FailureRequiresMeaningfulError()
    {
        var result = OperationResult.Failure("A validation error occurred.");

        Assert.False(result.IsSuccess);
        Assert.Equal("A validation error occurred.", result.Error);
        Assert.Throws<ArgumentException>(() => OperationResult.Failure(" "));
    }
}
