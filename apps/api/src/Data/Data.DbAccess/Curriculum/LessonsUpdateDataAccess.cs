using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Milese.Common.Types.Entities.Curriculum;
using Milese.Common.Types.ValueTypes.Curriculum;
using Milese.Common.Types.ValueTypes.Identity;
using Milese.Data.Db;
using Milese.Data.Db.Curriculum;

namespace Milese.Data.DbAccess.Curriculum;

public sealed class LessonsUpdateDataAccess
{
    private readonly IDbContextFactory<MileseDbContext> dbCntxFactory;
    private readonly CancellationToken cancellationToken;

    public LessonsUpdateDataAccess(
        IDbContextFactory<MileseDbContext> dbCntxFactory,
        CancellationToken cancellationToken)
    {
        this.dbCntxFactory = dbCntxFactory;
        this.cancellationToken = cancellationToken;
    }

    public async Task<LessonBo> CreateAsync(
        ConceptId conceptId,
        LessonTitle title,
        EstimatedMinutes estimatedMinutes,
        SortOrder order)
    {
        await using var ctx = await dbCntxFactory.CreateDbContextAsync(cancellationToken);

        var db = new LessonDb
        {
            ConceptId = conceptId,
            Title = title,
            EstimatedMinutes = estimatedMinutes,
            Order = order,
        };

        await ctx.Lessons.AddAsync(db, cancellationToken);
        await ctx.SaveChangesAsync(cancellationToken);

        return db.ToBo();
    }

    public async Task UpdateAsync(LessonBo lesson)
    {
        await using var ctx = await dbCntxFactory.CreateDbContextAsync(cancellationToken);

        var db = await ctx.Lessons
            .AsTracking()
            .SingleAsync(x => x.Id == lesson.Id, cancellationToken);

        db.Title = lesson.Title;
        db.EstimatedMinutes = lesson.EstimatedMinutes;
        db.Order = lesson.Order;

        await ctx.SaveChangesAsync(cancellationToken);
    }
}
