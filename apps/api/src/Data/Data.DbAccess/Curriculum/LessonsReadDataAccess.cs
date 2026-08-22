using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Milese.Common.Types.Entities.Curriculum;
using Milese.Common.Types.ValueTypes.Identity;
using Milese.Data.Db;

namespace Milese.Data.DbAccess.Curriculum;

public sealed class LessonsReadDataAccess
{
    private readonly IDbContextFactory<MileseDbContext> dbCntxFactory;
    private readonly CancellationToken cancellationToken;

    public LessonsReadDataAccess(
        IDbContextFactory<MileseDbContext> dbCntxFactory,
        CancellationToken cancellationToken)
    {
        this.dbCntxFactory = dbCntxFactory;
        this.cancellationToken = cancellationToken;
    }

    public async Task<LessonBo?> TryGetByIdAsync(LessonId id)
    {
        await using var ctx = await dbCntxFactory.CreateDbContextAsync(cancellationToken);

        var db = await ctx.Lessons.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        return db?.ToBo();
    }

    public async Task<IReadOnlyCollection<LessonBo>> ListByConceptAsync(ConceptId conceptId)
    {
        await using var ctx = await dbCntxFactory.CreateDbContextAsync(cancellationToken);

        var items = await ctx.Lessons
            .Where(x => x.ConceptId == conceptId)
            .OrderBy(x => x.Order)
            .ToListAsync(cancellationToken);

        return items.ConvertAll(x => x.ToBo());
    }
}
