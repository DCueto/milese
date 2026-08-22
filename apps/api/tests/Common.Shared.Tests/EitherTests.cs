using System;
using Milese.Common.Shared;

namespace Milese.Common.Shared.Tests;

public class EitherTests
{
    [Test]
    public void FromLeft_Sets_IsLeft()
    {
        var either = Either<string, int>.FromLeft("err");

        either.IsLeft.ShouldBeTrue();
        either.IsRight.ShouldBeFalse();
        either.Left.ShouldBe("err");
    }

    [Test]
    public void FromRight_Sets_IsRight()
    {
        var either = Either<string, int>.FromRight(5);

        either.IsRight.ShouldBeTrue();
        either.IsLeft.ShouldBeFalse();
        either.Right.ShouldBe(5);
    }

    [Test]
    public void Right_Throws_When_Left()
    {
        var either = Either<string, int>.FromLeft("err");

        Should.Throw<InvalidOperationException>(() => either.Right);
    }

    [Test]
    public void Left_Throws_When_Right()
    {
        var either = Either<string, int>.FromRight(1);

        Should.Throw<InvalidOperationException>(() => either.Left);
    }

    [Test]
    public void Match_Invokes_The_Matching_Branch()
    {
        Either<string, int>.FromLeft("err").Match(l => l, r => r.ToString()).ShouldBe("err");
        Either<string, int>.FromRight(9).Match(l => l, r => r.ToString()).ShouldBe("9");
    }

    [Test]
    public void MapLeft_Transforms_Left_Only()
    {
        var mapped = Either<string, int>.FromLeft("err").MapLeft(l => l.Length);
        mapped.Left.ShouldBe(3);

        var untouched = Either<string, int>.FromRight(9).MapLeft(l => l.Length);
        untouched.Right.ShouldBe(9);
    }

    [Test]
    public void GetRightOrDefault_Returns_Right_Or_Fallback()
    {
        Either<string, int>.FromRight(9).GetRightOrDefault(-1).ShouldBe(9);
        Either<string, int>.FromLeft("err").GetRightOrDefault(-1).ShouldBe(-1);
    }

    [Test]
    public void OnLeft_And_OnRight_Only_Invoke_The_Matching_Callback()
    {
        var leftInvoked = false;
        var rightInvoked = false;

        Either<string, int>.FromLeft("err")
            .OnLeft(_ => leftInvoked = true)
            .OnRight(_ => rightInvoked = true);

        leftInvoked.ShouldBeTrue();
        rightInvoked.ShouldBeFalse();
    }
}
