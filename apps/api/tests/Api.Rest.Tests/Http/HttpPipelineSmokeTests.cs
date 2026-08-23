using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Milese.Api.Rest.Curriculum;
using Milese.Api.Rest.Tests.Curriculum;

namespace Milese.Api.Rest.Tests.Http;

public sealed class HttpPipelineSmokeTests : HttpIntegrationTest
{
    [Test]
    public async Task A_request_through_the_real_pipeline_with_TestAuthHandler_substituted_reaches_the_controller_and_succeeds()
    {
        var conceptId = await LessonsArrange.InsertConceptAsync(DbContextFactory, "Hash Tables");
        var client = CreateAuthenticatedClient();

        var createResponse = await client.PostAsJsonAsync("/api/lessons", new CreateLessonRequest
        {
            ConceptId = conceptId.Value,
            Title = "Collision Resolution",
            EstimatedMinutes = 10,
            Order = 1,
        });
        await Assert.That(createResponse.StatusCode).IsStrictlyEqualTo(HttpStatusCode.Created);

        using var created = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        var lessonId = created.RootElement.GetProperty("id").GetProperty("value").GetInt32();

        var getResponse = await client.GetAsync($"/api/lessons/{lessonId}");
        await Assert.That(getResponse.StatusCode).IsStrictlyEqualTo(HttpStatusCode.OK);

        using var fetched = JsonDocument.Parse(await getResponse.Content.ReadAsStringAsync());
        await Assert.That(fetched.RootElement.GetProperty("title").GetProperty("value").GetString())
            .IsStrictlyEqualTo("Collision Resolution");
    }
}
