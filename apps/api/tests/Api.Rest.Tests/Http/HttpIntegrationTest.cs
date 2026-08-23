using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Milese.Tests.Integration;

namespace Milese.Api.Rest.Tests.Http;

public abstract class HttpIntegrationTest : DatabaseIntegrationTest
{
    private readonly List<WebApplicationFactory<Program>> derivedFactories = [];

    private MileseWebApplicationFactory? factory;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        factory = new MileseWebApplicationFactory(DbContextFactory);
    }

    protected HttpClient CreateAuthenticatedClient(string entraObjectId = TestAuthHandlerOptions.DefaultEntraObjectId) =>
        BuildAuthenticatedFactory(entraObjectId).CreateClient();

    protected async Task<AuthenticateResult> AuthenticateAsync(
        string entraObjectId = TestAuthHandlerOptions.DefaultEntraObjectId)
    {
        using var scope = BuildAuthenticatedFactory(entraObjectId).Services.CreateScope();
        var context = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
        return await context.AuthenticateAsync(TestAuthHandler.SchemeName);
    }

    protected HttpClient CreateClient() => RequireFactory().CreateClient();

    protected HttpClient CreateClientWithFixedJwtSigningKey(string issuer, string audience, SecurityKey signingKey)
    {
        var derivedFactory = RequireFactory().WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Configuration = new OpenIdConnectConfiguration { Issuer = issuer };
                options.TokenValidationParameters.ValidateIssuer = false;
                options.TokenValidationParameters.ValidAudience = audience;
                options.TokenValidationParameters.IssuerSigningKey = signingKey;
            })));
        derivedFactories.Add(derivedFactory);
        return derivedFactory.CreateClient();
    }

    public override async ValueTask DisposeAsync()
    {
        foreach (var derivedFactory in derivedFactories)
            await derivedFactory.DisposeAsync();

        if (factory is not null)
            await factory.DisposeAsync();

        await base.DisposeAsync();
    }

    private WebApplicationFactory<Program> BuildAuthenticatedFactory(string entraObjectId)
    {
        var derivedFactory = RequireFactory().WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            services
                .AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<TestAuthHandlerOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName,
                    options => options.EntraObjectId = entraObjectId)));
        derivedFactories.Add(derivedFactory);
        return derivedFactory;
    }

    private MileseWebApplicationFactory RequireFactory() =>
        factory ?? throw new InvalidOperationException("The web application factory has not been initialized yet.");
}
