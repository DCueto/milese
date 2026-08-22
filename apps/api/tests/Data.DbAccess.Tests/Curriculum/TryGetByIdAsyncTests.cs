using System.Threading;
using System.Threading.Tasks;
using Milese.Common.Types.ValueTypes.Identity;
using Milese.Data.DbAccess.Curriculum;
using Milese.Tests.Integration;

namespace Milese.Data.DbAccess.Tests.Curriculum;

public sealed class TryGetByIdAsyncTests : DatabaseIntegrationTest
{
    [Test]
    public async Task Returns_the_lesson_when_it_exists()
    {
        var conceptId = await LessonsArrange.InsertConceptAsync(DbContextFactory, "Hash Tables");
        var lessonId = await LessonsArrange.InsertLessonAsync(DbContextFactory, conceptId, "Why O(1) Lookups Work");
        var readDataAccess = new LessonsReadDataAccess(DbContextFactory, CancellationToken.None);

        var lesson = await readDataAccess.TryGetByIdAsync(lessonId);

        await Assert.That(lesson).IsNotNull();
        await Assert.That(lesson!.Id).IsStrictlyEqualTo(lessonId);
        await Assert.That(lesson.Title.Value).IsStrictlyEqualTo("Why O(1) Lookups Work");
    }

    [Test]
    public async Task Returns_null_when_the_lesson_does_not_exist()
    {
        var readDataAccess = new LessonsReadDataAccess(DbContextFactory, CancellationToken.None);

        var lesson = await readDataAccess.TryGetByIdAsync(new LessonId { Value = 999_999 });

        await Assert.That(lesson).IsNull();
    }
}
