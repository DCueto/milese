using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Milese.Data.Db;

namespace Milese.Api.Rest.Tests.Http;

internal sealed class MileseWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly IDbContextFactory<MileseDbContext> testDbContextFactory;

    public MileseWebApplicationFactory(IDbContextFactory<MileseDbContext> testDbContextFactory) =>
        this.testDbContextFactory = testDbContextFactory;

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IDbContextFactory<MileseDbContext>>();
            services.AddSingleton(testDbContextFactory);
        });
}
