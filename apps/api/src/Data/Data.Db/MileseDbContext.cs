using Microsoft.EntityFrameworkCore;
using Milese.Common.Server;
using Milese.Common.Types.ValueTypes.Identity;
using Milese.Data.Db.Curriculum;
using Milese.Data.Db.Identity;

namespace Milese.Data.Db;

public sealed class MileseDbContext : DbContext
{
    public MileseDbContext(DbContextOptions<MileseDbContext> options)
        : base(options)
    {
    }

    public DbSet<TrackDb> Tracks => Set<TrackDb>();

    public DbSet<SubjectDb> Subjects => Set<SubjectDb>();

    public DbSet<ConceptDb> Concepts => Set<ConceptDb>();

    public DbSet<LessonDb> Lessons => Set<LessonDb>();

    public DbSet<UserDb> Users => Set<UserDb>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.RegisterValueTypeConverters(typeof(TrackId).Assembly);
    }
}

public static class MileseDbContextOptionsBuilderExtensions
{
    public static TBuilder ConfigureMilese<TBuilder>(this TBuilder optionsBuilder)
        where TBuilder : DbContextOptionsBuilder
    {
        optionsBuilder.UseValueTypeTranslation();
        return optionsBuilder;
    }
}
