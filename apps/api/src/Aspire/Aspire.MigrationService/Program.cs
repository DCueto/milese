using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Milese.Aspire.MigrationService;
using Milese.Aspire.ServiceDefaults;
using Milese.Data.Db;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddDbContext<MileseDbContext>(options => options
    .UseNpgsql(builder.Configuration.GetConnectionString("milesedb"))
    .UseSnakeCaseNamingConvention());

builder.Services.AddHostedService<MigrationWorker>();

var host = builder.Build();
await host.RunAsync();
