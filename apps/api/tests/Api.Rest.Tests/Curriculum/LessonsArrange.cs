using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Milese.Api.Rest.Curriculum;
using Milese.Common.Types.ValueTypes.Curriculum;
using Milese.Common.Types.ValueTypes.Identity;
using Milese.Data.Db;
using Milese.Data.Db.Curriculum;
using Milese.Data.DbAccess.Curriculum;
using Milese.Services.Core.Curriculum;

namespace Milese.Api.Rest.Tests.Curriculum;

internal static class LessonsArrange
{
    public static LessonsController BuildController(IDbContextFactory<MileseDbContext> dbContextFactory)
    {
        var readService = new LessonsReadService(new LessonsReadDataAccess(dbContextFactory, CancellationToken.None));
        var updateService = new LessonsUpdateService(new LessonsUpdateDataAccess(dbContextFactory, CancellationToken.None));

        return new LessonsController(readService, updateService);
    }

    public static async Task<ConceptId> InsertConceptAsync(
        IDbContextFactory<MileseDbContext> dbContextFactory,
        string conceptTitle,
        CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var track = new TrackDb
        {
            Title = new TrackTitle { Value = "Foundations" },
            Order = new SortOrder { Value = 1 },
        };
        await context.Tracks.AddAsync(track, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        var subject = new SubjectDb
        {
            TrackId = track.Id,
            Title = new SubjectTitle { Value = "Data Structures & Algorithms" },
            Order = new SortOrder { Value = 1 },
        };
        await context.Subjects.AddAsync(subject, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        var concept = new ConceptDb
        {
            SubjectId = subject.Id,
            Title = new ConceptTitle { Value = conceptTitle },
            Order = new SortOrder { Value = 1 },
        };
        await context.Concepts.AddAsync(concept, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return concept.Id;
    }
}
