using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Milese.Api.Rest.Curriculum;
using Milese.Common.Types.Entities.Curriculum;
using Milese.Tests.Integration;

namespace Milese.Api.Rest.Tests.Curriculum;

public sealed class GetByIdAsyncTests : DatabaseIntegrationTest
{
    [Test]
    public async Task Returns_ok_with_the_lesson_when_it_exists()
    {
        var conceptId = await LessonsArrange.InsertConceptAsync(DbContextFactory, "Hash Tables");
        var controller = LessonsArrange.BuildController(DbContextFactory);

        var created = (CreatedAtActionResult)await controller.CreateAsync(new CreateLessonRequest
        {
            ConceptId = conceptId.Value,
            Title = "Collision Resolution",
            EstimatedMinutes = 10,
            Order = 1,
        });
        var createdLesson = (LessonBo)created.Value!;

        var response = await controller.GetByIdAsync(createdLesson.Id.Value);

        var ok = (OkObjectResult)response;
        await Assert.That(((LessonBo)ok.Value!).Title.Value).IsStrictlyEqualTo("Collision Resolution");
    }

    [Test]
    public async Task Returns_404_when_the_lesson_does_not_exist()
    {
        var controller = LessonsArrange.BuildController(DbContextFactory);

        var response = await controller.GetByIdAsync(999_999);

        var problem = (ObjectResult)response;
        await Assert.That(problem.StatusCode).IsEqualTo(404);
    }

    [Test]
    public async Task Returns_400_when_the_id_is_not_strictly_positive()
    {
        var controller = LessonsArrange.BuildController(DbContextFactory);

        var response = await controller.GetByIdAsync(0);

        var problem = (ObjectResult)response;
        await Assert.That(problem.StatusCode).IsEqualTo(400);
    }
}
