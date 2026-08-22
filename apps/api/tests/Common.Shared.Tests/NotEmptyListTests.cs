using Milese.Common.Shared;

namespace Milese.Common.Shared.Tests;

public class NotEmptyListTests
{
    [Test]
    public void Constructor_Adds_The_First_Element()
    {
        var list = new NotEmptyList<int>(1);

        list.ShouldBe([1]);
    }

    [Test]
    public void FromTrusted_Wraps_The_Given_Elements()
    {
        var list = NotEmptyList<int>.FromTrusted([1, 2, 3]);

        list.ShouldBe([1, 2, 3]);
    }

    [Test]
    public void Parse_Fails_On_Null()
    {
        var result = NotEmptyList<int>.Parse(null);

        result.IsFailure.ShouldBeTrue();
    }

    [Test]
    public void Parse_Fails_On_Empty()
    {
        var result = NotEmptyList<int>.Parse([]);

        result.IsFailure.ShouldBeTrue();
    }

    [Test]
    public void Parse_Succeeds_On_NonEmpty()
    {
        var result = NotEmptyList<int>.Parse([1, 2]);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe([1, 2]);
    }
}
