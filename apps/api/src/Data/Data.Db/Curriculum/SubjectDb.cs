using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Milese.Common.Types.ValueTypes.Curriculum;
using Milese.Common.Types.ValueTypes.Identity;

namespace Milese.Data.Db.Curriculum;

[Table("subjects")]
public sealed class SubjectDb
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public SubjectId Id { get; set; } = null!;

    public required TrackId TrackId { get; set; }

    public required SubjectTitle Title { get; set; }

    public required SortOrder Order { get; set; }

    public TrackDb? Track { get; set; }
}
