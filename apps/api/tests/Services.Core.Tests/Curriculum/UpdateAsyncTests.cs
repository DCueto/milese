using System.Threading.Tasks;
using Milese.Tests.Integration;
using Milese.Common.Types.Entities.Curriculum;
using Milese.Common.Types.ValueTypes.Curriculum;

namespace Milese.Services.Core.Tests.Curriculum;

public sealed class UpdateAsyncTests : DatabaseIntegrationTest
{
    [Test]
    public async Task Persists_changes_to_an_existing_lesson()
    {
        var conceptId = await LessonsArrange.InsertConceptAsync(DbContextFactory, "Hash Tables");
        var updateService = LessonsArrange.BuildUpdateService(DbContextFactory);
        var readService = LessonsArrange.BuildReadService(DbContextFactory);

        var created = await updateService.CreateAsync(
            conceptId, new LessonTitle { Value = "Draft" }, new EstimatedMinutes { Value = 5 }, new SortOrder { Value = 1 });

        var updated = new LessonBo
        {
            Id = created.Id,
            ConceptId = created.ConceptId,
            Title = new LessonTitle { Value = "Final" },
            EstimatedMinutes = created.EstimatedMinutes,
            Order = created.Order,
        };
        await updateService.UpdateAsync(updated);

        var result = await readService.GetByIdAsync(created.Id);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Value.Title.Value).IsStrictlyEqualTo("Final");
    }
}
