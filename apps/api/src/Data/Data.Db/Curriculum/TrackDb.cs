using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Milese.Common.Types.ValueTypes.Curriculum;
using Milese.Common.Types.ValueTypes.Identity;

namespace Milese.Data.Db.Curriculum;

[Table("tracks")]
public sealed class TrackDb
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public TrackId Id { get; set; } = null!;

    public required TrackTitle Title { get; set; }

    public required SortOrder Order { get; set; }
}
