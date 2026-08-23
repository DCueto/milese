using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Milese.Api.Rest.Tests.Http;

public sealed class AuthorizationTests : HttpIntegrationTest
{
    [Test]
    public async Task An_authenticated_request_to_LessonsController_succeeds()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync("/api/lessons?conceptId=1");

        await Assert.That(response.StatusCode).IsStrictlyEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task An_unauthenticated_request_to_LessonsController_is_rejected_with_401()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/api/lessons?conceptId=1");

        await Assert.That(response.StatusCode).IsStrictlyEqualTo(HttpStatusCode.Unauthorized);
    }
}
