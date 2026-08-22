using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var postgresPassword = builder.AddParameter("postgres-password", secret: true);

var postgres = builder
    .AddPostgres("postgres", password: postgresPassword)
    .WithDataVolume("milese-postgres-data");

var milesedb = postgres.AddDatabase("milesedb");

var migrations = builder
    .AddProject<Projects.Aspire_MigrationService>("migrations")
    .WithReference(milesedb)
    .WaitFor(milesedb);

builder
    .AddProject<Projects.Api_Rest>("api")
    .WithReference(milesedb)
    .WaitFor(migrations);

var application = builder.Build();

await application.RunAsync();
