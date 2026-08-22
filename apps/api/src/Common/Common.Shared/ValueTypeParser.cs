using System;
using System.Globalization;
using System.Threading.Tasks;
using InvalidDataError = Milese.Common.Shared.InvalidData;

namespace Milese.Common.Shared;

/// <summary>
/// Helper functions to parse and validate domain value types from their underlying
/// value. Centralise the validations most value types repeat (not empty, max
/// length, format, minimum value, ...) so each value type's <c>Parse</c> method
/// only needs to delegate to one of these functions, passing at most the field name.
/// </summary>
public static class ValueTypeParser
{
    /// <summary>
    /// Validates that <paramref name="value"/> is not null, empty, or made up
    /// solely of whitespace.
    /// </summary>
    public static Result<TSelf, InvalidDataError> StringNotEmpty<TSelf>(string? value, string? fieldName)
        where TSelf : IStringValueType<TSelf>, new()
    {
        if (string.IsNullOrWhiteSpace(value))
            return EmptyOrNullError(value, fieldName);

        return new TSelf { Value = value };
    }

    /// <summary>
    /// Validates that <paramref name="value"/> is not empty and does not exceed
    /// the maximum length declared by the value type
    /// (<see cref="IStringValueType{TSelf}.MaxLength"/>).
    /// </summary>
    public static Result<TSelf, InvalidDataError> StringNotEmptyAndMaxLength<TSelf>(string? value, string? fieldName)
        where TSelf : IStringValueType<TSelf>, new()
    {
        if (string.IsNullOrWhiteSpace(value))
            return EmptyOrNullError(value, fieldName);

        if (value.Length > TSelf.MaxLength)
            return MaxLengthError(value, fieldName, TSelf.MaxLength);

        return new TSelf { Value = value };
    }

    /// <summary>
    /// Validates that <paramref name="value"/> is not empty and has exactly
    /// <paramref name="exactLength"/> characters.
    /// </summary>
    public static Result<TSelf, InvalidDataError> StringNotEmptyExactLength<TSelf>(
        string? value,
        string? fieldName,
        int exactLength)
        where TSelf : IStringValueType<TSelf>, new()
    {
        if (string.IsNullOrWhiteSpace(value))
            return EmptyOrNullError(value, fieldName);

        if (value.Length > exactLength)
            return MaxLengthError(value, fieldName, exactLength);

        if (value.Length < exactLength)
            return MinLengthError(value, fieldName, exactLength);

        return new TSelf { Value = value };
    }

    /// <summary>
    /// Like <see cref="StringNotEmptyAndMaxLength{TSelf}(string, string)"/> but
    /// also validates that <paramref name="value"/> matches the format indicated
    /// by <paramref name="isValidFormat"/>.
    /// </summary>
    public static Result<TSelf, InvalidDataError> StringNotEmptyMaxLengthAndFormat<TSelf>(
        string? value,
        string? fieldName,
        Func<string, bool> isValidFormat)
        where TSelf : IStringValueType<TSelf>, new()
    {
        var result = StringNotEmptyAndMaxLength<TSelf>(value, fieldName);
        if (result.IsFailure)
            return result;

        if (!isValidFormat(result.Value.Value))
            return new InvalidDataError
            {
                FieldName = fieldName,
                InnerValue = value,
                Constraint = new InvalidDataConstraint.FormatInvalid()
            };

        return result;
    }

    /// <summary>
    /// Validates that <paramref name="value"/> is strictly positive (greater than zero).
    /// </summary>
    public static Result<TSelf, InvalidDataError> StrictlyPositive<TSelf>(int value, string? fieldName)
        where TSelf : IValueType<TSelf, int>, new()
    {
        if (value <= 0)
            return new InvalidDataError
            {
                FieldName = fieldName,
                InnerValue = value,
                Constraint = new InvalidDataConstraint.StrictlyPositive()
            };

        return new TSelf { Value = value };
    }

    /// <summary>
    /// Validates that <paramref name="value"/> is not less than <paramref name="min"/>.
    /// </summary>
    public static Result<TSelf, InvalidDataError> MinValue<TSelf>(decimal value, decimal min, string? fieldName)
        where TSelf : INumericValueType<TSelf>, new()
    {
        if (value < min)
            return new InvalidDataError
            {
                FieldName = fieldName,
                InnerValue = value,
                Constraint = new InvalidDataConstraint.MinValue(min)
            };

        return new TSelf { Value = value };
    }

    public static Task<Result<TSelf, InvalidDataError>> ExistsAsync<TSelf, T>(
        Result<TSelf, InvalidDataError> parsed,
        Func<TSelf, Task<bool>> exists,
        string? fieldName)
        where TSelf : IValueType<TSelf, T>
        =>
        parsed.Map<TSelf>(async self =>
        {
            if (await exists(self))
                return self;

            return new InvalidDataError
            {
                FieldName = fieldName,
                InnerValue = self.Value,
                Constraint = new InvalidDataConstraint.IdNotFound()
            };
        });

    public static Task<Result<TSelf, InvalidDataError>> ParseIdAsync<TSelf>(
        int value,
        string? fieldName,
        Func<TSelf, Task<bool>> exists)
        where TSelf : IValueType<TSelf, int>, new()
        =>
        ExistsAsync<TSelf, int>(StrictlyPositive<TSelf>(value, fieldName), exists, fieldName);

    public static Result<TSelf, InvalidDataError> DateTimeInRange<TSelf>(DateTime value, string? fieldName)
        where TSelf : IDateTimeValueType<TSelf>, new()
    {
        if (value.Year <= IDateTimeValueType<TSelf>.MinYear)
            return new InvalidDataError
            {
                FieldName = fieldName,
                InnerValue = value,
                Constraint = new InvalidDataConstraint.MinValue(IDateTimeValueType<TSelf>.MinYear)
            };

        if (value.Year >= IDateTimeValueType<TSelf>.MaxYear)
            return new InvalidDataError
            {
                FieldName = fieldName,
                InnerValue = value,
                Constraint = new InvalidDataConstraint.MaxValue(IDateTimeValueType<TSelf>.MaxYear)
            };

        return new TSelf { Value = value };
    }


    public static Result<TSelf, InvalidDataError> DateTimeInRange<TSelf>(string? value, string? fieldName)
        where TSelf : IDateTimeValueType<TSelf>, new()
    {
        if (!DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            return new InvalidDataError
            {
                FieldName = fieldName,
                InnerValue = value,
                Constraint = new InvalidDataConstraint.FormatInvalid()
            };

        return DateTimeInRange<TSelf>(parsed, fieldName);
    }

    private static InvalidDataError EmptyOrNullError(string? value, string? fieldName) =>
        new()
        {
            FieldName = fieldName,
            InnerValue = value,
            Constraint = new InvalidDataConstraint.CanNotBeEmptyOrNull()
        };

    private static InvalidDataError MinLengthError(string? value, string? fieldName, int minLength) =>
        new()
        {
            FieldName = fieldName,
            InnerValue = value,
            Constraint = new InvalidDataConstraint.MinLength(minLength)
        };

    private static InvalidDataError MaxLengthError(string? value, string? fieldName, int maxLength) =>
        new()
        {
            FieldName = fieldName,
            InnerValue = value,
            Constraint = new InvalidDataConstraint.MaxLength(maxLength)
        };
}
