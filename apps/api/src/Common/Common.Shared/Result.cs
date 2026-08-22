using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Milese.Common.Shared;

/// <summary>
/// Result monad representing the outcome of an operation that may succeed or fail.
/// </summary>
[JsonConverter(typeof(ResultJsonConverterFactory))]
public readonly struct Result<T, TErr>
{
    private readonly T? value;
    private readonly TErr? error;

    private Result(T value)
    {
        this.value = value;
        error = default;
        IsSuccess = true;
    }

    private Result(TErr error)
    {
        value = default;
        this.error = error;
        IsSuccess = false;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public T Value => IsSuccess
        ? value!
        : throw new InvalidOperationException("Cannot access the value of a failed Result.");

    public TErr Error => IsFailure
        ? error!
        : throw new InvalidOperationException("Cannot access the error of a successful Result.");

    public static Result<T, TErr> Success(T value) => new(value);

    public static Result<T, TErr> Failure(TErr error) => new(error);

    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<TErr, TOut> onFailure) =>
        IsSuccess ? onSuccess(value!) : onFailure(error!);

    public Result<TOut, TErr> Map<TOut>(Func<T, TOut> mapper) =>
        IsSuccess
            ? mapper(value!)
            : error!;

    public Result<TOut, TErr> Map<TOut>(Func<T, Result<TOut, TErr>> mapper) =>
        IsSuccess
            ? mapper(value!)
            : error!;


    public Task<Result<TOut, TErr>> Map<TOut>(Func<T, Task<Result<TOut, TErr>>> mapper) =>
            IsSuccess
                ? mapper(value!)
                : Task.FromResult(Result<TOut, TErr>.Failure(error!));

    public async Task<Result<TOut, Either<TErr, TErr2>>> Map<TOut, TErr2>(Func<T, Task<Result<TOut, TErr2>>> mapper)
    {
        if (IsSuccess)
        {
            var innerValue = await mapper(value!);
            if (innerValue.IsSuccess)
            {
                return innerValue.Value;
            }
            else
            {
                return Either<TErr, TErr2>.FromRight(innerValue.error!);
            }
        }
        else
        {
            return Either<TErr, TErr2>.FromLeft(error!);
        }
    }

    public async Task<Result<TOut, Either<TErr, TErr2>>> Map<TOut, TErr2>(Func<T, Task<Result<TOut, Either<TErr, TErr2>>>> mapper)
    {
        if (IsSuccess)
        {
            var innerValue = await mapper(value!);
            if (innerValue.IsSuccess)
            {
                return innerValue.Value;
            }
            else
            {
                if (innerValue.error.IsLeft)
                    return Either<TErr, TErr2>.FromLeft(innerValue.error.Left);
                else
                    return Either<TErr, TErr2>.FromRight(innerValue.error.Right);
            }
        }
        else
        {
            return Either<TErr, TErr2>.FromLeft(error!);
        }
    }

    public Result<T, TErrOut> MapError<TErrOut>(Func<TErr, TErrOut> errorMapper) =>
        IsSuccess
            ? value!
            : errorMapper(error!);

    public Result<T, TErr> OnSuccess(Action<T> action)
    {
        if (IsSuccess)
            action(value!);

        return this;
    }

    public Result<T, TErr> OnFailure(Action<TErr> action)
    {
        if (IsFailure)
            action(error!);
        return this;
    }

    public static implicit operator Result<T, TErr>(T value) => Success(value);
    public static implicit operator Result<T, TErr>(TErr value) => Failure(value);

    public override string ToString() =>
        IsSuccess ? $"Success({value})" : $"Failure({error})";
}
