using System.Threading.Tasks;

namespace Milese.Api.Rest.Tests.Http;

public sealed class TestAuthHandlerTests : HttpIntegrationTest
{
    [Test]
    public async Task Succeeds_with_the_default_EntraObjectId_claim_when_none_is_configured()
    {
        var result = await AuthenticateAsync();

        await Assert.That(result.Succeeded).IsTrue();
        await Assert.That(result.Principal!.FindFirst("oid")!.Value)
            .IsStrictlyEqualTo(TestAuthHandlerOptions.DefaultEntraObjectId);
    }

    [Test]
    public async Task Succeeds_with_a_caller_configured_EntraObjectId_claim()
    {
        const string entraObjectId = "22222222-2222-2222-2222-222222222222";

        var result = await AuthenticateAsync(entraObjectId);

        await Assert.That(result.Succeeded).IsTrue();
        await Assert.That(result.Principal!.FindFirst("oid")!.Value).IsStrictlyEqualTo(entraObjectId);
    }
}
