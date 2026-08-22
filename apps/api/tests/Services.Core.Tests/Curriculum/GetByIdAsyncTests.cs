using System.Threading.Tasks;
using Milese.Common.Shared;
using Milese.Common.Types.ValueTypes.Curriculum;
using Milese.Common.Types.ValueTypes.Identity;
using Milese.Tests.Integration;

namespace Milese.Services.Core.Tests.Curriculum;

public sealed class GetByIdAsyncTests : DatabaseIntegrationTest
{
    [Test]
    public async Task Returns_success_when_the_lesson_exists()
    {
        var conceptId = await LessonsArrange.InsertConceptAsync(DbContextFactory, "Hash Tables");
        var updateService = LessonsArrange.BuildUpdateService(DbContextFactory);
        var readService = LessonsArrange.BuildReadService(DbContextFactory);

        var lesson = await updateService.CreateAsync(
            conceptId,
            new LessonTitle { Value = "Collision Resolution" },
            new EstimatedMinutes { Value = 10 },
            new SortOrder { Value = 1 });

        var result = await readService.GetByIdAsync(lesson.Id);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Value.Title.Value).IsStrictlyEqualTo("Collision Resolution");
    }

    [Test]
    public async Task Returns_failure_with_IdNotFound_when_the_lesson_does_not_exist()
    {
        var readService = LessonsArrange.BuildReadService(DbContextFactory);

        var result = await readService.GetByIdAsync(new LessonId { Value = 999_999 });

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Error.Constraint).IsTypeOf<InvalidDataConstraint.IdNotFound>();
    }
}
