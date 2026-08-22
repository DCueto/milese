using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Milese.Common.Shared;
using Microsoft.EntityFrameworkCore;

namespace Milese.Common.Server;

/// <summary>
/// Extension methods to register <see cref="ValueTypeConverter{TValueType, T}"/>
/// instances in the EF Core model. Registering the conversion by convention means
/// any entity property whose type is a value type implementing
/// <see cref="IValueType{TSelf, T}"/> is automatically converted to/from its
/// underlying value with no per-property configuration. For string value types
/// (<see cref="IStringValueType{TSelf}"/>) the column's maximum length is also
/// applied automatically, and for numeric ones (<see cref="INumericValueType{TSelf}"/>)
/// the precision and scale.
/// </summary>
public static class ValueTypeModelConfigurationExtensions
{
    private static readonly MethodInfo ConfigureStringValueTypeMethod =
        typeof(ValueTypeModelConfigurationExtensions)
            .GetMethod(nameof(ConfigureStringValueType), BindingFlags.Public | BindingFlags.Static)!;

    private static readonly MethodInfo ConfigureNumericValueTypeMethod =
        typeof(ValueTypeModelConfigurationExtensions)
            .GetMethod(nameof(ConfigureNumericValueType), BindingFlags.Public | BindingFlags.Static)!;

    private static readonly MethodInfo ConfigureValueTypeMethod =
        typeof(ValueTypeModelConfigurationExtensions)
            .GetMethod(nameof(ConfigureValueType), BindingFlags.Public | BindingFlags.Static)!;

    /// <summary>
    /// Registers, by convention, the converter for a concrete value type whose
    /// underlying value is of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="TValueType">The value type to convert.</typeparam>
    /// <typeparam name="T">The underlying value's type.</typeparam>
    /// <param name="configurationBuilder">The model conventions builder.</param>
    /// <returns>The same <paramref name="configurationBuilder"/>, for chaining.</returns>
    public static ModelConfigurationBuilder ConfigureValueType<TValueType, T>(
        this ModelConfigurationBuilder configurationBuilder)
        where TValueType : IValueType<TValueType, T>, new()
    {
        ArgumentNullException.ThrowIfNull(configurationBuilder);

        configurationBuilder
            .Properties<TValueType>()
            .HaveConversion<ValueTypeConverter<TValueType, T>>();

        return configurationBuilder;
    }

    /// <summary>
    /// Registers, by convention, the converter for a string value type and
    /// automatically applies the maximum length declared on
    /// <see cref="IStringValueType{TSelf}.MaxLength"/>, avoiding the need to
    /// annotate every property with <c>[MaxLength]</c>.
    /// </summary>
    /// <typeparam name="TValueType">The string value type to convert.</typeparam>
    /// <param name="configurationBuilder">The model conventions builder.</param>
    /// <returns>The same <paramref name="configurationBuilder"/>, for chaining.</returns>
    public static ModelConfigurationBuilder ConfigureStringValueType<TValueType>(
        this ModelConfigurationBuilder configurationBuilder)
        where TValueType : IStringValueType<TValueType>, new()
    {
        ArgumentNullException.ThrowIfNull(configurationBuilder);

        var maxLength = TValueType.MaxLength;

        configurationBuilder
            .Properties<TValueType>()
            .HaveConversion<ValueTypeConverter<TValueType, string>>()
            .HaveMaxLength(maxLength);

        return configurationBuilder;
    }

