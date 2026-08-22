using System;
using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Milese.Data.Db;
using Milese.Data.DbAccess.Curriculum;
using Milese.Services.Core.Curriculum;

namespace Milese.Api.Rest.Extensions;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddMileseDataAccessAndServices(this IServiceCollection services)
    {
        AddDataAccess(services);
        AddReadAndUpdateServices(services);
        return services;
    }

    private static void AddDataAccess(IServiceCollection services) =>
        typeof(LessonsReadDataAccess).Assembly
            .GetTypes()
            .Where(t => t.IsClass &&
                        !t.IsAbstract &&
                        (t.Name.EndsWith("ReadDataAccess") || t.Name.EndsWith("UpdateDataAccess")))
            .ToList()
            .ForEach(type => services.AddScoped(type, sp =>
            {
                var factory = sp.GetRequiredService<IDbContextFactory<MileseDbContext>>();
                var cancellationToken = sp.GetService<IHostApplicationLifetime>()?.ApplicationStopping
                                         ?? CancellationToken.None;

                return ActivatorUtilities.CreateInstance(sp, type, factory, cancellationToken);
            }));

    private static void AddReadAndUpdateServices(IServiceCollection services) =>
        typeof(LessonsReadService).Assembly
            .GetTypes()
            .Where(t => t.IsClass &&
                        !t.IsAbstract &&
                        (t.Name.EndsWith("ReadService") || t.Name.EndsWith("UpdateService")))
            .ToList()
            .ForEach(type => services.AddScoped(type));
}
