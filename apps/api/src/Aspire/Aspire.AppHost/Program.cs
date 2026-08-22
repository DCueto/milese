using System.Globalization;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Milese.Aspire.AppHost.Instances;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = DistributedApplication.CreateBuilder(args);

var checkout = CheckoutLocator.Resolve(builder.AppHostDirectory);
var slot = InstanceSlotResolver.Resolve(checkout, PortAvailability.IsSlotFree);

var postgresPassword = builder.AddParameter("postgres-password", secret: true);

var postgresPort = int.Parse(builder.Configuration["Milese:PostgresPort"] ?? "15432", CultureInfo.InvariantCulture);

#pragma warning disable ASPIREPERSISTENCE001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
var postgres = builder
    .AddPostgres("postgres", password: postgresPassword, port: postgresPort)
    .WithContainerName("milese-postgres")
    .WithDataVolume("milese-postgres-data")
    .WithEndpointProxySupport(false)
    .WithPersistentLifetime();
#pragma warning restore ASPIREPERSISTENCE001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

var databaseName = builder.Configuration["Milese:DatabaseName"] ?? slot.DatabaseName;

var milesedb = postgres.AddDatabase("milesedb", databaseName: databaseName);

var migrations = builder
    .AddProject<Projects.Aspire_MigrationService>("migrations")
    .WithReference(milesedb)
    .WaitFor(milesedb);

var portOffset = builder.Configuration["Milese:PortOffset"] is string configuredOffset
    ? int.Parse(configuredOffset, CultureInfo.InvariantCulture)
    : slot.PortOffset;

builder
    .AddProject<Projects.Api_Rest>("api")
    .WithReference(milesedb)
    .WaitFor(migrations)
    .WithEndpoint("http", endpoint => SetPort(endpoint, InstancePorts.ApiHttp(portOffset)));

var application = builder.Build();

var logger = application.Services.GetRequiredService<ILogger<Program>>();

logger.LogInformation(
    "Milese instance: checkout {Checkout}, slot {Slot}, database {Database}, api http://localhost:{ApiPort}",
    checkout.Name,
    slot.Index,
    databaseName,
    InstancePorts.ApiHttp(portOffset)
);

await application.RunAsync();

static void SetPort(EndpointAnnotation endpoint, int port) => endpoint.Port = port;
