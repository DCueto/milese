using Milese.Common.Types.ValueTypes.Curriculum;

namespace Milese.Common.Types.Tests.ValueTypes.Curriculum;

public class ConceptTitleTests
{
    [Test]
    public void Parse_Should_Fail_When_Null()
    {
        var result = ConceptTitle.Parse(null);

        result.IsSuccess.ShouldBeFalse();
    }

    [Test]
    public void Parse_Should_Fail_When_Empty()
    {
        var result = ConceptTitle.Parse("   ");

        result.IsSuccess.ShouldBeFalse();
    }

    [Test]
    public void Parse_Should_Fail_When_Over_MaxLength()
    {
        var value = new string('a', ConceptTitle.MaxLength + 1);

        var result = ConceptTitle.Parse(value);

        result.IsSuccess.ShouldBeFalse();
    }

    [Test]
    public void Parse_Should_Succeed_At_MaxLength()
    {
        var value = new string('a', ConceptTitle.MaxLength);

        var result = ConceptTitle.Parse(value);

        result.IsSuccess.ShouldBeTrue();
    }

    [Test]
    public void Parse_Should_Succeed_With_Valid_Value()
    {
        var result = ConceptTitle.Parse("Hash Tables");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("Hash Tables");
    }
}
