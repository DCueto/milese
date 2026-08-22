using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Milese.Common.Types.ValueTypes.Curriculum;
using Milese.Common.Types.ValueTypes.Identity;

namespace Milese.Data.Db.Curriculum;

[Table("lessons", Schema = "curriculum")]
public sealed class LessonDb
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public LessonId Id { get; set; }

    public required ConceptId ConceptId { get; set; }

    public required LessonTitle Title { get; set; }

    public required EstimatedMinutes EstimatedMinutes { get; set; }

    public required SortOrder Order { get; set; }

    public ConceptDb? Concept { get; set; }
}
