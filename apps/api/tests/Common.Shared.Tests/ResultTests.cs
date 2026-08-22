using System;
using System.Threading.Tasks;
using Milese.Common.Shared;

namespace Milese.Common.Shared.Tests;

public class ResultTests
{
    [Test]
    public void Success_Sets_IsSuccess_And_Value()
    {
        var result = Result<int, string>.Success(42);

        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Value.ShouldBe(42);
    }

    [Test]
    public void Failure_Sets_IsFailure_And_Error()
    {
        var result = Result<int, string>.Failure("bad input");

        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe("bad input");
    }

    [Test]
    public void Value_Throws_When_Failure()
    {
        var result = Result<int, string>.Failure("bad input");

        Should.Throw<InvalidOperationException>(() => result.Value);
    }

    [Test]
    public void Error_Throws_When_Success()
    {
        var result = Result<int, string>.Success(1);

        Should.Throw<InvalidOperationException>(() => result.Error);
    }

    [Test]
    public void Implicit_Conversion_From_Value_Produces_Success()
    {
        Result<int, string> result = 7;

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(7);
    }

    [Test]
    public void Implicit_Conversion_From_Error_Produces_Failure()
    {
        Result<int, string> result = "nope";

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe("nope");
    }

    [Test]
    public void Match_Invokes_OnSuccess_When_Success()
    {
        var result = Result<int, string>.Success(3);

        var matched = result.Match(v => v * 2, _ => -1);

        matched.ShouldBe(6);
    }

    [Test]
    public void Match_Invokes_OnFailure_When_Failure()
    {
        var result = Result<int, string>.Failure("err");

        var matched = result.Match(v => v * 2, e => e.Length);

        matched.ShouldBe(3);
    }

    [Test]
    public void Map_Transforms_Value_On_Success()
    {
        var result = Result<int, string>.Success(2).Map(v => v.ToString());

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe("2");
    }

    [Test]
    public void Map_Propagates_Failure_Without_Invoking_Mapper()
    {
        var invoked = false;
        var result = Result<int, string>.Failure("err").Map(v =>
        {
            invoked = true;
            return v.ToString();
        });

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe("err");
        invoked.ShouldBeFalse();
    }

    [Test]
    public void MapError_Transforms_Error_On_Failure()
    {
        var result = Result<int, string>.Failure("err").MapError(e => e.Length);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(3);
    }

    [Test]
    public void MapError_Preserves_Value_On_Success()
    {
        var result = Result<int, string>.Success(5).MapError(e => e.Length);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(5);
    }

    [Test]
    public void OnSuccess_Invokes_Action_Only_When_Success()
    {
        var invoked = false;
        Result<int, string>.Success(1).OnSuccess(_ => invoked = true);
        invoked.ShouldBeTrue();

        invoked = false;
        Result<int, string>.Failure("err").OnSuccess(_ => invoked = true);
        invoked.ShouldBeFalse();
    }

    [Test]
    public void OnFailure_Invokes_Action_Only_When_Failure()
    {
        var invoked = false;
        Result<int, string>.Failure("err").OnFailure(_ => invoked = true);
        invoked.ShouldBeTrue();

        invoked = false;
        Result<int, string>.Success(1).OnFailure(_ => invoked = true);
        invoked.ShouldBeFalse();
    }

    [Test]
    public async Task Map_With_Task_Result_Chains_A_Second_Fallible_Step()
    {
        var result = await Result<int, string>.Success(2)
            .Map<string>(v => Task.FromResult(Result<string, string>.Success(v.ToString())));

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe("2");
    }
}
