using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Milese.Api.Rest.Extensions;
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
    app.MapControllers();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
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
