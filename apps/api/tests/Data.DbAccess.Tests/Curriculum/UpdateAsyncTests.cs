using System.Threading;
using System.Threading.Tasks;
using Milese.Common.Types.Entities.Curriculum;
using Milese.Common.Types.ValueTypes.Curriculum;
using Milese.Data.DbAccess.Curriculum;
using Milese.Tests.Integration;

namespace Milese.Data.DbAccess.Tests.Curriculum;

public sealed class UpdateAsyncTests : DatabaseIntegrationTest
{
    [Test]
    public async Task Updates_the_lessons_mutable_fields()
    {
        var conceptId = await LessonsArrange.InsertConceptAsync(DbContextFactory, "Hash Tables");
        var lessonId = await LessonsArrange.InsertLessonAsync(DbContextFactory, conceptId, "Draft Title");

        var readDataAccess = new LessonsReadDataAccess(DbContextFactory, CancellationToken.None);
        var updateDataAccess = new LessonsUpdateDataAccess(DbContextFactory, CancellationToken.None);

        var existing = await readDataAccess.TryGetByIdAsync(lessonId);

        var updated = new LessonBo
        {
            Id = existing!.Id,
            ConceptId = existing.ConceptId,
            Title = new LessonTitle { Value = "Final Title" },
            EstimatedMinutes = new EstimatedMinutes { Value = 14 },
            Order = new SortOrder { Value = 5 },
        };

        await updateDataAccess.UpdateAsync(updated);

        var fetched = await readDataAccess.TryGetByIdAsync(lessonId);

        await Assert.That(fetched).IsNotNull();
        await Assert.That(fetched!.Title.Value).IsStrictlyEqualTo("Final Title");
        await Assert.That(fetched.EstimatedMinutes.Value).IsStrictlyEqualTo(14);
        await Assert.That(fetched.Order.Value).IsStrictlyEqualTo(5);
    }
}
