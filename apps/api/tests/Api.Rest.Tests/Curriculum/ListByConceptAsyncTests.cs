using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Milese.Api.Rest.Curriculum;
using Milese.Common.Types.Entities.Curriculum;
using Milese.Tests.Integration;

namespace Milese.Api.Rest.Tests.Curriculum;

public sealed class ListByConceptAsyncTests : DatabaseIntegrationTest
{
    [Test]
    public async Task Returns_ok_with_the_concepts_lessons()
    {
        var conceptId = await LessonsArrange.InsertConceptAsync(DbContextFactory, "Hash Tables");
        var controller = LessonsArrange.BuildController(DbContextFactory);

        await controller.CreateAsync(new CreateLessonRequest
        {
            ConceptId = conceptId.Value,
            Title = "Lesson A",
            EstimatedMinutes = 5,
            Order = 1,
        });

        var response = await controller.ListByConceptAsync(conceptId.Value);

        var ok = (OkObjectResult)response;
        var lessons = (IReadOnlyCollection<LessonBo>)ok.Value!;
        await Assert.That(lessons.Count).IsStrictlyEqualTo(1);
    }

    [Test]
    public async Task Returns_400_when_the_concept_id_is_not_strictly_positive()
    {
        var controller = LessonsArrange.BuildController(DbContextFactory);

        var response = await controller.ListByConceptAsync(0);

        var problem = (ObjectResult)response;
        await Assert.That(problem.StatusCode).IsEqualTo(400);
    }
}
