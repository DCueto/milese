using Milese.Common.Shared;

namespace Milese.Common.Shared.Tests;

public class AllOrSomeTests
{
    [Test]
    public void All_Represents_The_Totality_With_No_Items()
    {
        var allOrSome = AllOrSome<int>.All();

        allOrSome.Match(() => true, _ => false).ShouldBeTrue();
        allOrSome.Match(() => -1, items => items.Count).ShouldBe(-1);
    }

    [Test]
    public void Some_Represents_A_Concrete_NonEmpty_Subset()
    {
        var allOrSome = AllOrSome<int>.Some(NotEmptyList<int>.FromTrusted([1, 2]));

        allOrSome.Match(() => true, _ => false).ShouldBeFalse();
        allOrSome.Match(() => [], items => items).ShouldBe([1, 2]);
    }

    [Test]
    public void FromValuesOrAll_Treats_Null_Or_Empty_As_All()
    {
        AllOrSome<int>.FromValuesOrAll(null).Match(() => true, _ => false).ShouldBeTrue();
        AllOrSome<int>.FromValuesOrAll([]).Match(() => true, _ => false).ShouldBeTrue();
    }

    [Test]
    public void FromValuesOrAll_Treats_NonEmpty_As_Some()
    {
        var allOrSome = AllOrSome<int>.FromValuesOrAll([1, 2, 3]);

        allOrSome.Match(() => true, _ => false).ShouldBeFalse();
        allOrSome.Match(() => [], items => items).ShouldBe([1, 2, 3]);
    }

    [Test]
    public void Match_Invokes_The_Matching_Branch()
    {
        AllOrSome<int>.All().Match(() => "all", items => $"some:{items.Count}").ShouldBe("all");
        AllOrSome<int>.Some(NotEmptyList<int>.FromTrusted([1, 2]))
            .Match(() => "all", items => $"some:{items.Count}")
            .ShouldBe("some:2");
    }

    [Test]
    public void Switch_Invokes_The_Matching_Branch()
    {
        var branch = "";

        AllOrSome<int>.All().Switch(() => branch = "all", _ => branch = "some");
        branch.ShouldBe("all");

        AllOrSome<int>.Some(NotEmptyList<int>.FromTrusted([1])).Switch(() => branch = "all", _ => branch = "some");
        branch.ShouldBe("some");
    }

    [Test]
    public void Contains_Is_Always_True_For_All()
    {
        AllOrSome<int>.All().Contains(999).ShouldBeTrue();
    }

    [Test]
    public void Contains_Checks_Membership_For_Some()
    {
        var allOrSome = AllOrSome<int>.Some(NotEmptyList<int>.FromTrusted([1, 2]));

        allOrSome.Contains(1).ShouldBeTrue();
        allOrSome.Contains(3).ShouldBeFalse();
    }
}
