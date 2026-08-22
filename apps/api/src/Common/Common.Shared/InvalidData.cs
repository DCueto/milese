using System.Globalization;
using System.Text.Json.Serialization;
using Milese.Common.Shared.Resources;

namespace Milese.Common.Shared;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(MinLength), "MinLength")]
[JsonDerivedType(typeof(MaxLength), "MaxLength")]
[JsonDerivedType(typeof(MinValue), "MinValue")]
[JsonDerivedType(typeof(MaxValue), "MaxValue")]
[JsonDerivedType(typeof(EnumNotDefined), "EnumNotDefined")]
[JsonDerivedType(typeof(IdNotFound), "IdNotFound")]
[JsonDerivedType(typeof(FormatInvalid), "FormatInvalid")]
[JsonDerivedType(typeof(CanNotBeNull), "CanNotBeNull")]
[JsonDerivedType(typeof(CanNotBeEmptyOrNull), "CanNotBeEmptyOrNull")]
[JsonDerivedType(typeof(StrictlyPositive), "StrictlyPositive")]
[JsonDerivedType(typeof(GenericError), "GenericError")]
[JsonDerivedType(typeof(AlreadyExists), "AlreadyExists")]
[JsonDerivedType(typeof(AlreadyResolved), "AlreadyResolved")]
[JsonDerivedType(typeof(AlreadyAssigned), "AlreadyAssigned")]
public abstract record InvalidDataConstraint
{
    private InvalidDataConstraint() { }

    public sealed record MinLength(int Value) : InvalidDataConstraint;
    public sealed record MaxLength(int Value) : InvalidDataConstraint;
    public sealed record MinValue(object Value) : InvalidDataConstraint;
    public sealed record MaxValue(object Value) : InvalidDataConstraint;

    public sealed record EnumNotDefined : InvalidDataConstraint;
    public sealed record IdNotFound : InvalidDataConstraint;
    public sealed record FormatInvalid : InvalidDataConstraint;
    public sealed record CanNotBeNull : InvalidDataConstraint;
    public sealed record CanNotBeEmptyOrNull : InvalidDataConstraint;
    public sealed record StrictlyPositive : InvalidDataConstraint;
    public sealed record GenericError : InvalidDataConstraint;
    public sealed record AlreadyExists : InvalidDataConstraint;
    public sealed record AlreadyResolved : InvalidDataConstraint;
    public sealed record AlreadyAssigned : InvalidDataConstraint;
}

public record InvalidData
{
    public required InvalidDataConstraint Constraint { get; init; }

    public required string? FieldName { get; init; }

    public object? InnerValue { get; init; }

    public string ClientMessage()
    {
        return Constraint switch
        {
            InvalidDataConstraint.MinLength c => string.Format(CultureInfo.CurrentCulture, Strings.MinLength, FieldName, c.Value),
            InvalidDataConstraint.MaxLength c => string.Format(CultureInfo.CurrentCulture, Strings.MaxLength, FieldName, c.Value),
            InvalidDataConstraint.MinValue c => string.Format(CultureInfo.CurrentCulture, Strings.MinValue, FieldName, c.Value),
            InvalidDataConstraint.MaxValue c => string.Format(CultureInfo.CurrentCulture, Strings.MaxValue, FieldName, c.Value),
            InvalidDataConstraint.EnumNotDefined => string.Format(CultureInfo.CurrentCulture, Strings.EnumNotDefined, FieldName),
            InvalidDataConstraint.IdNotFound => string.Format(CultureInfo.CurrentCulture, Strings.IdNotFound, FieldName),
            InvalidDataConstraint.FormatInvalid => string.Format(CultureInfo.CurrentCulture, Strings.FormatInvalid, FieldName),
            InvalidDataConstraint.CanNotBeNull => string.Format(CultureInfo.CurrentCulture, Strings.CanNotBeNull, FieldName),
            InvalidDataConstraint.CanNotBeEmptyOrNull => string.Format(CultureInfo.CurrentCulture, Strings.CanNotBeEmptyOrNull, FieldName),
            InvalidDataConstraint.StrictlyPositive => string.Format(CultureInfo.CurrentCulture, Strings.StrictlyPositive, FieldName),
            InvalidDataConstraint.GenericError => string.Format(CultureInfo.CurrentCulture, Strings.GenericError, FieldName),
            InvalidDataConstraint.AlreadyExists => string.Format(CultureInfo.CurrentCulture, Strings.AlreadyExists, FieldName),
            InvalidDataConstraint.AlreadyResolved => string.Format(CultureInfo.CurrentCulture, Strings.AlreadyResolved, FieldName),
            InvalidDataConstraint.AlreadyAssigned => string.Format(CultureInfo.CurrentCulture, Strings.AlreadyAssigned, FieldName),
            _ => string.Format(CultureInfo.CurrentCulture, Strings.GenericError, FieldName),
        };
    }
}
