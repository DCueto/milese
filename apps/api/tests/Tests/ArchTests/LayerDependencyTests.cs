using System.Linq;
using System.Reflection;
using TestResult = NetArchTest.Rules.TestResult;

namespace Milese.Tests.ArchTests;

public class LayerDependencyTests
{
    private static readonly Assembly CommonSharedAssembly =
        typeof(Common.Shared.InvalidData).Assembly;

    private static readonly Assembly CommonServerAssembly =
        typeof(Common.Server.QueryableFilterExtensions).Assembly;

    private static readonly Assembly CommonTypesAssembly =
        typeof(Common.Types.Entities.Curriculum.LessonBo).Assembly;

    private static readonly Assembly DataDbAssembly =
        typeof(Data.Db.MileseDbContext).Assembly;

    private static readonly Assembly DataDbAccessAssembly =
        typeof(Data.DbAccess.Curriculum.LessonMappers).Assembly;

    private static readonly Assembly ServicesCoreAssembly =
        typeof(Services.Core.Curriculum.LessonsReadService).Assembly;

    private static readonly Assembly ApiRestAssembly =
        typeof(Api.Rest.Curriculum.LessonsController).Assembly;

    private static readonly string CommonServerNamespace = typeof(Common.Server.QueryableFilterExtensions).Namespace!;
    private static readonly string CommonTypesNamespace = typeof(Common.Types.Entities.Curriculum.LessonBo).Namespace!;
    private static readonly string DataDbNamespace = typeof(Data.Db.MileseDbContext).Namespace!;
    private static readonly string DataDbAccessNamespace = typeof(Data.DbAccess.Curriculum.LessonMappers).Namespace!;
    private static readonly string ServicesCoreNamespace = typeof(Services.Core.Curriculum.LessonsReadService).Namespace!;

    [Test]
    public void Common_Shared_Should_Not_Reference_Anything_Else_In_The_Solution()
    {
        var result = NetArchTest
            .Rules.Types.InAssembly(CommonSharedAssembly)
            .Should()
            .NotHaveDependencyOnAny(
                CommonServerNamespace, CommonTypesNamespace, DataDbNamespace,
                DataDbAccessNamespace, ServicesCoreNamespace, "Milese.Api.Rest")
            .GetResult();

        result
            .IsSuccessful.ShouldBeTrue("Common.Shared must have zero dependencies on any other Milese project — "
                + $"it's the zero-dependency base every layer sits on. Offending types: {FormatFailingTypes(result)}"
            );
    }

    [Test]
    public void Common_Server_Should_Only_Depend_On_Common_Shared()
    {
        var result = NetArchTest
            .Rules.Types.InAssembly(CommonServerAssembly)
            .Should()
            .NotHaveDependencyOnAny(CommonTypesNamespace, DataDbNamespace, DataDbAccessNamespace, ServicesCoreNamespace, "Milese.Api.Rest")
            .GetResult();

        result
            .IsSuccessful.ShouldBeTrue("Common.Server is EF Core-only glue over Common.Shared — "
                + $"it must not depend on Common.Types or anything above it. Offending types: {FormatFailingTypes(result)}"
            );
    }

    [Test]
    public void Common_Types_Should_Not_Reference_Data_Or_Services_Layers()
    {
        var result = NetArchTest
            .Rules.Types.InAssembly(CommonTypesAssembly)
            .Should()
            .NotHaveDependencyOnAny(DataDbNamespace, DataDbAccessNamespace, ServicesCoreNamespace)
            .GetResult();

        result
            .IsSuccessful.ShouldBeTrue("Common.Types (Value Types + Bo records) must not depend on Data.Db, "
                + $"Data.DbAccess, or Services.Core. Offending types: {FormatFailingTypes(result)}"
            );
    }

    [Test]
    public void Data_Db_Should_Not_Reference_DataAccess_Or_Services_Layers()
    {
        var result = NetArchTest
            .Rules.Types.InAssembly(DataDbAssembly)
            .Should()
            .NotHaveDependencyOnAny(DataDbAccessNamespace, ServicesCoreNamespace)
            .GetResult();

        result
            .IsSuccessful.ShouldBeTrue($"Data.Db must not depend on Data.DbAccess or Services.Core. "
                + $"Offending types: {FormatFailingTypes(result)}"
            );
    }

    [Test]
    public void DataAccess_Should_Not_Reference_Services_Layer()
    {
        var result = NetArchTest
            .Rules.Types.InAssembly(DataDbAccessAssembly)
            .Should()
            .NotHaveDependencyOn(ServicesCoreNamespace)
            .GetResult();

        result
            .IsSuccessful.ShouldBeTrue($"Data.DbAccess must not depend on Services.Core. "
                + $"Offending types: {FormatFailingTypes(result)}"
            );
    }

    [Test]
    public void Services_Should_Not_Reference_Data_Db_Or_EntityFrameworkCore()
    {
        var result = NetArchTest
            .Rules.Types.InAssembly(ServicesCoreAssembly)
            .Should()
            .NotHaveDependencyOnAny(DataDbNamespace, "Microsoft.EntityFrameworkCore")
            .GetResult();

        result
            .IsSuccessful.ShouldBeTrue("Services.Core must never touch Data.Db types or EF Core directly — "
                + $"it only sees Bo's returned by Data.DbAccess. Offending types: {FormatFailingTypes(result)}"
            );
    }

    [Test]
    public void Api_Controllers_Should_Not_Reference_Data_Db_Or_DataAccess_Layers()
    {
        // Program.cs (top-level statements compile to a global-namespace "Program" class) and
        // Milese.Api.Rest.Extensions are the composition root — they need Data.Db to register
        // MileseDbContext and Data.DbAccess as reflection scan markers for
        // AddMileseDataAccessAndServices. See CLAUDE.md, Architecture rule 4. Everything else in
        // Api.Rest (controllers, in particular) must go through Services.Core only.
        var result = NetArchTest
            .Rules.Types.InAssembly(ApiRestAssembly)
            .That()
            .DoNotResideInNamespace("Milese.Api.Rest.Extensions")
            .And()
            .DoNotHaveName("Program")
            .Should()
            .NotHaveDependencyOnAny(DataDbNamespace, DataDbAccessNamespace)
            .GetResult();

        result
            .IsSuccessful.ShouldBeTrue("Api controllers must go through Services.Core only — Data.Db/"
                + "Data.DbAccess access is reserved for the Milese.Api.Rest.Extensions composition root. "
                + $"Offending types: {FormatFailingTypes(result)}"
            );
    }

    private static string FormatFailingTypes(TestResult result)
    {
        if (result.FailingTypes is null || !result.FailingTypes.Any())
            return "none";

        return string.Join(", ", result.FailingTypes.Select(t => t.FullName));
    }
}
