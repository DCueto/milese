using Milese.Common.Types.ValueTypes.Curriculum;

namespace Milese.Common.Types.Tests.ValueTypes.Curriculum;

public class TrackTitleTests
{
    [Test]
    public void Parse_Should_Fail_When_Null()
    {
        var result = TrackTitle.Parse(null);

        result.IsSuccess.ShouldBeFalse();
    }

    [Test]
    public void Parse_Should_Fail_When_Empty()
    {
        var result = TrackTitle.Parse("   ");

        result.IsSuccess.ShouldBeFalse();
    }

    [Test]
    public void Parse_Should_Fail_When_Over_MaxLength()
    {
        var value = new string('a', TrackTitle.MaxLength + 1);

        var result = TrackTitle.Parse(value);

        result.IsSuccess.ShouldBeFalse();
    }

    [Test]
    public void Parse_Should_Succeed_At_MaxLength()
    {
        var value = new string('a', TrackTitle.MaxLength);

        var result = TrackTitle.Parse(value);

        result.IsSuccess.ShouldBeTrue();
    }

    [Test]
    public void Parse_Should_Succeed_With_Valid_Value()
    {
        var result = TrackTitle.Parse("Foundations");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("Foundations");
    }
}