    /// <summary>
    /// Registers, by convention, the converter for a numeric value type and
    /// automatically applies the precision (total precision and scale) declared
    /// on <see cref="INumericValueType{TSelf}.Precision"/>, avoiding the need to
    /// annotate every property with <c>[Column(TypeName = "decimal(p,s)")]</c>.
    /// </summary>
    /// <typeparam name="TValueType">The numeric value type to convert.</typeparam>
    /// <param name="configurationBuilder">The model conventions builder.</param>
    /// <returns>The same <paramref name="configurationBuilder"/>, for chaining.</returns>
    public static ModelConfigurationBuilder ConfigureNumericValueType<TValueType>(
        this ModelConfigurationBuilder configurationBuilder)
        where TValueType : INumericValueType<TValueType>, new()
    {
        ArgumentNullException.ThrowIfNull(configurationBuilder);

        // Precision is an instance property; instantiate with the parameterless constructor to read it.
        var (precision, scale) = new TValueType().Precision;

        configurationBuilder
            .Properties<TValueType>()
            .HaveConversion<ValueTypeConverter<TValueType, decimal>>()
            .HavePrecision(precision, scale);

        return configurationBuilder;
    }

    /// <summary>
    /// Discovers, in the given assemblies, every type implementing
    /// <see cref="IValueType{TSelf, T}"/> and registers, by convention, the
    /// matching <see cref="ValueTypeConverter{TValueType, T}"/> for each one.
    /// String value types are additionally registered with their automatic
    /// maximum length.
    /// </summary>
    /// <param name="configurationBuilder">The model conventions builder.</param>
    /// <param name="assemblies">The assemblies to search for value types.</param>
    /// <returns>The same <paramref name="configurationBuilder"/>, for chaining.</returns>
    public static ModelConfigurationBuilder RegisterValueTypeConverters(
        this ModelConfigurationBuilder configurationBuilder,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(configurationBuilder);
        ArgumentNullException.ThrowIfNull(assemblies);

        foreach (var (valueType, underlyingType, kind) in assemblies.SelectMany(GetValueTypes))
        {
            switch (kind)
            {
                case ValueTypeKind.String:
                    ConfigureStringValueTypeMethod
                        .MakeGenericMethod(valueType)
                        .Invoke(null, [configurationBuilder]);
                    break;

                case ValueTypeKind.Numeric:
                    ConfigureNumericValueTypeMethod
                        .MakeGenericMethod(valueType)
                        .Invoke(null, [configurationBuilder]);
                    break;

                default:
                    ConfigureValueTypeMethod
                        .MakeGenericMethod(valueType, underlyingType)
                        .Invoke(null, [configurationBuilder]);
                    break;
            }
        }

        return configurationBuilder;
    }

    private static IEnumerable<(Type ValueType, Type UnderlyingType, ValueTypeKind Kind)> GetValueTypes(
        Assembly assembly) =>
        assembly
            .GetTypes()
            .Where(type => type is { IsAbstract: false, IsInterface: false, IsEnum: false })
            .Select(type => (Type: type, Interface: GetValueTypeInterface(type)))
            .Where(candidate => candidate.Interface is not null)
            .Select(candidate =>
            {
                var underlyingType = candidate.Interface!.GetGenericArguments()[1];
                var kind = GetValueTypeKind(candidate.Type, underlyingType);
                return (candidate.Type, underlyingType, kind);
            });

    private static ValueTypeKind GetValueTypeKind(Type type, Type underlyingType)
    {
        if (underlyingType == typeof(string) && ImplementsStringValueType(type))
            return ValueTypeKind.String;

        if (underlyingType == typeof(decimal) && ImplementsNumericValueType(type))
            return ValueTypeKind.Numeric;

        return ValueTypeKind.Plain;
    }

    private static Type? GetValueTypeInterface(Type type) =>
        type.GetInterfaces().FirstOrDefault(i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValueType<,>));

    private static bool ImplementsStringValueType(Type type) =>
        type.GetInterfaces().Any(i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IStringValueType<>));

    private static bool ImplementsNumericValueType(Type type) =>
        type.GetInterfaces().Any(i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INumericValueType<>));

    /// <summary>
    /// Classifies a value type by how its Entity Framework converter must be
    /// configured: <see cref="String"/> also applies the maximum length,
    /// <see cref="Numeric"/> also applies the precision, and <see cref="Plain"/>
    /// only registers the converter.
    /// </summary>
    private enum ValueTypeKind
    {
        Plain,
        String,
        Numeric,
    }
}
