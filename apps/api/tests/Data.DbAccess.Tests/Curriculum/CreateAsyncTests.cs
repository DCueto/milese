using System.Threading;
using System.Threading.Tasks;
using Milese.Common.Types.ValueTypes.Curriculum;
using Milese.Data.DbAccess.Curriculum;
using Milese.Tests.Integration;

namespace Milese.Data.DbAccess.Tests.Curriculum;

public sealed class CreateAsyncTests : DatabaseIntegrationTest
{
    [Test]
    public async Task Creates_a_lesson_and_assigns_it_a_positive_id()
    {
        var conceptId = await LessonsArrange.InsertConceptAsync(DbContextFactory, "Hash Tables");
        var updateDataAccess = new LessonsUpdateDataAccess(DbContextFactory, CancellationToken.None);

        var lesson = await updateDataAccess.CreateAsync(
            conceptId,
            new LessonTitle { Value = "Collision Resolution" },
            new EstimatedMinutes { Value = 12 },
            new SortOrder { Value = 2 });

        await Assert.That(lesson.Id.Value).IsGreaterThan(0);
        await Assert.That(lesson.ConceptId).IsStrictlyEqualTo(conceptId);
        await Assert.That(lesson.Title.Value).IsStrictlyEqualTo("Collision Resolution");
        await Assert.That(lesson.EstimatedMinutes.Value).IsStrictlyEqualTo(12);
        await Assert.That(lesson.Order.Value).IsStrictlyEqualTo(2);
    }

    [Test]
    public async Task Created_lesson_is_readable_afterward()
    {
        var conceptId = await LessonsArrange.InsertConceptAsync(DbContextFactory, "Hash Tables");
        var updateDataAccess = new LessonsUpdateDataAccess(DbContextFactory, CancellationToken.None);
        var readDataAccess = new LessonsReadDataAccess(DbContextFactory, CancellationToken.None);

        var created = await updateDataAccess.CreateAsync(
            conceptId,
            new LessonTitle { Value = "Load Factor" },
            new EstimatedMinutes { Value = 8 },
            new SortOrder { Value = 3 });

        var fetched = await readDataAccess.TryGetByIdAsync(created.Id);

        await Assert.That(fetched).IsNotNull();
        await Assert.That(fetched!.Title.Value).IsStrictlyEqualTo("Load Factor");
    }
}
