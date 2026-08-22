using System.Threading.Tasks;
using Milese.Tests.Integration;
using Milese.Common.Types.ValueTypes.Curriculum;

namespace Milese.Services.Core.Tests.Curriculum;

public sealed class CreateAsyncTests : DatabaseIntegrationTest
{
    [Test]
    public async Task Creates_a_lesson_with_a_positive_id()
    {
        var conceptId = await LessonsArrange.InsertConceptAsync(DbContextFactory, "Hash Tables");
        var updateService = LessonsArrange.BuildUpdateService(DbContextFactory);

        var lesson = await updateService.CreateAsync(
            conceptId,
            new LessonTitle { Value = "Load Factor" },
            new EstimatedMinutes { Value = 8 },
            new SortOrder { Value = 1 });

        await Assert.That(lesson.Id.Value).IsGreaterThan(0);
        await Assert.That(lesson.ConceptId).IsStrictlyEqualTo(conceptId);
    }
}
