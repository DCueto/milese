using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Milese.Api.Rest.Extensions;
using Milese.Data.Db;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration));

    builder.Services.AddControllers();

    var connectionString = builder.Configuration.GetConnectionString("milese")
        ?? throw new InvalidOperationException("Missing 'milese' connection string.");

    builder.Services.AddDbContextFactory<MileseDbContext>(options => options
        .UseNpgsql(connectionString)
        .UseSnakeCaseNamingConvention()
        .UseValidationCheckConstraints()
        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

    builder.Services.AddMileseDataAccessAndServices();

    var app = builder.Build();

    app.MapControllers();

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
