using System.Threading;
using System.Threading.Tasks;
using Milese.Common.Types.ValueTypes.Identity;
using Milese.Data.DbAccess.Curriculum;
using Milese.Tests.Integration;

namespace Milese.Data.DbAccess.Tests.Curriculum;

public sealed class ListByConceptAsyncTests : DatabaseIntegrationTest
{
    [Test]
    public async Task Returns_only_lessons_for_the_given_concept()
    {
        var conceptId = await LessonsArrange.InsertConceptAsync(DbContextFactory, "Hash Tables");
        var otherConceptId = await LessonsArrange.InsertConceptAsync(DbContextFactory, "Binary Trees");

        await LessonsArrange.InsertLessonAsync(DbContextFactory, conceptId, "Lesson A");
        await LessonsArrange.InsertLessonAsync(DbContextFactory, conceptId, "Lesson B");
        await LessonsArrange.InsertLessonAsync(DbContextFactory, otherConceptId, "Lesson C");

        var readDataAccess = new LessonsReadDataAccess(DbContextFactory, CancellationToken.None);

        var lessons = await readDataAccess.ListByConceptAsync(conceptId);

        await Assert.That(lessons.Count).IsStrictlyEqualTo(2);
    }

    [Test]
    public async Task Returns_an_empty_collection_when_the_concept_has_no_lessons()
    {
        var conceptId = await LessonsArrange.InsertConceptAsync(DbContextFactory, "Hash Tables");
        var readDataAccess = new LessonsReadDataAccess(DbContextFactory, CancellationToken.None);

        var lessons = await readDataAccess.ListByConceptAsync(conceptId);

        await Assert.That(lessons.Count).IsStrictlyEqualTo(0);
    }
}
