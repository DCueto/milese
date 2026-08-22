using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Milese.Common.Shared;
using Milese.Common.Types.ValueTypes.Identity;

namespace Milese.Common.Types.Tests.ValueTypes.Identity;

/// <summary>
/// Convention tests that discover, by reflection, every value type implementing
/// <see cref="IIdValueType{TSelf}"/> and thoroughly validate the common contract of
/// <c>FieldName</c>, <c>Parse</c>, and <c>ParseAsync</c>, so a newly-added Id type is covered
/// without needing a dedicated test.
/// </summary>
public class IdValueTypeConventionTests
{
    public static IEnumerable<Type> IdTypes() =>
        typeof(TrackId).Assembly
            .GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false } && ImplementsIIdValueType(t))
            .OrderBy(t => t.FullName, StringComparer.Ordinal)
            .ToList();

    [Test]
    public void There_Should_Be_At_Least_One_Id_Type() =>
        IdTypes().ShouldNotBeEmpty("the reflection-based test only makes sense if Id types are discovered");

    [Test]
    [MethodDataSource(nameof(IdTypes))]
    public async Task IdValueType_Should_Honor_Contract(Type idType)
    {
        var runner = typeof(IdValueTypeConventionTests)
            .GetMethod(nameof(RunChecksAsync), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(idType);

        await (Task)runner.Invoke(null, null)!;
    }

    private static async Task RunChecksAsync<T>()
        where T : IIdValueType<T>, new()
    {
        var name = typeof(T).Name;

        T.FieldName.ShouldNotBeNullOrWhiteSpace($"{name}.FieldName must be defined");

#pragma warning disable VSTHRD103 // Parse is a synchronous value type method, not an async operation.
        foreach (var invalid in new[] { 0, -1, int.MinValue })
        {
            var result = T.Parse(invalid);
            result.IsSuccess.ShouldBeFalse($"{name}.Parse({invalid}) should fail");
        }

        foreach (var valid in new[] { 1, 42, int.MaxValue })
        {
            var result = T.Parse(valid);
            result.IsSuccess.ShouldBeTrue($"{name}.Parse({valid}) should succeed");
            result.Value.Value.ShouldBe(valid, $"{name}.Parse({valid}) must preserve the underlying value");
        }
#pragma warning restore VSTHRD103

        var existenceChecked = false;
        var parseFailure = await T.ParseAsync(0, _ =>
        {
            existenceChecked = true;
            return Task.FromResult(true);
        });
        parseFailure.IsSuccess.ShouldBeFalse($"{name}.ParseAsync(0) should fail at parse");
        existenceChecked.ShouldBeFalse($"{name}.ParseAsync must not check existence if parse fails");

        var notFound = await T.ParseAsync(1, _ => Task.FromResult(false));
        notFound.IsSuccess.ShouldBeFalse($"{name}.ParseAsync should fail when not found");

        int? checkedValue = null;
        var found = await T.ParseAsync(7, value =>
        {
            checkedValue = value.Value;
            return Task.FromResult(true);
        });
        found.IsSuccess.ShouldBeTrue($"{name}.ParseAsync should succeed when found");
        found.Value.Value.ShouldBe(7, $"{name}.ParseAsync must preserve the underlying value");
        checkedValue.ShouldBe(7, $"{name}.ParseAsync must pass the parsed value to the existence checker");
    }

    private static bool ImplementsIIdValueType(Type type) =>
        type.GetInterfaces().Any(i =>
            i.IsGenericType &&
            i.GetGenericTypeDefinition() == typeof(IIdValueType<>) &&
            i.GetGenericArguments()[0] == type);
}
