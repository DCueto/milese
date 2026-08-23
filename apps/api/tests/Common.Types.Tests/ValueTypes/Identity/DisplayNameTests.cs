using Milese.Common.Types.ValueTypes.Identity;

namespace Milese.Common.Types.Tests.ValueTypes.Identity;

public class DisplayNameTests
{
    [Test]
    public void Parse_Should_Fail_When_Null()
    {
        var result = DisplayName.Parse(null);

        result.IsSuccess.ShouldBeFalse();
    }

    [Test]
    public void Parse_Should_Fail_When_Empty()
    {
        var result = DisplayName.Parse("   ");

        result.IsSuccess.ShouldBeFalse();
    }

    [Test]
    public void Parse_Should_Fail_When_Over_MaxLength()
    {
        var value = new string('a', DisplayName.MaxLength + 1);

        var result = DisplayName.Parse(value);

        result.IsSuccess.ShouldBeFalse();
    }

    [Test]
    public void Parse_Should_Succeed_At_MaxLength()
    {
        var value = new string('a', DisplayName.MaxLength);

        var result = DisplayName.Parse(value);

        result.IsSuccess.ShouldBeTrue();
    }

    [Test]
    public void Parse_Should_Succeed_With_Valid_Value()
    {
        var result = DisplayName.Parse("Daniel Cueto");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("Daniel Cueto");
    }
}
