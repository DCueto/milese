using System.Threading.Tasks;
using Milese.Tests.Integration;
using Milese.Common.Types.ValueTypes.Curriculum;

namespace Milese.Services.Core.Tests.Curriculum;

public sealed class ListByConceptAsyncTests : DatabaseIntegrationTest
{
    [Test]
    public async Task Returns_every_lesson_created_for_the_concept()
    {
        var conceptId = await LessonsArrange.InsertConceptAsync(DbContextFactory, "Hash Tables");
        var updateService = LessonsArrange.BuildUpdateService(DbContextFactory);
        var readService = LessonsArrange.BuildReadService(DbContextFactory);

        await updateService.CreateAsync(
            conceptId, new LessonTitle { Value = "Lesson A" }, new EstimatedMinutes { Value = 5 }, new SortOrder { Value = 1 });
        await updateService.CreateAsync(
            conceptId, new LessonTitle { Value = "Lesson B" }, new EstimatedMinutes { Value = 5 }, new SortOrder { Value = 2 });

        var lessons = await readService.ListByConceptAsync(conceptId);

        await Assert.That(lessons.Count).IsStrictlyEqualTo(2);
    }
}
