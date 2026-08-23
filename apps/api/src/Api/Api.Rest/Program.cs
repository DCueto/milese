using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using Milese.Api.Rest.Extensions;
using Milese.Api.Rest.Identity;
using Milese.Aspire.ServiceDefaults;
using Milese.Data.Db;
using Scalar.AspNetCore;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddServiceDefaults();

    builder.Host.UseSerilog((context, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration));

    builder.Services.AddControllers();
    builder.Services.AddOpenApi();

    var isDevelopmentEnvironment = builder.Environment.IsDevelopment();

    var defaultAuthenticationScheme = isDevelopmentEnvironment
        ? DevOrEntraAuthenticationScheme.Name
        : JwtBearerDefaults.AuthenticationScheme;

    builder.Services
        .AddAuthentication(defaultAuthenticationScheme)
        .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("EntraExternalId"));

    builder.Services.Configure<JwtBearerOptions>(
        JwtBearerDefaults.AuthenticationScheme, options => options.MapInboundClaims = false);

    if (isDevelopmentEnvironment)
    {
        builder.Services
            .AddAuthentication()
            .AddJwtBearer(DevTokenAuthentication.SchemeName, options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = DevTokenAuthentication.Issuer,
                    ValidateAudience = true,
                    ValidAudience = DevTokenAuthentication.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = DevTokenAuthentication.SigningKey,
                    ValidateLifetime = true,
                };
            })
            .AddPolicyScheme(DevOrEntraAuthenticationScheme.Name, DevOrEntraAuthenticationScheme.Name, options =>
                options.ForwardDefaultSelector = DevOrEntraAuthenticationScheme.SelectScheme);
    }

    builder.Services.AddAuthorization(options =>
        options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build());

    builder.AddNpgsqlDbContext<MileseDbContext>(
        "milesedb",
        configureDbContextOptions: options => options
            .UseSnakeCaseNamingConvention()
            .UseValidationCheckConstraints()
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .ConfigureMilese());

    builder.Services.AddDbContextFactory<MileseDbContext>(lifetime: ServiceLifetime.Scoped);

    builder.Services.AddMileseDataAccessAndServices();

    var app = builder.Build();

    app.MapDefaultEndpoints();

    app.UseAuthentication();
    app.UseMiddleware<UserProvisioningMiddleware>();
    app.UseAuthorization();

    app.MapControllers();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi().AllowAnonymous();
        app.MapScalarApiReference().AllowAnonymous();
        app.MapDevTokenEndpoint();
    }

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Milese.Api.Rest terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}

#pragma warning disable S2094, S1118 // Empty marker type — makes the top-level-statements Program class
// accessible to WebApplicationFactory<Program> in Api.Rest.Tests. No runtime behavior change.
public sealed partial class Program;
#pragma warning restore S2094, S1118
