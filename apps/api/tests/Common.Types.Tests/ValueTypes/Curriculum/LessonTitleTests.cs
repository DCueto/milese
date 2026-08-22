using Milese.Common.Types.ValueTypes.Curriculum;

namespace Milese.Common.Types.Tests.ValueTypes.Curriculum;

public class LessonTitleTests
{
    [Test]
    public void Parse_Should_Fail_When_Null()
    {
        var result = LessonTitle.Parse(null);

        result.IsSuccess.ShouldBeFalse();
    }

    [Test]
    public void Parse_Should_Fail_When_Empty()
    {
        var result = LessonTitle.Parse("   ");

        result.IsSuccess.ShouldBeFalse();
    }

    [Test]
    public void Parse_Should_Fail_When_Over_MaxLength()
    {
        var value = new string('a', LessonTitle.MaxLength + 1);

        var result = LessonTitle.Parse(value);

        result.IsSuccess.ShouldBeFalse();
    }

    [Test]
    public void Parse_Should_Succeed_At_MaxLength()
    {
        var value = new string('a', LessonTitle.MaxLength);

        var result = LessonTitle.Parse(value);

        result.IsSuccess.ShouldBeTrue();
    }

    [Test]
    public void Parse_Should_Succeed_With_Valid_Value()
    {
        var result = LessonTitle.Parse("Why Hash Tables Give O(1) Lookups");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("Why Hash Tables Give O(1) Lookups");
    }
}
