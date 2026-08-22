using System;
using System.Text.Json.Serialization;

namespace Milese.Common.Shared;

/// <summary>
/// Either monad representing a value that is either TLeft or TRight.
/// By convention, Left is the error/alternative case and Right is the success/primary case.
/// </summary>
[JsonConverter(typeof(EitherJsonConverterFactory))]
public readonly struct Either<TLeft, TRight>
{
    private readonly TLeft? left;
    private readonly TRight? right;

    private Either(TLeft left)
    {
        this.left = left;
        right = default;
        IsRight = false;
    }

    private Either(TRight right)
    {
        left = default;
        this.right = right;
        IsRight = true;
    }

    public bool IsRight { get; }

    public bool IsLeft => !IsRight;

    public TRight Right => IsRight
        ? right!
        : throw new InvalidOperationException("Cannot access Right on an Either that is Left.");

    public TLeft Left => IsLeft
        ? left!
        : throw new InvalidOperationException("Cannot access Left on an Either that is Right.");

    public static Either<TLeft, TRight> FromLeft(TLeft value) => new(value);

    public static Either<TLeft, TRight> FromRight(TRight value) => new(value);

    public Either<TOutLeft, TRight> MapLeft<TOutLeft>(Func<TLeft, TOutLeft> mapper) =>
        IsLeft
            ? mapper(left!)
            : right!;

    public TOut Match<TOut>(Func<TLeft, TOut> onLeft, Func<TRight, TOut> onRight) =>
        IsRight ? onRight(right!) : onLeft(left!);

    public Either<TLeft, TRight> OnRight(Action<TRight> action)
    {
        if (IsRight)
            action(right!);

        return this;
    }

    public Either<TLeft, TRight> OnLeft(Action<TLeft> action)
    {
        if (IsLeft)
            action(left!);

        return this;
    }

    public TRight GetRightOrDefault(TRight defaultValue) =>
        IsRight ? right! : defaultValue;

    public static implicit operator Either<TLeft, TRight>(TRight value) => FromRight(value);
    public static implicit operator Either<TLeft, TRight>(TLeft value) => FromLeft(value);

    public override string ToString() =>
        IsRight ? $"Right({right})" : $"Left({left})";
}
