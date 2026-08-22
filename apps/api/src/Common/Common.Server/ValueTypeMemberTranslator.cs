using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Milese.Common.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Milese.Common.Server;

/// <summary>
/// EF Core member translator that lets server-side execution reach the
/// <see cref="IValueType{TSelf, T}.Value"/> property of a value type. Because the
/// column is persisted via <see cref="ValueTypeConverter{TValueType, T}"/> using its
/// underlying value, accessing <c>.Value</c> is equivalent to the column itself, so
/// it's translated by reinterpreting it as the underlying type. Thanks to this,
/// operations like <c>property.Value.Contains(text)</c> translate to a SQL
/// <c>LIKE</c> instead of being evaluated client-side.
/// </summary>
public sealed class ValueTypeMemberTranslator : IMemberTranslator
{
    private const string ValueMemberName = "Value";

    private readonly ISqlExpressionFactory sqlExpressionFactory;
    private readonly IRelationalTypeMappingSource typeMappingSource;

    /// <summary>
    /// Initializes a new instance of the translator.
    /// </summary>
    /// <param name="sqlExpressionFactory">EF Core's SQL expression factory.</param>
    /// <param name="typeMappingSource">Source of relational type mappings.</param>
    public ValueTypeMemberTranslator(
        ISqlExpressionFactory sqlExpressionFactory,
        IRelationalTypeMappingSource typeMappingSource)
    {
        this.sqlExpressionFactory = sqlExpressionFactory;
        this.typeMappingSource = typeMappingSource;
    }

    /// <inheritdoc />
    public SqlExpression? Translate(
        SqlExpression? instance,
        MemberInfo member,
        Type returnType,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (instance is null || member.Name != ValueMemberName || !ImplementsValueType(instance.Type))
            return null;

        var underlyingTypeMapping = typeMappingSource.FindMapping(returnType) ?? instance.TypeMapping;

        return sqlExpressionFactory.MakeUnary(
            ExpressionType.Convert,
            instance,
            returnType,
            underlyingTypeMapping);
    }

    private static bool ImplementsValueType(Type type) =>
        type.GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValueType<,>));
}
