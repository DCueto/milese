using Milese.Common.Types.ValueTypes.Identity;

namespace Milese.Common.Types.Tests.ValueTypes.Identity;

public class EmailTests
{
    [Test]
    public void Parse_Should_Fail_When_Null()
    {
        var result = Email.Parse(null);

        result.IsSuccess.ShouldBeFalse();
    }

    [Test]
    public void Parse_Should_Fail_When_Empty()
    {
        var result = Email.Parse("   ");

        result.IsSuccess.ShouldBeFalse();
    }

    [Test]
    public void Parse_Should_Fail_When_Over_MaxLength()
    {
        var value = new string('a', Email.MaxLength + 1) + "@example.com";

        var result = Email.Parse(value);

        result.IsSuccess.ShouldBeFalse();
    }

    [Test]
    public void Parse_Should_Fail_When_Format_Is_Invalid()
    {
        var result = Email.Parse("not-an-email");

        result.IsSuccess.ShouldBeFalse();
    }

    [Test]
    public void Parse_Should_Succeed_With_Valid_Value()
    {
        var result = Email.Parse("learner@example.com");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("learner@example.com");
    }
}
