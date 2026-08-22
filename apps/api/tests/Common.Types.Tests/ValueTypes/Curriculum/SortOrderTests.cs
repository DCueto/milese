using Milese.Common.Types.ValueTypes.Curriculum;

namespace Milese.Common.Types.Tests.ValueTypes.Curriculum;

public class SortOrderTests
{
    [Test]
    public void Parse_Should_Fail_When_Zero()
    {
        var result = SortOrder.Parse(0);

        result.IsSuccess.ShouldBeFalse();
    }

    [Test]
    public void Parse_Should_Fail_When_Negative()
    {
        var result = SortOrder.Parse(-1);

        result.IsSuccess.ShouldBeFalse();
    }

    [Test]
    public void Parse_Should_Succeed_When_Positive()
    {
        var result = SortOrder.Parse(1);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(1);
    }
}
