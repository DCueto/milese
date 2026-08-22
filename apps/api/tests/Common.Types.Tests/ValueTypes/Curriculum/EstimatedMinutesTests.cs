using Milese.Common.Types.ValueTypes.Curriculum;

namespace Milese.Common.Types.Tests.ValueTypes.Curriculum;

public class EstimatedMinutesTests
{
    [Test]
    public void Parse_Should_Fail_Below_Minimum()
    {
        var result = EstimatedMinutes.Parse(0);

        result.IsSuccess.ShouldBeFalse();
    }

    [Test]
    public void Parse_Should_Fail_Above_Maximum()
    {
        var result = EstimatedMinutes.Parse(16);

        result.IsSuccess.ShouldBeFalse();
    }

    [Test]
    public void Parse_Should_Succeed_At_Minimum()
    {
        var result = EstimatedMinutes.Parse(1);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(1);
    }

    [Test]
    public void Parse_Should_Succeed_At_Maximum()
    {
        var result = EstimatedMinutes.Parse(15);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(15);
    }

    [Test]
    public void Parse_Should_Succeed_Within_Range()
    {
        var result = EstimatedMinutes.Parse(10);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(10);
    }
}
