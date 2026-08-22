using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Milese.Api.Rest.Curriculum;
using Milese.Common.Types.Entities.Curriculum;
using Milese.Tests.Integration;

namespace Milese.Api.Rest.Tests.Curriculum;

public sealed class CreateAsyncTests : DatabaseIntegrationTest
{
    [Test]
    public async Task Returns_created_with_the_new_lesson()
    {
        var conceptId = await LessonsArrange.InsertConceptAsync(DbContextFactory, "Hash Tables");
        var controller = LessonsArrange.BuildController(DbContextFactory);

        var response = await controller.CreateAsync(new CreateLessonRequest
        {
            ConceptId = conceptId.Value,
            Title = "Collision Resolution",
            EstimatedMinutes = 10,
            Order = 1,
        });

        var created = (CreatedAtActionResult)response;
        var lesson = (LessonBo)created.Value!;

        await Assert.That(lesson.Id.Value).IsGreaterThan(0);
        await Assert.That(lesson.Title.Value).IsStrictlyEqualTo("Collision Resolution");
    }

    [Test]
    public async Task Returns_400_when_the_title_is_empty()
    {
        var conceptId = await LessonsArrange.InsertConceptAsync(DbContextFactory, "Hash Tables");
        var controller = LessonsArrange.BuildController(DbContextFactory);

        var response = await controller.CreateAsync(new CreateLessonRequest
        {
            ConceptId = conceptId.Value,
            Title = "",
            EstimatedMinutes = 10,
            Order = 1,
        });

        var problem = (ObjectResult)response;
        await Assert.That(problem.StatusCode).IsEqualTo(400);
    }

    [Test]
    public async Task Returns_400_when_estimated_minutes_is_out_of_range()
    {
        var conceptId = await LessonsArrange.InsertConceptAsync(DbContextFactory, "Hash Tables");
        var controller = LessonsArrange.BuildController(DbContextFactory);

        var response = await controller.CreateAsync(new CreateLessonRequest
        {
            ConceptId = conceptId.Value,
            Title = "Collision Resolution",
            EstimatedMinutes = 99,
            Order = 1,
        });

        var problem = (ObjectResult)response;
        await Assert.That(problem.StatusCode).IsEqualTo(400);
    }
}
