using Milese.Common.Types.ValueTypes.Curriculum;

namespace Milese.Common.Types.Tests.ValueTypes.Curriculum;

public class SubjectTitleTests
{
    [Test]
    public void Parse_Should_Fail_When_Null()
    {
        var result = SubjectTitle.Parse(null);

        result.IsSuccess.ShouldBeFalse();
    }

    [Test]
    public void Parse_Should_Fail_When_Empty()
    {
        var result = SubjectTitle.Parse("   ");

        result.IsSuccess.ShouldBeFalse();
    }

    [Test]
    public void Parse_Should_Fail_When_Over_MaxLength()
    {
        var value = new string('a', SubjectTitle.MaxLength + 1);

        var result = SubjectTitle.Parse(value);

        result.IsSuccess.ShouldBeFalse();
    }

    [Test]
    public void Parse_Should_Succeed_At_MaxLength()
    {
        var value = new string('a', SubjectTitle.MaxLength);

        var result = SubjectTitle.Parse(value);

        result.IsSuccess.ShouldBeTrue();
    }

    [Test]
    public void Parse_Should_Succeed_With_Valid_Value()
    {
        var result = SubjectTitle.Parse("Data Structures & Algorithms");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("Data Structures & Algorithms");
    }
}
